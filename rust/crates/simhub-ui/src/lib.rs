slint::include_modules!();

pub mod window_state;

use chrono::Local;
use simhub_core::{
    DashboardKind, DashboardSnapshot, EngineGaugeSnapshot, Metric as CoreMetric,
    MetricGroup as CoreMetricGroup, TelemetryPoint,
};
use slint::{Color, ComponentHandle, ModelRc, Timer, TimerMode, VecModel};
use std::error::Error;
use std::sync::{Arc, Mutex};
use std::{fmt::Write as _, time::Duration};

/// `dashboard_kind` value of the waiting screen shown while no game runs.
const IDLE_KIND: i32 = 4;

/// The dashboard window. Lives on the UI thread; other threads reach it
/// through a [`DashboardHandle`].
pub struct DashboardUi {
    window: DashboardWindow,
    _clock: Timer,
    _placement: Option<Timer>,
}

impl DashboardUi {
    pub fn new() -> Result<Self, Box<dyn Error>> {
        let window = DashboardWindow::new()?;
        window.set_dashboard_kind(IDLE_KIND);
        let placement = remember_placement(&window);

        let weak_window = window.as_weak();
        let clock = Timer::default();
        clock.start(TimerMode::Repeated, Duration::from_secs(1), move || {
            if let Some(window) = weak_window.upgrade() {
                window.set_clock_text(Local::now().format("%H:%M:%S").to_string().into());
            }
        });
        window.set_clock_text(Local::now().format("%H:%M:%S").to_string().into());

        Ok(Self {
            window,
            _clock: clock,
            _placement: placement,
        })
    }

    pub fn handle(&self) -> DashboardHandle {
        DashboardHandle {
            window: self.window.as_weak(),
            pending: Arc::new(Mutex::new(Pending::default())),
        }
    }

    /// Shows a snapshot, or the waiting screen for `None`. UI thread only.
    pub fn show(&self, snapshot: Option<DashboardSnapshot>, status: &str) {
        apply(&self.window, snapshot, status);
    }

    /// Runs the event loop until the window is closed.
    pub fn run(self) -> Result<(), Box<dyn Error>> {
        self.window.run()?;
        Ok(())
    }
}

/// How often the window placement is checked and, when it changed, saved.
const PLACEMENT_CHECK: Duration = Duration::from_secs(1);

/// Puts the window back where it was last placed, then keeps saving its
/// placement whenever it changes. Saving as it goes, rather than on close,
/// means arranging the window on a sim rig is remembered without ever closing
/// it, and survives the app being stopped any other way.
///
/// The returned timer does the saving and has to be kept alive.
fn remember_placement(window: &DashboardWindow) -> Option<Timer> {
    let Some(path) = window_state::default_path() else {
        log::warn!("No settings directory; the window position will not be remembered");
        return None;
    };

    let saved = window_state::load(&path);
    if let Some(state) = saved {
        if window_state::on_a_monitor(state.centre()) {
            window
                .window()
                .set_position(slint::PhysicalPosition::new(state.x, state.y));
        } else {
            log::info!("The saved window position is off every monitor; using the default");
        }
        window
            .window()
            .set_size(slint::PhysicalSize::new(state.width, state.height));
        if state.maximized {
            window.window().set_maximized(true);
        }
    }

    let weak = window.as_weak();
    let mut last = saved;
    let timer = Timer::default();
    timer.start(TimerMode::Repeated, PLACEMENT_CHECK, move || {
        let Some(window) = weak.upgrade() else {
            return;
        };
        let window = window.window();
        if !window.is_visible() || window.is_minimized() {
            return;
        }
        let state = window_state::WindowState::capture(
            window.position(),
            window.size(),
            window.is_maximized(),
            last,
        );
        if last == Some(state) {
            return;
        }
        match window_state::save(&path, &state) {
            Ok(()) => last = Some(state),
            Err(error) => log::warn!("Could not save the window position: {error}"),
        }
    });
    Some(timer)
}

/// Sends snapshots to the dashboard from any thread.
///
/// Telemetry arrives far faster than a screen refreshes, so only the newest
/// snapshot is kept and at most one repaint is queued at a time. A slow frame
/// therefore skips stale data instead of building a backlog, which is what the
/// bounded drop-oldest channel did in the C# app.
#[derive(Clone)]
pub struct DashboardHandle {
    window: slint::Weak<DashboardWindow>,
    pending: Arc<Mutex<Pending>>,
}

type Frame = (Option<DashboardSnapshot>, String);

#[derive(Default)]
struct Pending {
    latest: Option<Frame>,
    scheduled: bool,
}

impl DashboardHandle {
    pub fn show(&self, snapshot: Option<DashboardSnapshot>, status: &str) {
        let mut pending = self.pending.lock().unwrap_or_else(|e| e.into_inner());
        pending.latest = Some((snapshot, status.to_owned()));
        if pending.scheduled {
            return;
        }
        pending.scheduled = true;
        drop(pending);

        let shared = Arc::clone(&self.pending);
        let _ = self.window.upgrade_in_event_loop(move |window| {
            let latest = {
                let mut pending = shared.lock().unwrap_or_else(|e| e.into_inner());
                pending.scheduled = false;
                pending.latest.take()
            };
            if let Some((snapshot, status)) = latest {
                apply(&window, snapshot, &status);
            }
        });
    }

    /// Closes the window, ending [`DashboardUi::run`].
    pub fn close(&self) {
        let _ = slint::invoke_from_event_loop(|| {
            let _ = slint::quit_event_loop();
        });
    }
}

/// Shows one fixed snapshot, for the demo mode.
pub fn run(snapshot: DashboardSnapshot) -> Result<(), Box<dyn Error>> {
    let ui = DashboardUi::new()?;
    ui.show(Some(snapshot), "DEMO DATA");
    ui.run()
}

fn apply(window: &DashboardWindow, snapshot: Option<DashboardSnapshot>, status: &str) {
    window.set_status_text(status.into());
    let Some(snapshot) = snapshot else {
        window.set_dashboard_kind(IDLE_KIND);
        window.set_title_text("HaddySimHub".into());
        return;
    };

    let (gauge_ticks, gauge_arc, needle_path) = gauge_visuals(snapshot.engine_gauge.as_ref());
    let (compass_ticks, heading_marker_path, track_marker_path, bug_marker_path) = match snapshot
        .flight
    {
        Some(flight) => heading_compass_visuals(flight.heading, flight.track, flight.heading_bug),
        None => (Vec::new(), String::new(), String::new(), String::new()),
    };
    let attitude = snapshot
        .flight
        .map(|flight| flight_attitude_visuals(flight.pitch, flight.bank, flight.slip))
        .unwrap_or_default();
    let (brake_trace, clutch_trace, throttle_trace) = telemetry_paths(&snapshot.telemetry_trace);

    window.set_title_text(snapshot.title.into());
    window.set_hero_label(snapshot.hero_label.into());
    window.set_hero_value(snapshot.hero_value.into());
    window.set_hero_unit(snapshot.hero_unit.into());
    window.set_hero_detail(snapshot.hero_detail.into());
    window.set_rpm_value(snapshot.rpm_value.into());
    window.set_rpm_detail(snapshot.rpm_detail.into());
    window.set_gear_value(snapshot.gear_value.into());
    window.set_recommended_gear(snapshot.recommended_gear.unwrap_or_default().into());
    window.set_speed_limit(
        snapshot
            .speed_limit
            .map_or_else(|| "--".to_string(), |limit| limit.to_string())
            .into(),
    );
    window.set_compass_ticks(ModelRc::new(VecModel::from(compass_ticks)));
    window.set_heading_marker_path(heading_marker_path.into());
    window.set_track_marker_path(track_marker_path.into());
    window.set_bug_marker_path(bug_marker_path.into());
    window.set_attitude_sky_path(attitude.sky_path.into());
    window.set_attitude_ground_path(attitude.ground_path.into());
    window.set_attitude_horizon_path(attitude.horizon_path.into());
    window.set_attitude_rungs(ModelRc::new(VecModel::from(attitude.rungs)));
    window.set_attitude_labels(ModelRc::new(VecModel::from(attitude.labels)));
    window.set_attitude_roll_ticks(ModelRc::new(VecModel::from(attitude.roll_ticks)));
    window.set_attitude_roll_index_path(attitude.roll_index_path.into());
    window.set_attitude_reference_path(attitude.reference_path.into());
    window.set_attitude_aircraft_path(attitude.aircraft_path.into());
    window.set_attitude_slip_x(attitude.slip_x);
    window.set_needle_path(needle_path.into());
    window.set_gauge_ticks(ModelRc::new(VecModel::from(gauge_ticks)));
    window.set_gauge_arc(ModelRc::new(VecModel::from(gauge_arc)));
    window.set_brake_trace(brake_trace.into());
    window.set_clutch_trace(clutch_trace.into());
    window.set_throttle_trace(throttle_trace.into());
    window.set_progress_pct(snapshot.progress_pct);
    window.set_header_metrics(ModelRc::new(VecModel::from(to_slint_metrics(
        snapshot.header_metrics,
        4,
    ))));
    window.set_groups_top(ModelRc::new(VecModel::from(to_slint_groups(
        snapshot.groups_top,
        4,
    ))));
    window.set_groups_middle(ModelRc::new(VecModel::from(to_slint_groups(
        snapshot.groups_middle,
        3,
    ))));
    window.set_groups_bottom(ModelRc::new(VecModel::from(to_slint_groups(
        snapshot.groups_bottom,
        2,
    ))));
    window.set_navigation_group(to_slint_group(snapshot.navigation_group));
    window.set_footer_metrics(ModelRc::new(VecModel::from(to_slint_metrics(
        snapshot.footer_metrics,
        1,
    ))));
    window.set_pedals(ModelRc::new(VecModel::from(to_slint_metrics(
        snapshot.pedals,
        1,
    ))));
    window.set_dashboard_kind(match snapshot.kind {
        DashboardKind::Race => 0,
        DashboardKind::Rally => 1,
        DashboardKind::Truck => 2,
        DashboardKind::Flight => 3,
    });
}

#[derive(Default)]
struct AttitudeVisuals {
    sky_path: String,
    ground_path: String,
    horizon_path: String,
    rungs: Vec<GaugeTick>,
    labels: Vec<GaugeTick>,
    roll_ticks: Vec<GaugeTick>,
    roll_index_path: String,
    reference_path: String,
    aircraft_path: String,
    slip_x: f32,
}

fn flight_attitude_visuals(pitch: f32, bank: f32, slip: f32) -> AttitudeVisuals {
    fn append_quadratic(
        points: &mut Vec<(f32, f32)>,
        start: (f32, f32),
        control: (f32, f32),
        end: (f32, f32),
    ) {
        for step in 1..=6 {
            let t = step as f32 / 6.0;
            let inverse = 1.0 - t;
            points.push((
                inverse * inverse * start.0 + 2.0 * inverse * t * control.0 + t * t * end.0,
                inverse * inverse * start.1 + 2.0 * inverse * t * control.1 + t * t * end.1,
            ));
        }
    }

    let transform = |x: f32, y: f32| {
        let radians = -bank.to_radians();
        let dx = x - 100.0;
        let dy = y - 100.0 + pitch * 4.2;
        (
            100.0 + dx * radians.cos() - dy * radians.sin(),
            100.0 + dx * radians.sin() + dy * radians.cos(),
        )
    };
    let mut local_horizon = vec![(-400.0, 100.0), (0.0, 100.0), (32.0, 100.0)];
    append_quadratic(
        &mut local_horizon,
        (32.0, 100.0),
        (41.0, 100.0),
        (48.0, 104.0),
    );
    append_quadratic(
        &mut local_horizon,
        (48.0, 104.0),
        (53.0, 107.0),
        (60.0, 107.0),
    );
    local_horizon.push((100.0, 107.0));
    local_horizon.push((140.0, 107.0));
    append_quadratic(
        &mut local_horizon,
        (140.0, 107.0),
        (147.0, 107.0),
        (152.0, 104.0),
    );
    append_quadratic(
        &mut local_horizon,
        (152.0, 104.0),
        (159.0, 100.0),
        (168.0, 100.0),
    );
    local_horizon.push((600.0, 100.0));
    let horizon_points: Vec<_> = local_horizon
        .iter()
        .map(|(x, y)| transform(*x, *y))
        .collect();
    let path_from_points = |points: &[(f32, f32)], close: bool| {
        let mut path = String::new();
        for (index, (x, y)) in points.iter().enumerate() {
            let command = if index == 0 { "M" } else { "L" };
            let _ = write!(path, "{command} {x:.2} {y:.2} ");
        }
        if close {
            path.push('Z');
        }
        path
    };
    let horizon_path = path_from_points(&horizon_points, false);
    let mut sky_points = vec![(-400.0, -400.0), (600.0, -400.0)];
    sky_points.extend(horizon_points.iter().rev().copied());
    let sky_path = path_from_points(&sky_points, true);
    let mut ground_points = horizon_points;
    ground_points.extend([(600.0, 600.0), (-400.0, 600.0)]);
    let ground_path = path_from_points(&ground_points, true);

    let mut rungs = Vec::with_capacity(4);
    let mut labels = Vec::with_capacity(8);
    for degrees in [-10.0_f32, -5.0, 5.0, 10.0] {
        let major = degrees.abs() == 10.0;
        let half_width = if major { 34.0 } else { 18.0 };
        let y = 100.0 - degrees * 4.2;
        let left = transform(100.0 - half_width, y);
        let right = transform(100.0 + half_width, y);
        rungs.push(GaugeTick {
            commands: format!(
                "M {:.2} {:.2} L {:.2} {:.2}",
                left.0, left.1, right.0, right.1
            )
            .into(),
            label: String::new().into(),
            label_x: 0.0,
            label_y: 0.0,
        });

        let label_offset = if major { 45.0 } else { 28.0 };
        for x in [100.0 - label_offset, 100.0 + label_offset] {
            let position = transform(x, y);
            labels.push(GaugeTick {
                commands: String::new().into(),
                label: degrees.abs().to_string().into(),
                label_x: position.0 - 6.0,
                label_y: position.1 - 7.0,
            });
        }
    }

    let roll_ticks = (0..13)
        .map(|index| {
            let angle = (-60.0 + index as f32 * 10.0).to_radians();
            let point = |radius: f32| (100.0 + angle.sin() * radius, 100.0 - angle.cos() * radius);
            let start = point(86.0);
            let end = point(if index % 3 == 0 { 74.0 } else { 79.0 });
            GaugeTick {
                commands: format!(
                    "M {:.2} {:.2} L {:.2} {:.2}",
                    start.0, start.1, end.0, end.1
                )
                .into(),
                label: String::new().into(),
                label_x: 0.0,
                label_y: 0.0,
            }
        })
        .collect();

    AttitudeVisuals {
        sky_path,
        ground_path,
        horizon_path,
        rungs,
        labels,
        roll_ticks,
        roll_index_path: "M 100 9 L 89 22 L 111 22 Z".into(),
        reference_path:
            "M 55 100 L 82 103 L 96 101 L 96 98 L 104 98 L 104 101 L 118 103 L 145 100"
                .into(),
        aircraft_path:
            "M100 90 103 98 119 100 119 103 103 102 103 106 109 110 109 112 100 109 91 112 91 110 97 106 97 102 81 103 81 100 97 98Z"
                .into(),
        slip_x: 100.0 + slip.clamp(-1.0, 1.0) * 22.0,
    }
}

fn heading_compass_visuals(
    heading: f32,
    track: f32,
    bug: Option<f32>,
) -> (Vec<GaugeTick>, String, String, String) {
    let point_at = |bearing: f32, radius: f32| {
        let radians = (bearing - 90.0).to_radians();
        (
            100.0 + radians.cos() * radius,
            100.0 + radians.sin() * radius,
        )
    };
    let heading = heading.rem_euclid(360.0);
    let track = track.rem_euclid(360.0);
    let ticks = (0..72)
        .map(|index| {
            let bearing = index as f32 * 5.0;
            let relative_bearing = bearing - heading;
            let major = index % 2 == 0;
            let (outer_x, outer_y) = point_at(relative_bearing, 84.0);
            let (inner_x, inner_y) = point_at(relative_bearing, if major { 72.0 } else { 79.0 });
            let (label_x, label_y) = point_at(relative_bearing, 60.0);
            let label = if bearing % 90.0 == 0.0 {
                match index {
                    0 => "N".into(),
                    18 => "E".into(),
                    36 => "S".into(),
                    _ => "W".into(),
                }
            } else if bearing % 30.0 == 0.0 {
                format!("{:02}", index / 2)
            } else {
                String::new()
            };

            GaugeTick {
                commands: format!("M {outer_x:.2} {outer_y:.2} L {inner_x:.2} {inner_y:.2}").into(),
                label: label.into(),
                label_x: label_x - 12.0,
                label_y: label_y - 7.0,
            }
        })
        .collect();
    let marker_path = |bearing: f32| {
        let relative_bearing = ((bearing - heading + 540.0) % 360.0) - 180.0;
        let tip = point_at(relative_bearing, 91.0);
        let base_left = point_at(relative_bearing - 4.0, 81.0);
        let base_right = point_at(relative_bearing + 4.0, 81.0);
        format!(
            "M {:.2} {:.2} L {:.2} {:.2} L {:.2} {:.2} Z",
            tip.0, tip.1, base_left.0, base_left.1, base_right.0, base_right.1
        )
    };
    let heading_marker_path = "M 100 16 L 94 32 L 106 32 Z".into();
    let bug_marker_path = bug.map_or_else(String::new, marker_path);
    (
        ticks,
        heading_marker_path,
        marker_path(track),
        bug_marker_path,
    )
}

fn gauge_visuals(
    gauge: Option<&EngineGaugeSnapshot>,
) -> (Vec<GaugeTick>, Vec<GaugeArcSegment>, String) {
    let Some(gauge) = gauge.filter(|gauge| gauge.max.is_finite() && gauge.max > 0.0) else {
        return (Vec::new(), Vec::new(), String::new());
    };

    let center_x = 125.0_f32;
    let center_y = 105.0_f32;
    let angle_at = |fraction: f32| 145.0 + fraction * 250.0;
    let point_at = |fraction: f32, radius: f32| {
        let radians = angle_at(fraction).to_radians();
        (
            center_x + radians.cos() * radius,
            center_y + radians.sin() * radius,
        )
    };
    let rpm_fraction = if gauge.value.is_finite() {
        (gauge.value / gauge.max).clamp(0.0, 1.0)
    } else {
        0.0
    };

    let mut ticks = Vec::with_capacity(11);
    for index in 0..=10 {
        let fraction = index as f32 / 10.0;
        let major = index % 2 == 0;
        let tick_length = if major { 14.0 } else { 8.0 };
        let (outer_x, outer_y) = point_at(fraction, 90.0);
        let (inner_x, inner_y) = point_at(fraction, 90.0 - tick_length);
        let (label_center_x, label_center_y) = point_at(fraction, 62.0);

        ticks.push(GaugeTick {
            commands: format!("M {outer_x:.2} {outer_y:.2} L {inner_x:.2} {inner_y:.2}").into(),
            label: if major {
                ((fraction * gauge.max / 100.0).round() as u32)
                    .to_string()
                    .into()
            } else {
                "".into()
            },
            label_x: label_center_x - 14.0,
            label_y: label_center_y - 8.0,
        });
    }

    let green_start = (gauge.green_start / gauge.max).clamp(0.0, 1.0);
    let green_end = (gauge.green_end / gauge.max).clamp(green_start, 1.0);
    let redline = gauge.redline_fraction.clamp(green_end, 1.0);
    let arc = [
        (0.0, green_start, Color::from_rgb_u8(190, 197, 214)),
        (green_start, green_end, Color::from_rgb_u8(0, 204, 102)),
        (green_end, redline, Color::from_rgb_u8(255, 204, 0)),
        (redline, 1.0, Color::from_rgb_u8(255, 51, 0)),
    ]
    .into_iter()
    .filter(|(start, end, _)| end - start > f32::EPSILON)
    .map(|(start, end, accent)| {
        let start_angle = angle_at(start);
        let end_angle = angle_at(end);
        let start_radians = start_angle.to_radians();
        let end_radians = end_angle.to_radians();
        let radius = 99.0;
        let start_x = center_x + start_radians.cos() * radius;
        let start_y = center_y + start_radians.sin() * radius;
        let end_x = center_x + end_radians.cos() * radius;
        let end_y = center_y + end_radians.sin() * radius;

        GaugeArcSegment {
            commands: format!(
                "M {start_x:.2} {start_y:.2} A {radius:.1} {radius:.1} 0 0 1 {end_x:.2} {end_y:.2}"
            )
            .into(),
            accent,
        }
    })
    .collect();

    let needle_angle = angle_at(rpm_fraction).to_radians();
    let needle_x = center_x + needle_angle.cos() * 68.0;
    let needle_y = center_y + needle_angle.sin() * 68.0;
    let needle_path = format!("M {center_x:.1} {center_y:.1} L {needle_x:.1} {needle_y:.1}");

    (ticks, arc, needle_path)
}

fn telemetry_paths(points: &[TelemetryPoint]) -> (String, String, String) {
    fn path(points: &[TelemetryPoint], value: impl Fn(&TelemetryPoint) -> f32) -> String {
        let mut commands = String::new();
        let last_index = points.len().saturating_sub(1).max(1) as f32;
        for (index, point) in points.iter().enumerate() {
            let x = index as f32 / last_index * 1_000.0;
            let y = 95.0 - value(point).clamp(0.0, 100.0) * 0.9;
            let command = if index == 0 { "M" } else { "L" };
            let _ = write!(commands, "{command} {x:.1} {y:.1} ");
        }
        commands
    }

    (
        path(points, |point| point.brake),
        path(points, |point| point.clutch),
        path(points, |point| point.throttle),
    )
}

fn to_slint_groups(groups: Vec<CoreMetricGroup>, minimum_count: usize) -> Vec<MetricGroup> {
    let mut groups: Vec<MetricGroup> = groups.into_iter().map(to_slint_group).collect();
    while groups.len() < minimum_count {
        groups.push(empty_group());
    }
    groups
}

fn empty_group() -> MetricGroup {
    MetricGroup {
        title: "".into(),
        accent: Color::from_rgb_u8(60, 64, 89),
        metrics: ModelRc::new(VecModel::from(Vec::new())),
    }
}

fn to_slint_group(group: CoreMetricGroup) -> MetricGroup {
    let metrics: Vec<Metric> = group.metrics.into_iter().map(to_slint_metric).collect();
    MetricGroup {
        title: group.title.into(),
        accent: Color::from_rgb_u8(group.accent[0], group.accent[1], group.accent[2]),
        metrics: ModelRc::new(VecModel::from(metrics)),
    }
}

fn to_slint_metrics(metrics: Vec<CoreMetric>, minimum_count: usize) -> Vec<Metric> {
    let mut metrics: Vec<Metric> = metrics.into_iter().map(to_slint_metric).collect();
    while metrics.len() < minimum_count {
        metrics.push(to_slint_metric(CoreMetric {
            label: "".into(),
            value: "".into(),
            delta: None,
            ratio: 0.0,
            active: false,
            warning: false,
            critical: false,
        }));
    }
    metrics
}

fn to_slint_metric(metric: CoreMetric) -> Metric {
    let delta_value = metric
        .delta
        .map_or_else(String::new, |delta| format!("{delta:+.3}"));
    let delta_status = metric.delta.map_or(0, |delta| {
        if delta < 0.0 {
            -1
        } else if delta > 0.0 {
            1
        } else {
            0
        }
    });

    Metric {
        label: metric.label.into(),
        value: metric.value.into(),
        delta_value: delta_value.into(),
        delta_status,
        ratio: metric.ratio,
        active: metric.active,
        warning: metric.warning,
        critical: metric.critical,
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use simhub_core::{DashboardKind, Metric as CoreMetric, demo_dashboard};

    #[test]
    fn race_lap_delta_values_keep_their_sign_and_direction() {
        let make_metric = |delta| CoreMetric {
            label: "Lap".into(),
            value: "01:33.812".into(),
            delta,
            ratio: 0.0,
            active: false,
            warning: false,
            critical: false,
        };

        let faster = to_slint_metric(make_metric(Some(-0.143)));
        let slower = to_slint_metric(make_metric(Some(0.143)));
        let tied = to_slint_metric(make_metric(Some(0.0)));
        let missing = to_slint_metric(make_metric(None));

        assert_eq!(faster.delta_value, "-0.143");
        assert_eq!(faster.delta_status, -1);
        assert_eq!(slower.delta_value, "+0.143");
        assert_eq!(slower.delta_status, 1);
        assert_eq!(tied.delta_value, "+0.000");
        assert_eq!(tied.delta_status, 0);
        assert_eq!(missing.delta_value, "");
        assert_eq!(missing.delta_status, 0);
    }

    #[test]
    fn flight_compass_marks_are_normalized_and_distinguish_heading_from_track() {
        let (ticks, heading_path, track_path, bug_path) = heading_compass_visuals(0.0, 90.0, None);
        let (_, wrapped_heading_path, _, _) = heading_compass_visuals(360.0, 90.0, None);

        assert_eq!(ticks.len(), 72);
        assert_eq!(
            ticks.iter().filter(|tick| !tick.label.is_empty()).count(),
            12
        );
        assert_eq!(heading_path, wrapped_heading_path);
        assert_ne!(heading_path, track_path);
        assert_eq!(heading_path, "M 100 16 L 94 32 L 106 32 Z");
        assert!(track_path.starts_with("M "));
        assert_eq!(bug_path, "");
    }

    #[test]
    fn flight_compass_card_rotates_with_heading() {
        let (ticks, _, _, _) = heading_compass_visuals(90.0, 90.0, None);
        let north = ticks.iter().find(|tick| tick.label == "N").unwrap();

        assert!(north.label_x < 30.0);
        assert!((90.0..=97.0).contains(&north.label_y));
        let (_, _, aligned_track, _) = heading_compass_visuals(90.0, 90.0, None);
        let (_, _, north_track, _) = heading_compass_visuals(0.0, 0.0, None);
        assert_eq!(aligned_track, north_track);
        let (_, _, _, bug_at_sixteen) = heading_compass_visuals(90.0, 0.0, Some(106.0));
        let (_, _, _, bug_relative_to_north) = heading_compass_visuals(0.0, 0.0, Some(16.0));
        assert_eq!(bug_at_sixteen, bug_relative_to_north);
    }

    #[test]
    fn flight_attitude_horizon_moves_with_pitch_and_bank() {
        let level = flight_attitude_visuals(0.0, 0.0, 0.0);
        let pitched = flight_attitude_visuals(5.0, 0.0, 0.0);
        let banked = flight_attitude_visuals(0.0, 30.0, 0.0);

        assert!(level.horizon_path.starts_with("M -400.00 100.00"));
        assert!(level.horizon_path.contains("100.00 107.00"));
        assert!(pitched.horizon_path.contains("100.00 128.00"));
        assert_ne!(banked.horizon_path, level.horizon_path);
        assert_eq!(level.rungs.len(), 4);
        assert_eq!(level.labels.len(), 8);
        assert_eq!(level.roll_ticks.len(), 13);
    }

    #[test]
    fn flight_attitude_slip_ball_stays_within_the_instrument() {
        let left = flight_attitude_visuals(0.0, 0.0, -2.0);
        let centered = flight_attitude_visuals(0.0, 0.0, 0.0);
        let right = flight_attitude_visuals(0.0, 0.0, 2.0);

        assert_eq!(left.slip_x, 78.0);
        assert_eq!(centered.slip_x, 100.0);
        assert_eq!(right.slip_x, 122.0);
    }

    #[test]
    fn rally_split_uses_the_shared_directional_delta_color() {
        let rally = demo_dashboard(DashboardKind::Rally);
        let split = rally
            .header_metrics
            .into_iter()
            .find(|metric| metric.label == "SPLIT")
            .unwrap();
        let split = to_slint_metric(split);

        assert_eq!(split.delta_value, "+1.240");
        assert_eq!(split.delta_status, 1);
    }

    #[test]
    fn truck_gauge_has_ticks_colored_zones_and_live_needle_value() {
        let snapshot = demo_dashboard(DashboardKind::Truck);
        let (ticks, arc, needle_path) = gauge_visuals(snapshot.engine_gauge.as_ref());

        assert_eq!(ticks.len(), 11);
        assert_eq!(
            ticks.iter().filter(|tick| !tick.label.is_empty()).count(),
            6
        );
        assert_eq!(arc.len(), 4);
        assert!(arc.iter().all(|segment| segment.commands.contains(" A ")));
        assert!(ticks.iter().all(|tick| tick.commands.contains(" L ")));
        assert!(needle_path.starts_with("M 125.0 105.0 L "));
    }

    #[test]
    fn missing_or_invalid_gauge_data_does_not_create_marks() {
        assert_eq!(gauge_visuals(None).0.len(), 0);
        let invalid = EngineGaugeSnapshot {
            value: 500.0,
            max: 0.0,
            green_start: 100.0,
            green_end: 200.0,
            redline_fraction: 0.9,
        };
        assert!(gauge_visuals(Some(&invalid)).0.is_empty());
    }

    #[test]
    fn race_telemetry_creates_three_distinct_plot_traces() {
        let snapshot = simhub_core::demo_dashboard(DashboardKind::Race);
        let (brake, clutch, throttle) = telemetry_paths(&snapshot.telemetry_trace);

        assert!(brake.starts_with("M "));
        assert!(clutch.starts_with("M "));
        assert!(throttle.starts_with("M "));
        assert_ne!(brake, clutch);
        assert_ne!(clutch, throttle);
        assert_eq!(
            brake.matches('L').count(),
            snapshot.telemetry_trace.len() - 1
        );
    }
}

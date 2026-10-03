//! Turns converter output into what the dashboard draws.
//!
//! The labels, formatting and warning thresholds follow the web displays this
//! replaces (`ClientApp/src/app/displays/*`), so a driver sees the same values
//! in the same places. The truck dashboard was Dutch there and stays Dutch.

use crate::{
    DashboardKind, DashboardSnapshot, EngineGaugeSnapshot, FlightInstruments, Metric, MetricGroup,
    TelemetryPoint,
};
use simhub_model::{
    CourseDeviationSource, DisplayUpdate, EngineType, FlightData, NavToFrom, RaceData, RallyData,
    TruckData,
};
use std::collections::VecDeque;

/// How many pedal samples the race trace keeps, matching the demo trace.
pub const TRACE_LENGTH: usize = 96;

/// iRacing reports an unlimited session as a week remaining.
const UNLIMITED_SESSION_SECONDS: f32 = 168.0 * 3600.0;

/// State carried from one update to the next. Only the race trace needs any.
#[derive(Default)]
pub struct LiveDashboard {
    trace: VecDeque<TelemetryPoint>,
}

impl LiveDashboard {
    pub fn new() -> Self {
        Self::default()
    }

    /// The snapshot for an update, or `None` when no game is feeding the
    /// dashboard and it should show its idle screen.
    pub fn snapshot(&mut self, update: &DisplayUpdate) -> Option<DashboardSnapshot> {
        match update {
            DisplayUpdate::None => {
                self.trace.clear();
                None
            }
            DisplayUpdate::Race(data) => {
                self.record_pedals(data);
                Some(race(data, self.trace.iter().cloned().collect()))
            }
            DisplayUpdate::Rally(data) => Some(rally(data)),
            DisplayUpdate::Truck(data) => Some(truck(data)),
            DisplayUpdate::Flight(data) => Some(flight(data)),
        }
    }

    fn record_pedals(&mut self, data: &RaceData) {
        if self.trace.len() == TRACE_LENGTH {
            self.trace.pop_front();
        }
        self.trace.push_back(TelemetryPoint {
            brake: data.brake_pct as f32,
            clutch: data.clutch_pct as f32,
            throttle: data.throttle_pct as f32,
        });
    }
}

fn metric(label: &str, value: impl Into<String>) -> Metric {
    Metric {
        label: label.into(),
        value: value.into(),
        delta: None,
        ratio: 0.0,
        active: false,
        warning: false,
        critical: false,
    }
}

fn indicator(label: &str, active: bool) -> Metric {
    Metric {
        active,
        ..metric(label, if active { "ON" } else { "OFF" })
    }
}

fn meter(label: &str, value: impl Into<String>, ratio: f32) -> Metric {
    Metric {
        ratio: if ratio.is_finite() {
            ratio.clamp(0.0, 1.0)
        } else {
            0.0
        },
        ..metric(label, value)
    }
}

fn group(title: &str, accent: [u8; 3], metrics: Vec<Metric>) -> MetricGroup {
    MetricGroup {
        title: title.into(),
        accent,
        metrics,
    }
}

const BLUE: [u8; 3] = [93, 173, 226];
const AMBER: [u8; 3] = [255, 193, 7];

fn empty_snapshot(kind: DashboardKind, title: &str) -> DashboardSnapshot {
    DashboardSnapshot {
        kind,
        title: title.into(),
        header_metrics: vec![],
        progress_pct: 0.0,
        hero_label: "SPEED".into(),
        hero_value: String::new(),
        hero_unit: "km/h".into(),
        hero_detail: String::new(),
        rpm_value: String::new(),
        rpm_detail: String::new(),
        gear_value: String::new(),
        recommended_gear: None,
        speed_limit: None,
        engine_gauge: None,
        flight: None,
        groups_top: vec![],
        groups_middle: vec![],
        groups_bottom: vec![],
        navigation_group: group("", BLUE, vec![]),
        footer_metrics: vec![],
        pedals: vec![],
        telemetry_trace: vec![],
    }
}

// ---------------------------------------------------------------- race

fn race(data: &RaceData, trace: Vec<TelemetryPoint>) -> DashboardSnapshot {
    let mut header = vec![metric("SESSION", data.session_type.clone())];
    if let Some(sof) = data.strength_of_field {
        header.push(metric("SOF", sof.to_string()));
    }
    if let Some(rating) = data.safety_rating {
        header.push(metric("SAFETY RATING", rating.to_string()));
    }
    header.push(metric(
        "TIME LEFT",
        format_time(data.session_time_remaining),
    ));
    header.push(metric("AIR", format!("{}°", fixed(data.air_temp, 1))));
    header.push(metric("TRACK", format!("{}°", fixed(data.track_temp, 1))));
    if let Some(wind) = data.wind_speed {
        header.push(metric("WIND", format!("{} m/s", fixed(wind, 1))));
    }
    if let Some(rain) = data.rain_intensity.filter(|rain| *rain > 0) {
        header.push(metric("RAIN", rain_label(rain)));
    }
    if let Some(grip) = data.track_grip_status {
        header.push(Metric {
            warning: grip == 3,
            ..metric("GRIP", grip_label(grip))
        });
    }
    if data.pit_limiter_on {
        header.push(Metric {
            critical: true,
            ..metric("PIT", "LIMITER")
        });
    }

    let laps = if (data.is_limited_time && !data.is_limited_session_laps) || data.total_laps <= 0 {
        data.current_lap.to_string()
    } else {
        format!("{}/{}", data.current_lap, data.total_laps)
    };

    let mut session = Vec::new();
    if let Some(incidents) = data.incidents {
        let value = match data.max_incidents {
            Some(max) if max != 999 => format!("{incidents}/{max}"),
            _ => incidents.to_string(),
        };
        session.push(metric("Incidents", value));
    }
    if let Some(i_rating) = data.i_rating {
        session.push(metric("iR", format_i_rating(i_rating)));
    }
    if let Some(position) = data.position {
        session.push(metric("Pos", format!("P{position}")));
    }
    if let Some(expected) = &data.expected_position {
        session.push(metric("Expected pos", format!("P{expected}")));
    }
    session.push(metric("Lap", laps));
    if data.current_lap_time != 0.0 {
        session.push(metric(
            "Current lap",
            format_lap_time(data.current_lap_time),
        ));
    }
    session.push(Metric {
        delta: data.last_lap_time_delta,
        ..metric("Last lap", format_lap_time(data.last_lap_time))
    });
    if let Some(best) = data.best_lap_time {
        session.push(Metric {
            delta: data.best_lap_time_delta,
            ..metric("Best lap", format_lap_time(best))
        });
    }

    let mut fuel = Vec::new();
    if let Some(remaining) = data.fuel_remaining {
        fuel.push(metric("Fuel", format!("{} L", fixed(remaining, 1))));
    }
    let laps_left = data.total_laps - data.current_lap;
    fuel.push(Metric {
        critical: data.total_laps > 0 && laps_left as f32 > data.fuel_est_laps,
        ..metric("Est. laps", fixed(data.fuel_est_laps, 1))
    });
    if let Some(last) = data.fuel_last_lap {
        fuel.push(metric("Last lap", format!("{}L", fixed(last, 1))));
    }
    if let Some(avg) = data.fuel_avg_lap {
        fuel.push(metric("Avg lap", format!("{}L", fixed(avg, 1))));
    }
    if let Some(bias) = data.brake_bias {
        fuel.push(metric("Brake bias", format!("{}%", fixed(bias, 1))));
    }

    DashboardSnapshot {
        header_metrics: header,
        hero_value: data.speed.to_string(),
        hero_detail: format!("RPM  {}", data.rpm),
        rpm_value: data.rpm.to_string(),
        rpm_detail: if data.rpm_max > 0 {
            format!("of {} RPM", data.rpm_max)
        } else {
            "RPM".into()
        },
        gear_value: data.gear.clone(),
        groups_top: vec![group("SESSION", BLUE, session), group("FUEL", AMBER, fuel)],
        pedals: vec![
            meter(
                "BRAKE",
                format!("{}%", data.brake_pct),
                data.brake_pct as f32 / 100.0,
            ),
            meter(
                "CLUTCH",
                format!("{}%", data.clutch_pct),
                data.clutch_pct as f32 / 100.0,
            ),
            meter(
                "THROTTLE",
                format!("{}%", data.throttle_pct),
                data.throttle_pct as f32 / 100.0,
            ),
        ],
        telemetry_trace: trace,
        ..empty_snapshot(DashboardKind::Race, "Race")
    }
}

fn rain_label(value: i32) -> &'static str {
    match value {
        3.. => "Heavy",
        2 => "Medium",
        _ => "Light",
    }
}

fn grip_label(value: i32) -> &'static str {
    match value {
        0 => "Green",
        1 => "Fast",
        2 => "Optimum",
        3 => "Wet",
        _ => "Unknown",
    }
}

fn format_i_rating(value: i32) -> String {
    if value < 1_000 {
        value.to_string()
    } else {
        let text = format!("{:.1}", value as f32 / 1_000.0);
        format!("{}k", text.strip_suffix(".0").unwrap_or(&text))
    }
}

// ---------------------------------------------------------------- rally

fn rally(data: &RallyData) -> DashboardSnapshot {
    let header_metrics = vec![
        metric(
            "DISTANCE",
            format!("Distance  {} m", data.distance_travelled),
        ),
        metric("PROGRESS", format!("{}%", data.completed_pct)),
        metric("TIME", format_lap_time(data.lap_time)),
        metric(
            "POSITION",
            if data.position > 0 {
                format!("P{}", data.position)
            } else {
                String::new()
            },
        ),
    ];

    DashboardSnapshot {
        header_metrics,
        progress_pct: (data.completed_pct as f32 / 100.0).clamp(0.0, 1.0),
        hero_value: data.speed.to_string(),
        hero_detail: format!("RPM  {}", data.rpm),
        rpm_value: data.rpm.to_string(),
        rpm_detail: "RPM".into(),
        gear_value: data.gear.clone(),
        groups_bottom: vec![
            group(
                "SECTOR 1",
                BLUE,
                vec![metric("Time", format_lap_time(data.sector1_time))],
            ),
            group(
                "SECTOR 2",
                [125, 211, 192],
                vec![metric("Time", format_lap_time(data.sector2_time))],
            ),
            group(
                "STAGE RUNNING",
                AMBER,
                vec![metric("Time", format_lap_time(data.lap_time))],
            ),
        ],
        ..empty_snapshot(DashboardKind::Rally, "Rally")
    }
}

// ---------------------------------------------------------------- truck

const MINUTES_PER_DAY: i64 = 24 * 60;

fn truck(data: &TruckData) -> DashboardSnapshot {
    let arrival = if data.time_remaining > 0 {
        format!(
            " → aankomst {}",
            clock(data.game_time as i64 + data.time_remaining as i64)
        )
    } else {
        String::new()
    };

    let route = vec![
        metric("Speltijd", clock(data.game_time as i64)),
        metric(
            "Vertrekpunt",
            waypoint(&data.source_city, &data.source_company),
        ),
        metric(
            "Bestemming",
            waypoint(&data.destination_city, &data.destination_company),
        ),
        metric(
            "Resterend",
            if data.time_remaining != 0 {
                format!(
                    "{} / {} km{arrival}",
                    timespan(data.time_remaining as i64),
                    nl(data.distance_remaining)
                )
            } else {
                "-".into()
            },
        ),
        Metric {
            critical: data.time_remaining > 0 && data.rest_time_remaining < data.time_remaining,
            ..metric(
                "Volgend rustmoment",
                if data.rest_time_remaining != 0 {
                    timespan(data.rest_time_remaining as i64)
                } else {
                    "-".into()
                },
            )
        },
    ];

    let job = vec![
        metric("Truck", or_dash(&data.truck_name)),
        metric(
            "Deadline",
            if data.job_time_remaining != 0 {
                timespan(data.job_time_remaining)
            } else {
                "-".into()
            },
        ),
        metric(
            "Inkomen",
            if data.job_income != 0 {
                format!("€ {}", nl(data.job_income as f32))
            } else {
                "-".into()
            },
        ),
        metric(
            "Lading",
            if data.job_cargo_name.is_empty() {
                "-".into()
            } else {
                format!(
                    "{} ({} kg) · {}% schade",
                    data.job_cargo_name,
                    nl(data.job_cargo_mass as f32),
                    data.job_cargo_damage
                )
            },
        ),
    ];

    let mut truck_damage = vec![
        damage("Motor", data.damage_truck_engine),
        damage("Transmissie", data.damage_truck_transmission),
        damage("Cabine", data.damage_truck_cabin),
        damage("Chassis", data.damage_truck_chassis),
        damage("Wielen", data.damage_truck_wheels),
    ];
    truck_damage.push(metric(
        "Kilometerteller",
        if data.odometer > 0.0 {
            format!("{} km", nl(data.odometer))
        } else {
            "-".into()
        },
    ));

    let trailer_damage = if data.number_of_trailers_attached > 0 {
        vec![
            damage("Chassis", data.damage_trailer_chassis),
            damage("Opbouw", data.damage_trailer_body),
            damage("Wielen", data.damage_trailer_wheels),
            damage("Lading", data.damage_trailer_cargo),
        ]
    } else {
        vec![metric("", "Geen trailer gekoppeld")]
    };

    let fuel_ratio = if data.fuel_capacity > 0.0 {
        data.fuel_amount / data.fuel_capacity
    } else {
        0.0
    };

    let gear_advice = (!data.recommended_gear.is_empty() && data.recommended_gear != data.gear)
        .then(|| data.recommended_gear.clone());

    DashboardSnapshot {
        hero_value: data.speed.to_string(),
        hero_detail: if data.cruise_control_on {
            format!("CRUISE  {} km/h", data.cruise_control_speed)
        } else {
            String::new()
        },
        speed_limit: (data.speed_limit > 0).then_some(data.speed_limit as i32),
        rpm_value: data.rpm.to_string(),
        rpm_detail: format!("of {} RPM", data.rpm_max),
        gear_value: data.gear.clone(),
        recommended_gear: gear_advice,
        engine_gauge: Some(EngineGaugeSnapshot {
            value: data.rpm as f32,
            max: data.rpm_max.max(1) as f32,
            green_start: 1_250.0,
            green_end: 2_000.0,
            redline_fraction: 0.85,
        }),
        groups_top: vec![
            group("ROUTE", [75, 165, 255], route),
            group("OPDRACHT", [44, 211, 112], job),
            group("TRUCK SCHADE", [255, 177, 0], truck_damage),
            group("TRAILER SCHADE", [255, 76, 76], trailer_damage),
        ],
        groups_bottom: vec![
            group(
                "BRANDSTOF",
                BLUE,
                vec![
                    Metric {
                        critical: data.fuel_distance < data.distance_remaining,
                        ..meter(
                            "Fuel",
                            format!("{} km ({} l)", nl(data.fuel_distance), nl(data.fuel_amount)),
                            fuel_ratio,
                        )
                    },
                    metric("AdBlue", format!("{} l", nl(data.ad_blue_amount))),
                    metric("Acceleration", format!("{} %", data.throttle)),
                ],
            ),
            group(
                "VOERTUIG",
                BLUE,
                vec![
                    metric("Battery", format!("{} V", nl(data.battery_voltage))),
                    metric("Water", format!("{} °C", nl(data.water_temp))),
                    metric("Oil pressure", format!("{} PSI", nl(data.oil_pressure))),
                    metric("Brake air", format!("{} PSI", nl(data.brake_air_pressure))),
                ],
            ),
        ],
        footer_metrics: vec![
            indicator("PARKING", data.parking_lights_on),
            indicator("LOW BEAM", data.low_beam_on),
            indicator("HIGH BEAM", data.high_beam_on),
            indicator("LEFT", data.blinker_left_on),
            indicator("HAZARD", data.hazard_lights_on),
            indicator("RIGHT", data.blinker_right_on),
            indicator("BRAKE", data.parking_brake_on),
            indicator("WIPERS", data.wipers_on),
            indicator("DIFF LOCK", data.differential_lock),
            indicator("ENGINE", data.engine_on),
            indicator("ENGINE BRAKE", data.motor_brake_on),
            indicator("BEACON", data.beacon_on),
            indicator("LIFT AXLE", data.lift_axle_indicator_on),
            indicator(
                "AIR",
                data.air_pressure_warning_on || data.air_pressure_emergency_on,
            ),
        ],
        ..empty_snapshot(DashboardKind::Truck, "Truck")
    }
}

fn damage(label: &str, value: i32) -> Metric {
    Metric {
        warning: (25..50).contains(&value),
        critical: value >= 50,
        ..metric(label, format!("{value} %"))
    }
}

fn waypoint(city: &str, company: &str) -> String {
    match (city.is_empty(), company.is_empty()) {
        (true, _) => "-".into(),
        (false, true) => city.into(),
        (false, false) => format!("{city} ({company})"),
    }
}

fn or_dash(value: &str) -> String {
    if value.is_empty() {
        "-".into()
    } else {
        value.into()
    }
}

/// Minutes as `h:mm`.
fn timespan(minutes: i64) -> String {
    format!("{}:{:02}", minutes / 60, (minutes % 60).abs())
}

/// Minutes since the start of the game as a time of day.
fn clock(minutes: i64) -> String {
    let of_day = minutes.rem_euclid(MINUTES_PER_DAY);
    format!("{:02}:{:02}", of_day / 60, of_day % 60)
}

// ---------------------------------------------------------------- flight

fn flight(d: &FlightData) -> DashboardSnapshot {
    let has_engine = d.engine_type != EngineType::None && d.engine_count > 0;

    let alert = if d.stall_warning {
        "     STALL"
    } else if d.overspeed_warning {
        "     OVERSPEED"
    } else {
        ""
    };

    let attitude = group(
        "ATTITUDE",
        BLUE,
        vec![
            metric("Pitch", format!("{}°", fixed(d.pitch_degrees, 1))),
            metric("Bank", format!("{}°", fixed(d.bank_degrees, 1))),
            metric("Slip", fixed(d.slip_ball, 2)),
        ],
    );

    let altitude = group(
        "ALTITUDE",
        BLUE,
        vec![
            metric(
                "Indicated",
                format!("{} ft", nl(d.indicated_altitude.round())),
            ),
            metric("AGL", format!("{} ft", nl(d.altitude_above_ground.round()))),
            Metric {
                active: d.vertical_speed > 50.0,
                warning: d.vertical_speed < -50.0,
                ..metric("Vertical speed", format!("{} fpm", en(d.vertical_speed, 0)))
            },
            metric("QNH", format!("{}", d.altimeter_setting_hpa.round())),
        ],
    );

    let heading = group(
        "HEADING",
        BLUE,
        vec![
            metric("Heading", format!("{}°", en(d.heading_magnetic, 0))),
            metric("Track", format!("{}°", en(d.ground_track, 0))),
            metric(
                "Wind",
                format!("{}° / {} kts", en(d.wind_direction, 0), en(d.wind_speed, 0)),
            ),
        ],
    );

    let course = group("COURSE", BLUE, course_metrics(d));

    let modes: Vec<&str> = [
        (d.heading_hold, "HDG"),
        (d.nav_hold, "NAV"),
        (d.approach_hold, "APR"),
        (d.altitude_hold, "ALT"),
        (d.speed_hold, "SPD"),
    ]
    .into_iter()
    .filter_map(|(on, mode)| on.then_some(mode))
    .collect();
    let autopilot = if d.autopilot_master {
        let mut metrics = vec![
            Metric {
                active: true,
                ..metric(
                    "Modes",
                    if modes.is_empty() {
                        "CWS".into()
                    } else {
                        modes.join(" · ")
                    },
                )
            },
            metric("Altitude", format!("{} ft", nl(d.altitude_target.round()))),
            metric("Heading", format!("{}°", en(d.heading_bug, 0))),
        ];
        if d.speed_hold {
            metrics.push(metric("Speed", format!("{} kts", en(d.speed_target, 0))));
        }
        metrics
    } else {
        vec![metric("Modes", "AP OFF")]
    };

    let navigation = if d.has_active_flight_plan {
        let mut metrics = vec![
            metric(
                "Next waypoint",
                d.next_waypoint_id.clone().unwrap_or_default(),
            ),
            metric(
                "Distance",
                format!(
                    "{} NM",
                    en(d.distance_to_waypoint_nm.unwrap_or_default(), 1)
                ),
            ),
            metric("ETE", minutes(d.waypoint_ete_seconds)),
            metric("Destination", d.destination_id.clone().unwrap_or_default()),
            metric(
                "Distance",
                format!(
                    "{} NM",
                    en(d.distance_to_destination_nm.unwrap_or_default(), 0)
                ),
            ),
            metric(
                "Arrival",
                format!(
                    "{} · ETA {}",
                    minutes(d.destination_ete_seconds),
                    d.destination_eta_utc_seconds
                        .map(|eta| format!("{}:{:02}Z", eta / 3600 % 24, eta % 3600 / 60))
                        .unwrap_or_default()
                ),
            ),
        ];
        if let Some(xtk) = d.cross_track_error_nm {
            metrics.push(metric(
                "XTK",
                format!(
                    "{:.2} NM {}",
                    xtk.abs(),
                    if xtk >= 0.0 { "right" } else { "left" }
                ),
            ));
        }
        metrics
    } else {
        vec![metric("", "No active flight plan")]
    };

    let mut engine = Vec::new();
    if has_engine {
        let label = if d.engine_type == EngineType::Piston {
            "% RPM"
        } else {
            "N1"
        };
        let primary = d.engine_primary_pct.unwrap_or_default();
        engine.push(meter(
            label,
            format!("{}%", en(primary, 0)),
            primary / 100.0,
        ));
    }
    let fuel_ratio = if d.fuel_capacity_lbs > 0.0 {
        d.fuel_quantity_lbs / d.fuel_capacity_lbs
    } else {
        0.0
    };
    engine.push(Metric {
        warning: d
            .fuel_endurance_seconds
            .is_some_and(|seconds| seconds < 2_700),
        ..meter(
            "Fuel",
            format!("{} lb", nl(d.fuel_quantity_lbs.round())),
            fuel_ratio,
        )
    });
    if let Some(endurance) = d.fuel_endurance_seconds {
        engine.push(metric(
            "Endurance",
            format!("{}:{:02}", endurance / 3600, endurance % 3600 / 60),
        ));
    }
    if has_engine {
        engine.push(metric(
            "Fuel flow",
            format!("{} pph", en(d.fuel_flow_pph.unwrap_or_default(), 0)),
        ));
        engine.push(metric(
            "Oil",
            format!(
                "{}°C / {} psi",
                en(d.oil_temperature.unwrap_or_default(), 0),
                en(d.oil_pressure.unwrap_or_default(), 0)
            ),
        ));
    }

    let gear = if d.gear_percent_extended >= 99.0 {
        "DOWN"
    } else if d.gear_percent_extended <= 1.0 {
        "UP"
    } else {
        "IN TRANSIT"
    };
    let lights: Vec<&str> = [
        (d.landing_lights_on, "LAND"),
        (d.taxi_lights_on, "TAXI"),
        (d.strobe_lights_on, "STROBE"),
        (d.nav_lights_on, "NAV"),
        (d.beacon_on, "BCN"),
    ]
    .into_iter()
    .filter_map(|(on, name)| on.then_some(name))
    .collect();
    let configuration = vec![
        metric(
            "Flaps",
            format!(
                "{} / {}",
                if d.flaps_handle_index == 0 {
                    "UP".to_string()
                } else {
                    d.flaps_handle_index.to_string()
                },
                (d.flaps_handle_positions - 1).max(0)
            ),
        ),
        Metric {
            active: gear == "DOWN",
            warning: gear == "IN TRANSIT",
            ..metric("Gear", gear)
        },
        Metric {
            active: d.spoilers_pct > 0.0,
            ..metric(
                "Spoilers",
                if d.spoilers_armed && d.spoilers_pct <= 0.0 {
                    "ARMED".to_string()
                } else {
                    format!("{}%", d.spoilers_pct.round())
                },
            )
        },
        Metric {
            active: d.parking_brake_on,
            ..metric("Park brake", if d.parking_brake_on { "SET" } else { "OFF" })
        },
        metric("Trim", format!("{}%", en(d.elevator_trim_pct, 0))),
        metric(
            "Lights",
            if lights.is_empty() {
                "-".into()
            } else {
                lights.join(" · ")
            },
        ),
        metric("Aircraft", d.aircraft_title.clone()),
    ];

    DashboardSnapshot {
        hero_label: "INDICATED AIRSPEED".into(),
        hero_value: en(d.indicated_airspeed, 0),
        hero_unit: "kts".into(),
        hero_detail: format!(
            "GS  {}     TAS  {}{alert}",
            en(d.ground_speed, 0),
            en(d.true_airspeed, 0)
        ),
        flight: Some(FlightInstruments {
            pitch: d.pitch_degrees,
            bank: d.bank_degrees,
            slip: d.slip_ball,
            heading: d.heading_magnetic,
            track: d.ground_track,
            heading_bug: d.autopilot_master.then_some(d.heading_bug),
        }),
        groups_top: vec![attitude, altitude],
        groups_middle: vec![heading, course, group("AUTOPILOT", BLUE, autopilot)],
        groups_bottom: vec![
            group("ENGINE & FUEL", BLUE, engine),
            group("CONFIGURATION", BLUE, configuration),
        ],
        navigation_group: group("NAVIGATION", BLUE, navigation),
        ..empty_snapshot(DashboardKind::Flight, "Flight")
    }
}

fn course_metrics(d: &FlightData) -> Vec<Metric> {
    let source = match d.deviation_source {
        CourseDeviationSource::None => return vec![metric("", "No lateral guidance")],
        CourseDeviationSource::Gps => "GPS",
        CourseDeviationSource::Vor => "VOR",
        CourseDeviationSource::Localizer => "LOC",
    };
    let to_from = match d.to_from {
        NavToFrom::To => " TO",
        NavToFrom::From => " FROM",
        NavToFrom::Off => "",
    };

    let mut metrics = vec![Metric {
        active: true,
        ..metric(
            "Source",
            format!(
                "{source}{}{to_from}",
                d.deviation_source_id
                    .as_deref()
                    .map(|id| format!(" {id}"))
                    .unwrap_or_default()
            ),
        )
    }];
    if let Some(course) = d.selected_course {
        metrics.push(metric("Course", format!("{}°", en(course, 0))));
    }
    if let Some(scale) = d.lateral_full_scale_nm {
        metrics.push(metric("Scale", format!("±{scale} NM")));
    }
    if let Some(lateral) = d.lateral_deviation {
        metrics.push(metric("Deviation", fixed(lateral, 2)));
    }
    if let Some(glideslope) = d.glideslope_deviation {
        metrics.push(metric("Glideslope", fixed(glideslope, 2)));
    }
    metrics
}

fn minutes(seconds: Option<i32>) -> String {
    seconds
        .map(|seconds| format!("{} min", (seconds as f32 / 60.0).round()))
        .unwrap_or_default()
}

// ---------------------------------------------------------------- formatting

/// `mm:ss.fff`, or dashes when there is no time yet.
pub fn format_lap_time(seconds: f32) -> String {
    if seconds == 0.0 || !seconds.is_finite() {
        return "--:--.---".into();
    }
    let millis = (seconds as f64 * 1000.0).round() as i64;
    format!(
        "{:02}:{:02}.{:03}",
        millis / 60_000,
        millis / 1000 % 60,
        millis % 1000
    )
}

/// Session time left as `hh:mm:ss` or `mm:ss`.
pub fn format_time(seconds: f32) -> String {
    if seconds == UNLIMITED_SESSION_SECONDS {
        return "--:--:--".into();
    }
    let sign = if seconds < 0.0 { "-" } else { "" };
    let total = seconds.abs() as i64;
    let (hours, minutes, secs) = (total / 3600, total % 3600 / 60, total % 60);
    if hours > 0 {
        format!("{sign}{hours:02}:{minutes:02}:{secs:02}")
    } else {
        format!("{sign}{minutes:02}:{secs:02}")
    }
}

/// Rounds the way `Intl.NumberFormat` does, half away from zero, where `format!`
/// would round an exact half to even.
fn round_half_away(value: f32, digits: usize) -> f64 {
    let factor = 10f64.powi(digits as i32);
    (f64::from(value) * factor).round() / factor
}

/// A fixed number of decimals, the way `Intl.NumberFormat('en-US')` with equal
/// minimum and maximum fraction digits wrote it, minus the grouping.
fn fixed(value: f32, digits: usize) -> String {
    format!("{:.digits$}", round_half_away(value, digits))
}

/// `en-US` grouping: `24,000.5`.
fn en(value: f32, digits: usize) -> String {
    group_digits(
        &format!("{:.digits$}", round_half_away(value, digits)),
        ',',
        '.',
    )
}

/// `nl-NL` with at most one decimal: `1.234,5`, and `12` rather than `12,0`.
fn nl(value: f32) -> String {
    let text = format!("{:.1}", round_half_away(value, 1));
    let text = text.strip_suffix(".0").unwrap_or(&text);
    group_digits(text, '.', ',')
}

fn group_digits(text: &str, thousands: char, decimal: char) -> String {
    let (sign, unsigned) = match text.strip_prefix('-') {
        Some(rest) => ("-", rest),
        None => ("", text),
    };
    let (whole, fraction) = match unsigned.split_once('.') {
        Some((whole, fraction)) => (whole, Some(fraction)),
        None => (unsigned, None),
    };

    let mut grouped = String::new();
    for (index, digit) in whole.chars().enumerate() {
        if index > 0 && (whole.len() - index) % 3 == 0 {
            grouped.push(thousands);
        }
        grouped.push(digit);
    }

    let mut out = format!("{sign}{grouped}");
    if let Some(fraction) = fraction {
        out.push(decimal);
        out.push_str(fraction);
    }
    // "-0" reads oddly on a dashboard.
    if out == "-0" { "0".into() } else { out }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn find<'a>(metrics: &'a [Metric], label: &str) -> &'a Metric {
        metrics
            .iter()
            .find(|metric| metric.label == label)
            .unwrap_or_else(|| panic!("no metric {label}"))
    }

    #[test]
    fn no_game_means_the_idle_screen() {
        assert!(
            LiveDashboard::new()
                .snapshot(&DisplayUpdate::None)
                .is_none()
        );
    }

    #[test]
    fn lap_times_read_like_the_web_dashboard() {
        assert_eq!(format_lap_time(0.0), "--:--.---");
        assert_eq!(format_lap_time(93.427), "01:33.427");
        assert_eq!(format_lap_time(59.9995), "01:00.000");
    }

    #[test]
    fn session_time_drops_the_hours_when_there_are_none() {
        assert_eq!(format_time(522.0), "08:42");
        assert_eq!(format_time(3_725.0), "01:02:05");
        assert_eq!(format_time(-5.0), "-00:05");
        assert_eq!(format_time(UNLIMITED_SESSION_SECONDS), "--:--:--");
    }

    #[test]
    fn numbers_are_grouped_per_locale() {
        assert_eq!(en(24_000.0, 0), "24,000");
        assert_eq!(en(1_234.56, 1), "1,234.6");
        assert_eq!(en(-0.2, 0), "0");
        assert_eq!(nl(32_145.0), "32.145");
        assert_eq!(nl(12.25), "12,3");
        assert_eq!(nl(12.0), "12");
    }

    #[test]
    fn i_rating_is_shortened_above_a_thousand() {
        assert_eq!(format_i_rating(950), "950");
        assert_eq!(format_i_rating(2_000), "2k");
        assert_eq!(format_i_rating(2_430), "2.4k");
    }

    #[test]
    fn race_shows_optional_values_only_when_the_sim_has_them() {
        let data = RaceData {
            session_type: "Race".into(),
            speed: 184,
            gear: "4".into(),
            rpm: 6_420,
            rpm_max: 7_800,
            ..RaceData::default()
        };
        let snapshot = race(&data, vec![]);

        assert_eq!(snapshot.hero_value, "184");
        assert!(snapshot.header_metrics.iter().all(|m| m.label != "SOF"));
        assert!(
            snapshot.groups_top[0]
                .metrics
                .iter()
                .all(|m| m.label != "Pos")
        );
        assert!(
            snapshot.groups_top[1]
                .metrics
                .iter()
                .all(|m| m.label != "Fuel")
        );

        let with_position = race(
            &RaceData {
                position: Some(3),
                strength_of_field: Some(2_130),
                ..data
            },
            vec![],
        );
        assert_eq!(
            find(&with_position.groups_top[0].metrics, "Pos").value,
            "P3"
        );
        assert_eq!(find(&with_position.header_metrics, "SOF").value, "2130");
    }

    #[test]
    fn a_lap_limited_race_shows_the_total() {
        let data = RaceData {
            current_lap: 7,
            total_laps: 12,
            ..RaceData::default()
        };
        assert_eq!(
            find(&race(&data, vec![]).groups_top[0].metrics, "Lap").value,
            "7/12"
        );

        let timed = RaceData {
            is_limited_time: true,
            ..data
        };
        assert_eq!(
            find(&race(&timed, vec![]).groups_top[0].metrics, "Lap").value,
            "7"
        );
    }

    #[test]
    fn too_little_fuel_for_the_remaining_laps_is_flagged() {
        let data = RaceData {
            current_lap: 2,
            total_laps: 12,
            fuel_est_laps: 8.0,
            ..RaceData::default()
        };
        assert!(find(&race(&data, vec![]).groups_top[1].metrics, "Est. laps").critical);
    }

    #[test]
    fn the_pedal_trace_keeps_the_most_recent_samples() {
        let mut dashboard = LiveDashboard::new();
        for throttle in 0..(TRACE_LENGTH as i32 + 10) {
            dashboard.snapshot(&DisplayUpdate::Race(RaceData {
                throttle_pct: throttle,
                ..RaceData::default()
            }));
        }
        let snapshot = dashboard
            .snapshot(&DisplayUpdate::Race(RaceData::default()))
            .unwrap();

        assert_eq!(snapshot.telemetry_trace.len(), TRACE_LENGTH);
        assert_eq!(snapshot.telemetry_trace.last().unwrap().throttle, 0.0);
        assert_eq!(snapshot.telemetry_trace[0].throttle, 11.0);
    }

    #[test]
    fn rally_progress_is_a_fraction_for_the_progress_bar() {
        let snapshot = rally(&RallyData {
            completed_pct: 35,
            distance_travelled: 5_000,
            ..RallyData::default()
        });
        assert_eq!(snapshot.progress_pct, 0.35);
        assert_eq!(snapshot.header_metrics[1].value, "35%");
        assert_eq!(snapshot.groups_bottom.len(), 3);
    }

    #[test]
    fn truck_damage_is_graded_like_the_web_dashboard() {
        assert!(!damage("x", 24).warning);
        assert!(damage("x", 25).warning);
        assert!(!damage("x", 50).warning && damage("x", 50).critical);
    }

    #[test]
    fn a_truck_without_a_trailer_says_so() {
        let snapshot = truck(&TruckData::default());
        assert_eq!(
            snapshot.groups_top[3].metrics[0].value,
            "Geen trailer gekoppeld"
        );
        assert_eq!(snapshot.footer_metrics.len(), 14);
    }

    #[test]
    fn gear_advice_only_appears_when_it_differs() {
        let same = truck(&TruckData {
            gear: "9".into(),
            recommended_gear: "9".into(),
            ..TruckData::default()
        });
        assert!(same.recommended_gear.is_none());

        let shift = truck(&TruckData {
            gear: "7".into(),
            recommended_gear: "9".into(),
            ..TruckData::default()
        });
        assert_eq!(shift.recommended_gear.as_deref(), Some("9"));
    }

    #[test]
    fn the_game_clock_wraps_at_midnight() {
        assert_eq!(clock(25 * 60 + 5), "01:05");
        assert_eq!(timespan(90), "1:30");
    }

    #[test]
    fn flight_instruments_are_numbers_not_text() {
        let snapshot = flight(&FlightData {
            pitch_degrees: 3.0,
            bank_degrees: -12.0,
            heading_magnetic: 94.0,
            ground_track: 97.0,
            heading_bug: 106.0,
            autopilot_master: true,
            ..FlightData::default()
        });
        let instruments = snapshot.flight.unwrap();
        assert_eq!(instruments.bank, -12.0);
        assert_eq!(instruments.heading_bug, Some(106.0));
        assert_eq!(snapshot.groups_middle.len(), 3);
        assert_eq!(snapshot.groups_bottom.len(), 2);
    }

    #[test]
    fn without_guidance_or_a_plan_the_flight_panels_say_so() {
        let snapshot = flight(&FlightData::default());
        assert_eq!(
            snapshot.groups_middle[1].metrics[0].value,
            "No lateral guidance"
        );
        assert_eq!(
            snapshot.navigation_group.metrics[0].value,
            "No active flight plan"
        );
        assert_eq!(snapshot.groups_middle[2].metrics[0].value, "AP OFF");
    }
}

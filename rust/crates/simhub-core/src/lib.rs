#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum DashboardKind {
    Race,
    Rally,
    Truck,
    Flight,
}

impl DashboardKind {
    pub fn from_arg(value: &str) -> Option<Self> {
        match value {
            "race" => Some(Self::Race),
            "rally" => Some(Self::Rally),
            "truck" => Some(Self::Truck),
            "flight" => Some(Self::Flight),
            _ => None,
        }
    }

    pub fn as_str(self) -> &'static str {
        match self {
            Self::Race => "race",
            Self::Rally => "rally",
            Self::Truck => "truck",
            Self::Flight => "flight",
        }
    }
}

#[derive(Clone, Debug, PartialEq)]
pub struct Metric {
    pub label: String,
    pub value: String,
    pub delta: Option<f32>,
    pub ratio: f32,
    pub active: bool,
    pub warning: bool,
    pub critical: bool,
}

#[derive(Clone, Debug, PartialEq)]
pub struct MetricGroup {
    pub title: String,
    pub accent: [u8; 3],
    pub metrics: Vec<Metric>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct EngineGaugeSnapshot {
    pub value: f32,
    pub max: f32,
    pub green_start: f32,
    pub green_end: f32,
    pub redline_fraction: f32,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TelemetryPoint {
    pub brake: f32,
    pub clutch: f32,
    pub throttle: f32,
}

#[derive(Clone, Debug, PartialEq)]
pub struct DashboardSnapshot {
    pub kind: DashboardKind,
    pub title: String,
    pub header_metrics: Vec<Metric>,
    pub progress_pct: f32,
    pub hero_label: String,
    pub hero_value: String,
    pub hero_unit: String,
    pub hero_detail: String,
    pub rpm_value: String,
    pub rpm_detail: String,
    pub gear_value: String,
    pub recommended_gear: Option<String>,
    pub engine_gauge: Option<EngineGaugeSnapshot>,
    pub groups_top: Vec<MetricGroup>,
    pub groups_middle: Vec<MetricGroup>,
    pub groups_bottom: Vec<MetricGroup>,
    pub navigation_group: MetricGroup,
    pub footer_metrics: Vec<Metric>,
    pub pedals: Vec<Metric>,
    pub telemetry_trace: Vec<TelemetryPoint>,
}

fn metric(label: &str, value: &str) -> Metric {
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

fn delta_metric(label: &str, value: &str, delta: f32) -> Metric {
    Metric {
        delta: Some(delta),
        ..metric(label, value)
    }
}

fn meter(label: &str, value: &str, ratio: f32) -> Metric {
    Metric {
        ratio: ratio.clamp(0.0, 1.0),
        ..metric(label, value)
    }
}

fn indicator(label: &str, active: bool) -> Metric {
    Metric {
        active,
        ..metric(label, if active { "ON" } else { "OFF" })
    }
}

fn highlighted_metric(label: &str, value: &str, warning: bool, critical: bool) -> Metric {
    Metric {
        warning,
        critical,
        ..metric(label, value)
    }
}

fn group(title: &str, metrics: Vec<Metric>) -> MetricGroup {
    group_with_accent(title, metrics, [93, 173, 226])
}

fn group_with_accent(title: &str, metrics: Vec<Metric>, accent: [u8; 3]) -> MetricGroup {
    MetricGroup {
        title: title.into(),
        accent,
        metrics,
    }
}

pub fn demo_dashboard(kind: DashboardKind) -> DashboardSnapshot {
    match kind {
        DashboardKind::Race => race_dashboard(),
        DashboardKind::Rally => rally_dashboard(),
        DashboardKind::Truck => truck_dashboard(),
        DashboardKind::Flight => flight_dashboard(),
    }
}

fn race_dashboard() -> DashboardSnapshot {
    DashboardSnapshot {
        kind: DashboardKind::Race,
        title: "Race".into(),
        header_metrics: vec![
            metric("SESSION", "Practice"),
            metric("SOF", "2,130"),
            metric("SAFETY RATING", "3.42"),
            metric("TIME LEFT", "08:42"),
            metric("AIR", "22.4°"),
            metric("TRACK", "31.0°"),
            metric("WIND", "4.2 m/s"),
            metric("RAIN", "Light"),
            metric("GRIP", "Wet"),
        ],
        progress_pct: 0.0,
        hero_label: "SPEED".into(),
        hero_value: "184".into(),
        hero_unit: "km/h".into(),
        hero_detail: "RPM  6,420 / 7,800".into(),
        rpm_value: "6,420".into(),
        rpm_detail: "of 7,800 RPM".into(),
        gear_value: "4".into(),
        recommended_gear: None,
        engine_gauge: None,
        groups_top: vec![
            group(
                "SESSION",
                vec![
                    metric("Position", "P3"),
                    metric("Expected position", "P4"),
                    metric("Incidents", "2 / 17"),
                    metric("iRating", "2.4k"),
                    metric("Lap", "7 / 12"),
                    metric("Current lap", "01:33.427"),
                    delta_metric("Last lap", "01:33.812", 0.143),
                    delta_metric("Best lap", "01:33.669", 0.0),
                ],
            ),
            group_with_accent(
                "FUEL & TRACK",
                vec![
                    metric("Fuel remaining", "42.6 L"),
                    metric("Estimated range", "8.4 laps"),
                    metric("Fuel last / avg lap", "4.8 / 4.9 L"),
                    metric("Air / track", "22.4° / 31.0°"),
                    metric("Wind", "4.2 m/s NW"),
                    metric("Conditions", "Light rain · Wet grip"),
                    metric("Brake bias", "54.0%"),
                ],
                [255, 193, 7],
            ),
        ],
        groups_middle: vec![],
        groups_bottom: vec![],
        navigation_group: group("", vec![]),
        footer_metrics: vec![],
        pedals: vec![
            meter("BRAKE", "42%", 0.42),
            meter("CLUTCH", "0%", 0.0),
            meter("THROTTLE", "78%", 0.78),
        ],
        telemetry_trace: demo_telemetry_trace(),
    }
}

fn rally_dashboard() -> DashboardSnapshot {
    DashboardSnapshot {
        kind: DashboardKind::Rally,
        title: "Rally".into(),
        header_metrics: vec![
            metric("STAGE", "Col de Turini"),
            metric("DISTANCE", "5,000 m"),
            metric("TIME", "01:33.000"),
            delta_metric("SPLIT", "", 1.240),
        ],
        progress_pct: 0.35,
        hero_label: "SPEED".into(),
        hero_value: "120".into(),
        hero_unit: "km/h".into(),
        hero_detail: "RPM  4,500".into(),
        rpm_value: "4,500".into(),
        rpm_detail: "RPM".into(),
        gear_value: "4".into(),
        recommended_gear: None,
        engine_gauge: None,
        groups_top: vec![],
        groups_middle: vec![],
        groups_bottom: vec![
            group_with_accent(
                "SECTOR 1",
                vec![metric("Time", "00:45.000")],
                [93, 173, 226],
            ),
            group_with_accent(
                "SECTOR 2",
                vec![metric("Time", "00:48.000")],
                [125, 211, 192],
            ),
            group_with_accent(
                "STAGE RUNNING",
                vec![metric("Time", "01:33.000")],
                [255, 193, 7],
            ),
        ],
        navigation_group: group("", vec![]),
        footer_metrics: vec![],
        pedals: vec![],
        telemetry_trace: vec![],
    }
}

fn truck_dashboard() -> DashboardSnapshot {
    DashboardSnapshot {
        kind: DashboardKind::Truck,
        title: "Truck".into(),
        header_metrics: vec![],
        progress_pct: 0.0,
        hero_label: "SPEED".into(),
        hero_value: "80".into(),
        hero_unit: "km/h".into(),
        hero_detail: "LIMIT  90 km/h".into(),
        rpm_value: "1,500".into(),
        rpm_detail: "of 2,500 RPM".into(),
        gear_value: "7".into(),
        recommended_gear: Some("9".into()),
        engine_gauge: Some(EngineGaugeSnapshot {
            value: 1_500.0,
            max: 2_500.0,
            green_start: 1_250.0,
            green_end: 2_000.0,
            redline_fraction: 0.85,
        }),
        groups_top: vec![
            group_with_accent(
                "ROUTE",
                vec![
                    metric("Route", "13:00"),
                    metric("Departure", "Rotterdam (Cargo)"),
                    metric("Destination", "Utrecht (Depot)"),
                    metric("Remaining", "1:30 / 120 km"),
                    metric("Next rest", "1:00"),
                    metric("Speed limit", "90 km/h"),
                ],
                [75, 165, 255],
            ),
            group_with_accent(
                "JOB",
                vec![
                    metric("Truck", "Volvo FH"),
                    metric("Deadline", "10:00"),
                    metric("Income", "€ 32,145"),
                    metric("Cargo", "Helicopter (2,500 kg)"),
                ],
                [44, 211, 112],
            ),
            group_with_accent(
                "TRUCK DAMAGE",
                vec![
                    highlighted_metric("Engine", "10%", false, false),
                    highlighted_metric("Transmission", "30%", true, false),
                    metric("Cabin", "0%"),
                    metric("Chassis", "0%"),
                    metric("Wheels", "0%"),
                    metric("Odometer", "100 km"),
                ],
                [255, 177, 0],
            ),
            group_with_accent(
                "TRAILER DAMAGE",
                vec![
                    metric("Chassis", "0%"),
                    metric("Body", "0%"),
                    metric("Wheels", "0%"),
                    highlighted_metric("Cargo", "60%", false, true),
                ],
                [255, 76, 76],
            ),
        ],
        groups_middle: vec![],
        groups_bottom: vec![
            group(
                "FUEL & CONTROLS",
                vec![
                    meter("Fuel", "800 km (500 l)", 0.62),
                    metric("AdBlue", "100 l"),
                    metric("Acceleration", "20%"),
                ],
            ),
            group(
                "VEHICLE",
                vec![
                    metric("Battery", "24 V"),
                    metric("Water", "90 °C"),
                    metric("Oil pressure", "30 PSI"),
                    metric("Brake air", "8 PSI"),
                ],
            ),
        ],
        navigation_group: group("", vec![]),
        footer_metrics: vec![
            indicator("ENGINE", true),
            indicator("PARKING", false),
            indicator("BEACON", true),
            indicator("LOW BEAM", true),
            indicator("HIGH BEAM", false),
            indicator("HAZARD", false),
            indicator("LEFT", false),
            indicator("RIGHT", false),
            indicator("SEAT BELT", true),
            indicator("BRAKE", false),
            indicator("ABS", true),
            indicator("TRACTION", true),
            indicator("ENGINE BRAKE", false),
            indicator("DIFFERENTIAL", false),
        ],
        pedals: vec![],
        telemetry_trace: vec![],
    }
}

fn demo_telemetry_trace() -> Vec<TelemetryPoint> {
    (0..96)
        .map(|index| {
            let phase = index % 24;
            let (brake, clutch, throttle) = match phase {
                0..=3 => (0.0, 0.0, phase as f32 * 25.0),
                4..=11 => (0.0, 0.0, 95.0 - (phase - 4) as f32),
                12..=15 => (
                    [25.0, 68.0, 92.0, 54.0][phase - 12],
                    0.0,
                    [65.0, 35.0, 10.0, 0.0][phase - 12],
                ),
                16..=19 => (
                    0.0,
                    if phase == 17 { 85.0 } else { 0.0 },
                    20.0 + (phase - 16) as f32 * 24.0,
                ),
                _ => (0.0, 0.0, 92.0),
            };
            TelemetryPoint {
                brake,
                clutch,
                throttle,
            }
        })
        .collect()
}

fn flight_dashboard() -> DashboardSnapshot {
    DashboardSnapshot {
        kind: DashboardKind::Flight,
        title: "Flight".into(),
        header_metrics: vec![],
        progress_pct: 0.0,
        hero_label: "INDICATED AIRSPEED".into(),
        hero_value: "268".into(),
        hero_unit: "kts".into(),
        hero_detail: "GROUND  281 kts     MACH  0.78".into(),
        rpm_value: "84%".into(),
        rpm_detail: "N1".into(),
        gear_value: "FLAPS  1".into(),
        recommended_gear: None,
        engine_gauge: None,
        groups_top: vec![
            group(
                "ATTITUDE",
                vec![
                    metric("Pitch", "3.0°"),
                    metric("Bank", "12.0°"),
                    metric("Slip", "Centered"),
                ],
            ),
            group(
                "ALTITUDE",
                vec![
                    metric("Indicated", "24,000 ft"),
                    metric("Radar", "23,860 ft"),
                    metric("Vertical speed", "720 fpm"),
                    metric("QNH", "1013"),
                ],
            ),
        ],
        groups_middle: vec![
            group(
                "HEADING",
                vec![
                    metric("Heading", "94°"),
                    metric("Track", "97°"),
                    metric("Wind", "270° / 34 kts"),
                ],
            ),
            group(
                "COURSE",
                vec![
                    metric("GPS", "ACTIVE"),
                    metric("Waypoint", "ARTIP"),
                    metric("Course", "106°"),
                    metric("Scale", "±2 NM"),
                ],
            ),
            group(
                "AUTOPILOT",
                vec![
                    metric("Modes", "HDG · NAV · ALT"),
                    metric("Altitude", "24,000 ft"),
                    metric("Heading", "106°"),
                ],
            ),
        ],
        groups_bottom: vec![
            group(
                "ENGINE & FUEL",
                vec![meter("N1", "78%", 0.78), meter("Fuel", "4,320 lb", 0.65)],
            ),
            group(
                "CONFIGURATION",
                vec![
                    metric("Flaps", "UP / 4"),
                    metric("Gear", "UP"),
                    metric("Spoilers", "0%"),
                    metric("Park brake", "OFF"),
                    metric("Trim", "4%"),
                    metric("Aircraft", "Cessna"),
                ],
            ),
        ],
        navigation_group: group(
            "NAVIGATION",
            vec![
                metric("Next waypoint", "ARTIP"),
                metric("Distance", "18.4 NM"),
                metric("ETA", "4 min"),
                metric("Destination", "EHAM"),
                metric("Distance", "87 NM"),
                metric("Arrival", "34 min · 14:12Z"),
            ],
        ),
        footer_metrics: vec![
            indicator("ENGINE", true),
            indicator("PARKING BRAKE", false),
            indicator("LANDING LIGHTS", true),
            indicator("STROBE", true),
        ],
        pedals: vec![],
        telemetry_trace: vec![],
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn known_dashboard_modes_parse() {
        assert_eq!(DashboardKind::from_arg("race"), Some(DashboardKind::Race));
        assert_eq!(DashboardKind::from_arg("rally"), Some(DashboardKind::Rally));
        assert_eq!(DashboardKind::from_arg("truck"), Some(DashboardKind::Truck));
        assert_eq!(
            DashboardKind::from_arg("flight"),
            Some(DashboardKind::Flight)
        );
        assert_eq!(DashboardKind::from_arg("unknown"), None);
    }

    #[test]
    fn demo_data_matches_each_dashboard_layout() {
        let race = demo_dashboard(DashboardKind::Race);
        assert_eq!(race.hero_value, "184");
        assert_eq!(race.groups_top.len(), 2);
        assert_eq!(race.pedals.len(), 3);
        assert_eq!(race.telemetry_trace.len(), 96);
        assert_eq!(
            race.pedals
                .iter()
                .map(|pedal| pedal.label.as_str())
                .collect::<Vec<_>>(),
            ["BRAKE", "CLUTCH", "THROTTLE"]
        );

        let rally = demo_dashboard(DashboardKind::Rally);
        assert_eq!(rally.progress_pct, 0.35);
        assert_eq!(rally.groups_bottom.len(), 3);

        let truck = demo_dashboard(DashboardKind::Truck);
        assert_eq!(truck.groups_top.len(), 4);
        assert_eq!(truck.footer_metrics.len(), 14);
        assert_eq!(truck.gear_value, "7");
        assert_eq!(truck.recommended_gear.as_deref(), Some("9"));
        assert_eq!(
            truck.engine_gauge.as_ref().map(|gauge| gauge.value),
            Some(1_500.0)
        );

        let flight = demo_dashboard(DashboardKind::Flight);
        assert_eq!(flight.groups_middle.len(), 3);
        assert_eq!(flight.navigation_group.title, "NAVIGATION");
    }

    #[test]
    fn race_lap_deltas_are_separate_from_lap_times() {
        let race = demo_dashboard(DashboardKind::Race);
        let session_metrics = &race.groups_top[0].metrics;
        let last_lap = session_metrics
            .iter()
            .find(|metric| metric.label == "Last lap")
            .unwrap();
        let best_lap = session_metrics
            .iter()
            .find(|metric| metric.label == "Best lap")
            .unwrap();

        assert_eq!(last_lap.value, "01:33.812");
        assert_eq!(best_lap.value, "01:33.669");
        assert_eq!(last_lap.delta, Some(0.143));
        assert_eq!(best_lap.delta, Some(0.0));
    }

    #[test]
    fn rally_split_is_exposed_as_a_colored_delta() {
        let rally = demo_dashboard(DashboardKind::Rally);
        let split = rally
            .header_metrics
            .iter()
            .find(|metric| metric.label == "SPLIT")
            .unwrap();

        assert_eq!(split.value, "");
        assert_eq!(split.delta, Some(1.240));
    }

    #[test]
    fn meter_ratios_stay_in_renderable_range() {
        assert_eq!(meter("test", "low", -0.1).ratio, 0.0);
        assert_eq!(meter("test", "high", 1.1).ratio, 1.0);
    }
}

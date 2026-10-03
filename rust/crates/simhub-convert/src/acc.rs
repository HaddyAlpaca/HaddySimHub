//! Ported from `HaddySimHub/Displays/ACC/ACCDataConverter.cs`.

use simhub_model::{DisplayUpdate, RaceData, telemetry::AccTelemetry};

/// Assetto Corsa encodes gears as 0 = reverse, 1 = neutral, 2 = first gear.
fn gear_label(gear: i32) -> String {
    match gear {
        g if g <= 0 => "R".to_string(),
        1 => "N".to_string(),
        g => (g - 1).to_string(),
    }
}

fn session_label(session_type: i32) -> &'static str {
    match session_type {
        1 => "Qualifying",
        2 => "Race",
        3 => "Hotlap",
        4 => "Time Attack",
        5 => "Drift",
        6 => "Drag",
        7 => "Hotstint",
        8 => "Superpole",
        // 0 and anything unrecognised fall back to practice, as in the C# converter.
        _ => "Practice",
    }
}

pub fn convert(source: &AccTelemetry) -> DisplayUpdate {
    DisplayUpdate::Race(RaceData {
        session_type: session_label(source.session_type).to_string(),
        is_limited_time: source.session_time_left_ms > 0,
        is_limited_session_laps: source.number_of_laps > 0,
        // The graphics page counts laps finished, so the lap being driven is one further on.
        current_lap: source.current_lap + 1,
        total_laps: source.number_of_laps,
        session_time_remaining: source.session_time_left_ms as f32 / 1000.0,
        position: Some(source.position),
        speed: source.speed_kmh as i32,
        gear: gear_label(source.gear),
        rpm: source.rpms,
        rpm_max: source.max_rpm as i32,
        track_temp: source.road_temp,
        air_temp: source.air_temp,
        fuel_remaining: Some(source.fuel),
        fuel_avg_lap: Some(source.fuel_per_lap),
        fuel_last_lap: None,
        fuel_est_laps: source.fuel_estimated_laps,
        current_lap_time: source.current_time_ms as f32 / 1000.0,
        last_lap_time: source.last_time_ms as f32 / 1000.0,
        last_lap_time_delta: Some(source.delta_ms as f32 / 1000.0),
        best_lap_time: Some(source.best_time_ms as f32 / 1000.0),
        best_lap_time_delta: None,
        clutch_pct: (source.clutch * 100.0) as i32,
        throttle_pct: (source.gas * 100.0) as i32,
        brake_pct: (source.brake * 100.0) as i32,
        pit_limiter_on: source.pit_limiter_on == 1,
        steering_pct: (source.steer_angle.abs() * 100.0) as i32,
        brake_bias: Some(source.brake_bias),
        rain_intensity: Some(source.rain_intensity),
        wind_speed: Some(source.wind_speed),
        wind_direction: Some(source.wind_direction),
        track_grip_status: Some(source.track_grip_status),
        ..Default::default()
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    use simhub_model::DisplayType;

    fn race(source: &AccTelemetry) -> RaceData {
        match convert(source) {
            DisplayUpdate::Race(data) => data,
            other => panic!("expected a race dashboard, got {other:?}"),
        }
    }

    #[test]
    fn convert_returns_race_dashboard() {
        let update = convert(&AccTelemetry::default());
        assert_eq!(update.display_type(), DisplayType::RaceDashboard);
    }

    #[test]
    fn convert_gear_neutral() {
        let data = race(&AccTelemetry {
            gear: 1,
            ..Default::default()
        });
        assert_eq!(data.gear, "N");
    }

    #[test]
    fn convert_gear_reverse() {
        let data = race(&AccTelemetry {
            gear: 0,
            ..Default::default()
        });
        assert_eq!(data.gear, "R");
    }

    #[test]
    fn convert_gear_forward() {
        let data = race(&AccTelemetry {
            gear: 3,
            ..Default::default()
        });
        assert_eq!(data.gear, "2");
    }

    #[test]
    fn convert_speed_truncates_towards_zero() {
        let data = race(&AccTelemetry {
            speed_kmh: 108.9,
            ..Default::default()
        });
        assert_eq!(data.speed, 108);
    }

    #[test]
    fn convert_rpm() {
        let data = race(&AccTelemetry {
            rpms: 6000,
            ..Default::default()
        });
        assert_eq!(data.rpm, 6000);
    }

    #[test]
    fn convert_session_types() {
        for (raw, expected) in [
            (0, "Practice"),
            (1, "Qualifying"),
            (2, "Race"),
            (3, "Hotlap"),
            (4, "Time Attack"),
            (5, "Drift"),
            (6, "Drag"),
            (7, "Hotstint"),
            (8, "Superpole"),
            (99, "Practice"),
        ] {
            let data = race(&AccTelemetry {
                session_type: raw,
                ..Default::default()
            });
            assert_eq!(data.session_type, expected, "session type {raw}");
        }
    }

    #[test]
    fn convert_lap_counter_advances_past_completed_laps() {
        let data = race(&AccTelemetry {
            current_lap: 4,
            ..Default::default()
        });
        assert_eq!(data.current_lap, 5);
    }

    #[test]
    fn convert_times_are_seconds() {
        let data = race(&AccTelemetry {
            current_time_ms: 91_500,
            last_time_ms: 92_250,
            best_time_ms: 90_000,
            delta_ms: -1_250,
            ..Default::default()
        });
        assert_eq!(data.current_lap_time, 91.5);
        assert_eq!(data.last_lap_time, 92.25);
        assert_eq!(data.best_lap_time, Some(90.0));
        assert_eq!(data.last_lap_time_delta, Some(-1.25));
    }

    #[test]
    fn convert_pedals_are_percentages() {
        let data = race(&AccTelemetry {
            gas: 0.5,
            brake: 0.25,
            clutch: 1.0,
            steer_angle: -0.4,
            ..Default::default()
        });
        assert_eq!(data.throttle_pct, 50);
        assert_eq!(data.brake_pct, 25);
        assert_eq!(data.clutch_pct, 100);
        // Steering is reported as magnitude, so a left lock reads the same as a right one.
        assert_eq!(data.steering_pct, 40);
    }

    #[test]
    fn convert_marks_fields_acc_does_not_expose() {
        let data = race(&AccTelemetry::default());
        assert_eq!(data.fuel_last_lap, None);
        assert_eq!(data.best_lap_time_delta, None);
    }
}

//! Ported from `HaddySimHub/Displays/AC/ACDataConverter.cs`.

use simhub_model::{DisplayUpdate, RaceData, telemetry::AcTelemetry};

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
        // 0 is practice, and -1 means the game has not said yet.
        _ => "Practice",
    }
}

/// The game reports an unset best lap as `i32::MAX` rather than zero.
fn lap_time_seconds(milliseconds: i32) -> Option<f32> {
    if milliseconds > 0 && milliseconds != i32::MAX {
        Some(milliseconds as f32 / 1000.0)
    } else {
        None
    }
}

pub fn convert(source: &AcTelemetry) -> DisplayUpdate {
    DisplayUpdate::Race(RaceData {
        session_type: session_label(source.session_type).to_string(),
        is_limited_time: source.session_time_left > 0.0,
        is_limited_session_laps: source.number_of_laps > 0,
        // The graphics page counts laps finished, so the lap being driven is one further on.
        current_lap: source.completed_laps + 1,
        total_laps: source.number_of_laps,
        session_time_remaining: source.session_time_left / 1000.0,
        position: (source.position > 0).then_some(source.position),

        speed: source.speed_kmh.max(0.0) as i32,
        gear: gear_label(source.gear),
        rpm: source.rpms,
        rpm_max: source.max_rpm,
        track_temp: source.road_temp,
        air_temp: source.air_temp,

        fuel_remaining: Some(source.fuel),
        // Assetto Corsa does not report fuel use per lap, so neither the average
        // nor anything derived from it can be filled in.
        fuel_avg_lap: None,
        fuel_last_lap: None,
        fuel_est_laps: 0.0,

        current_lap_time: source.current_time as f32 / 1000.0,
        last_lap_time: source.last_time as f32 / 1000.0,
        // Not exposed by the Assetto Corsa shared memory.
        last_lap_time_delta: None,
        best_lap_time: lap_time_seconds(source.best_time),
        best_lap_time_delta: None,

        clutch_pct: (source.clutch * 100.0) as i32,
        throttle_pct: (source.gas * 100.0) as i32,
        brake_pct: (source.brake * 100.0) as i32,
        pit_limiter_on: source.pit_limiter_on == 1,
        steering_pct: (source.steer_angle.abs() * 100.0) as i32,
        brake_bias: Some(source.brake_bias),
        ..Default::default()
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    use simhub_model::DisplayType;

    fn race(source: &AcTelemetry) -> RaceData {
        match convert(source) {
            DisplayUpdate::Race(data) => data,
            other => panic!("expected a race dashboard, got {other:?}"),
        }
    }

    #[test]
    fn convert_returns_a_race_dashboard() {
        assert_eq!(
            convert(&AcTelemetry::default()).display_type(),
            DisplayType::RaceDashboard
        );
    }

    #[test]
    fn gear_uses_the_assetto_encoding() {
        assert_eq!(gear_label(0), "R");
        assert_eq!(gear_label(1), "N");
        assert_eq!(gear_label(2), "1");
        assert_eq!(gear_label(7), "6");
    }

    #[test]
    fn session_types_cover_the_seven_assetto_modes() {
        for (raw, expected) in [
            (0, "Practice"),
            (1, "Qualifying"),
            (2, "Race"),
            (3, "Hotlap"),
            (4, "Time Attack"),
            (5, "Drift"),
            (6, "Drag"),
        ] {
            assert_eq!(session_label(raw), expected, "session type {raw}");
        }
    }

    #[test]
    fn an_unknown_session_falls_back_to_practice() {
        // The game reports -1 until it has decided.
        assert_eq!(session_label(-1), "Practice");
        assert_eq!(session_label(42), "Practice");
    }

    #[test]
    fn position_is_omitted_when_the_game_does_not_expose_it() {
        assert_eq!(race(&AcTelemetry::default()).position, None);
        let data = race(&AcTelemetry {
            position: 4,
            ..Default::default()
        });
        assert_eq!(data.position, Some(4));
    }

    #[test]
    fn an_unset_best_lap_reads_as_no_lap() {
        assert_eq!(lap_time_seconds(i32::MAX), None);
        assert_eq!(lap_time_seconds(0), None);
        assert_eq!(lap_time_seconds(-1), None);
        assert_eq!(lap_time_seconds(90_500), Some(90.5));
    }

    #[test]
    fn speed_is_floored_at_zero_before_truncating() {
        let rolling_back = race(&AcTelemetry {
            speed_kmh: -3.5,
            ..Default::default()
        });
        assert_eq!(rolling_back.speed, 0);

        let data = race(&AcTelemetry {
            speed_kmh: 108.9,
            ..Default::default()
        });
        assert_eq!(data.speed, 108);
    }

    #[test]
    fn session_time_is_milliseconds_as_a_float() {
        let data = race(&AcTelemetry {
            session_time_left: 125_400.0,
            ..Default::default()
        });
        assert_eq!(data.session_time_remaining, 125.4);
        assert!(data.is_limited_time);

        let untimed = race(&AcTelemetry::default());
        assert!(!untimed.is_limited_time);
    }

    #[test]
    fn the_lap_counter_advances_past_completed_laps() {
        let data = race(&AcTelemetry {
            completed_laps: 4,
            ..Default::default()
        });
        assert_eq!(data.current_lap, 5);
    }

    #[test]
    fn fuel_use_per_lap_is_not_reported_by_this_game() {
        let data = race(&AcTelemetry {
            fuel: 42.0,
            ..Default::default()
        });
        assert_eq!(data.fuel_remaining, Some(42.0));
        assert_eq!(data.fuel_avg_lap, None);
        assert_eq!(data.fuel_last_lap, None);
        assert_eq!(data.fuel_est_laps, 0.0);
    }

    #[test]
    fn deltas_are_not_exposed_by_this_game() {
        let data = race(&AcTelemetry::default());
        assert_eq!(data.last_lap_time_delta, None);
        assert_eq!(data.best_lap_time_delta, None);
    }
}

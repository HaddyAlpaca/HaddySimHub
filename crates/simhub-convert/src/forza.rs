//! Ported from `HaddySimHub/Displays/Forza/ForzaDataConverter.cs`.

use simhub_model::{DisplayUpdate, RaceData, telemetry::ForzaTelemetry};

/// Forza encodes gears as 0 = reverse, 1 = neutral, 2 = first gear.
fn gear_label(gear: u8) -> String {
    match gear {
        0 => "R".to_string(),
        1 => "N".to_string(),
        g => (g as i32 - 1).to_string(),
    }
}

/// Pedals arrive as a byte over the full 0-255 range. The C# converter divided
/// in integer arithmetic, so the result truncates: half travel reads 50, not 50.2.
fn pedal_pct(raw: u8) -> i32 {
    raw as i32 * 100 / u8::MAX as i32
}

pub fn convert(source: &ForzaTelemetry) -> DisplayUpdate {
    DisplayUpdate::Race(RaceData {
        session_type: if source.is_race_on != 0 {
            "Race".to_string()
        } else {
            "Practice".to_string()
        },
        current_lap: source.lap_number as i32,
        speed: (source.speed * 3.6).max(0.0) as i32,
        gear: gear_label(source.gear),
        rpm: source.current_engine_rpm.max(0.0) as i32,
        rpm_max: source.engine_max_rpm.max(0.0) as i32,
        fuel_remaining: Some(source.fuel),
        current_lap_time: source.current_lap.max(0.0),
        last_lap_time: source.last_lap.max(0.0),
        best_lap_time: (source.best_lap > 0.0).then_some(source.best_lap),
        throttle_pct: pedal_pct(source.throttle),
        brake_pct: pedal_pct(source.brake),
        clutch_pct: pedal_pct(source.clutch),
        // Steering is signed; the dashboard shows magnitude against full lock.
        steering_pct: ((source.steer as i32).abs() as f32 * 100.0 / i8::MAX as f32) as i32,
        position: (source.race_position > 0).then_some(source.race_position as i32),
        car_ordinal: Some(source.car_ordinal),
        car_class: Some(source.car_class),
        car_performance_index: Some(source.car_performance_index),
        drivetrain_type: Some(source.drivetrain_type),
        num_cylinders: Some(source.num_cylinders),
        power: Some(source.power),
        torque: Some(source.torque),
        ..Default::default()
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    use simhub_model::DisplayType;

    fn race(source: &ForzaTelemetry) -> RaceData {
        match convert(source) {
            DisplayUpdate::Race(data) => data,
            other => panic!("expected a race dashboard, got {other:?}"),
        }
    }

    #[test]
    fn convert_returns_a_race_dashboard() {
        assert_eq!(
            convert(&ForzaTelemetry::default()).display_type(),
            DisplayType::RaceDashboard
        );
    }

    #[test]
    fn gear_uses_the_same_encoding_as_assetto() {
        assert_eq!(gear_label(0), "R");
        assert_eq!(gear_label(1), "N");
        assert_eq!(gear_label(2), "1");
        assert_eq!(gear_label(9), "8");
    }

    #[test]
    fn the_session_reads_as_a_race_only_while_one_is_running() {
        let paused = race(&ForzaTelemetry::default());
        assert_eq!(paused.session_type, "Practice");

        let racing = race(&ForzaTelemetry {
            is_race_on: 1,
            ..Default::default()
        });
        assert_eq!(racing.session_type, "Race");
    }

    #[test]
    fn pedals_scale_from_a_byte_and_truncate() {
        assert_eq!(pedal_pct(0), 0);
        assert_eq!(pedal_pct(255), 100);
        // 128/255 is 50.2%, which the integer division drops to 50.
        assert_eq!(pedal_pct(128), 50);
        // A barely pressed pedal reads as nothing at all.
        assert_eq!(pedal_pct(1), 0);
    }

    #[test]
    fn steering_reports_magnitude_against_full_lock() {
        let centred = race(&ForzaTelemetry::default());
        assert_eq!(centred.steering_pct, 0);

        let full_right = race(&ForzaTelemetry {
            steer: i8::MAX,
            ..Default::default()
        });
        assert_eq!(full_right.steering_pct, 100);

        // Left lock travels one further than right, and still reads as full.
        let full_left = race(&ForzaTelemetry {
            steer: i8::MIN,
            ..Default::default()
        });
        assert_eq!(full_left.steering_pct, 100);
    }

    #[test]
    fn speed_converts_metres_per_second_to_kilometres_per_hour() {
        let data = race(&ForzaTelemetry {
            speed: 30.0,
            ..Default::default()
        });
        assert_eq!(data.speed, 108);
    }

    #[test]
    fn negative_readings_are_floored_at_zero() {
        let data = race(&ForzaTelemetry {
            speed: -5.0,
            current_engine_rpm: -1.0,
            engine_max_rpm: -1.0,
            current_lap: -2.0,
            last_lap: -3.0,
            ..Default::default()
        });
        assert_eq!(data.speed, 0);
        assert_eq!(data.rpm, 0);
        assert_eq!(data.rpm_max, 0);
        assert_eq!(data.current_lap_time, 0.0);
        assert_eq!(data.last_lap_time, 0.0);
    }

    #[test]
    fn an_unset_best_lap_is_omitted() {
        assert_eq!(race(&ForzaTelemetry::default()).best_lap_time, None);

        let data = race(&ForzaTelemetry {
            best_lap: 92.5,
            ..Default::default()
        });
        assert_eq!(data.best_lap_time, Some(92.5));
    }

    #[test]
    fn position_is_omitted_outside_a_race() {
        assert_eq!(race(&ForzaTelemetry::default()).position, None);

        let data = race(&ForzaTelemetry {
            race_position: 7,
            ..Default::default()
        });
        assert_eq!(data.position, Some(7));
    }

    #[test]
    fn car_identification_passes_through() {
        let data = race(&ForzaTelemetry {
            car_ordinal: 2402,
            car_class: 5,
            car_performance_index: 812,
            drivetrain_type: 2,
            num_cylinders: 8,
            power: 410_000.0,
            torque: 720.0,
            ..Default::default()
        });
        assert_eq!(data.car_ordinal, Some(2402));
        assert_eq!(data.car_class, Some(5));
        assert_eq!(data.car_performance_index, Some(812));
        assert_eq!(data.drivetrain_type, Some(2));
        assert_eq!(data.num_cylinders, Some(8));
        assert_eq!(data.power, Some(410_000.0));
        assert_eq!(data.torque, Some(720.0));
    }
}

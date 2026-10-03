//! Ported from `HaddySimHub/Displays/Dirt2/Dirt2DataConverter.cs`.

use simhub_model::{DisplayUpdate, RallyData, telemetry::Dirt2Telemetry};

fn gear_label(gear: f32) -> String {
    if gear == 0.0 {
        "N".to_string()
    } else if gear < 0.0 {
        // The packet does not number reverse gears, so there is only ever one.
        "R".to_string()
    } else {
        (gear as i32).to_string()
    }
}

pub fn convert(data: &Dirt2Telemetry) -> DisplayUpdate {
    DisplayUpdate::Rally(RallyData {
        speed: (data.speed_ms * 3.6).round() as i32,
        // The packet reports engine speed in tens.
        rpm: (data.rpm * 10.0).round() as i32,
        rpm_max: (data.max_rpm * 10.0).round() as i32,
        gear: gear_label(data.gear),
        completed_pct: ((data.progress * 100.0).round() as i32).min(100),
        distance_travelled: (data.distance.round() as i32).max(0),
        position: data.car_pos.round() as i32,
        sector1_time: data.sector_1_time,
        sector2_time: data.sector_2_time,
        lap_time: data.lap_time,
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    use simhub_model::DisplayType;

    fn rally(data: &Dirt2Telemetry) -> RallyData {
        match convert(data) {
            DisplayUpdate::Rally(data) => data,
            other => panic!("expected a rally dashboard, got {other:?}"),
        }
    }

    #[test]
    fn convert_returns_a_rally_dashboard() {
        assert_eq!(
            convert(&Dirt2Telemetry::default()).display_type(),
            DisplayType::RallyDashboard
        );
    }

    #[test]
    fn gear_labels_have_a_single_reverse() {
        assert_eq!(gear_label(0.0), "N");
        assert_eq!(gear_label(-1.0), "R");
        assert_eq!(gear_label(4.0), "4");
    }

    #[test]
    fn speed_converts_metres_per_second_to_kilometres_per_hour() {
        let data = rally(&Dirt2Telemetry {
            speed_ms: 30.0,
            ..Default::default()
        });
        assert_eq!(data.speed, 108);
    }

    #[test]
    fn engine_speed_is_scaled_by_ten() {
        let data = rally(&Dirt2Telemetry {
            rpm: 620.5,
            max_rpm: 750.0,
            ..Default::default()
        });
        assert_eq!(data.rpm, 6205);
        assert_eq!(data.rpm_max, 7500);
    }

    #[test]
    fn progress_is_capped_at_a_full_stage() {
        // The packet can report slightly past the end of the stage.
        let data = rally(&Dirt2Telemetry {
            progress: 1.04,
            ..Default::default()
        });
        assert_eq!(data.completed_pct, 100);
    }

    #[test]
    fn progress_reports_whole_percentages() {
        let data = rally(&Dirt2Telemetry {
            progress: 0.436,
            ..Default::default()
        });
        assert_eq!(data.completed_pct, 44);
    }

    #[test]
    fn distance_is_floored_at_zero() {
        // Before the start line the packet counts down to the stage beginning.
        let data = rally(&Dirt2Telemetry {
            distance: -12.0,
            ..Default::default()
        });
        assert_eq!(data.distance_travelled, 0);
    }

    #[test]
    fn stage_times_pass_through_unchanged() {
        let data = rally(&Dirt2Telemetry {
            lap_time: 123.456,
            sector_1_time: 41.5,
            sector_2_time: 80.25,
            car_pos: 3.0,
            ..Default::default()
        });
        assert_eq!(data.lap_time, 123.456);
        assert_eq!(data.sector1_time, 41.5);
        assert_eq!(data.sector2_time, 80.25);
        assert_eq!(data.position, 3);
    }
}

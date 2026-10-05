//! Ported from `HaddySimHub/Displays/ACRally/ACRallyDataConverter.cs`.
//!
//! A rally stage is a single lap, so stage progress comes from the position
//! along the spline rather than a lap count. The shared memory pages expose the
//! duration of the sector that was completed last, but not the running split
//! times the dashboard shows, so those are accumulated here as the car crosses
//! each sector.

use simhub_model::{DisplayUpdate, RallyData, telemetry::AcRallyTelemetry};

/// Assetto Corsa reports gears as 0 = reverse, 1 = neutral, 2 = first gear.
fn gear_label(gear: i32) -> String {
    match gear {
        g if g <= 0 => "R".to_string(),
        1 => "N".to_string(),
        g => (g - 1).to_string(),
    }
}

fn distance_travelled(data: &AcRallyTelemetry, progress: f32) -> i32 {
    // The graphics page reports metres covered directly; the spline position
    // scaled by the stage length covers the frames before it starts counting.
    if data.distance_traveled > 0.0 {
        return data.distance_traveled.round() as i32;
    }

    if data.track_spline_length > 0.0 {
        (progress * data.track_spline_length).round() as i32
    } else {
        0
    }
}

#[derive(Debug, Default)]
pub struct AcRallyConverter {
    last_sector_index: i32,
    last_stage_time_ms: i32,
    sector1_time: f32,
    sector2_time: f32,
}

impl AcRallyConverter {
    pub fn new() -> Self {
        Self::default()
    }

    /// Records the elapsed stage time each time the car crosses into a new
    /// sector, and starts over when a new stage begins.
    ///
    /// The C# original guarded this with a lock; here exclusive access comes
    /// from `&mut self`, so no lock is needed.
    fn track_sector_splits(&mut self, data: &AcRallyTelemetry) -> (f32, f32) {
        let restarted = data.current_sector_index < self.last_sector_index
            || data.current_time < self.last_stage_time_ms;

        if restarted {
            self.sector1_time = 0.0;
            self.sector2_time = 0.0;
        } else if data.current_sector_index > self.last_sector_index {
            let elapsed_seconds = data.current_time as f32 / 1000.0;

            // The sector just left is the one before the index the car moved into.
            match data.current_sector_index - 1 {
                0 => self.sector1_time = elapsed_seconds,
                1 => self.sector2_time = elapsed_seconds,
                _ => {}
            }
        }

        self.last_sector_index = data.current_sector_index;
        self.last_stage_time_ms = data.current_time;

        (self.sector1_time, self.sector2_time)
    }

    pub fn convert(&mut self, data: &AcRallyTelemetry) -> DisplayUpdate {
        let (sector1_time, sector2_time) = self.track_sector_splits(data);

        let stage_time_seconds = data.current_time as f32 / 1000.0;
        // The static page carries a rev limit for most cars; where it does not,
        // the physics page has the current car's.
        let rpm_max = if data.max_rpm > 0 {
            data.max_rpm
        } else {
            data.current_max_rpm.round() as i32
        };
        let progress = data.normalized_car_position.clamp(0.0, 1.0);

        DisplayUpdate::Rally(RallyData {
            speed: data.speed_kmh.max(0.0).round() as i32,
            rpm: data.rpms,
            rpm_max,
            gear: gear_label(data.gear),
            completed_pct: (progress * 100.0).round() as i32,
            distance_travelled: distance_travelled(data, progress),
            // A stage is driven alone, so there is always a position to show.
            position: if data.position > 0 { data.position } else { 1 },
            sector1_time,
            sector2_time,
            lap_time: stage_time_seconds,
        })
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use simhub_model::DisplayType;

    fn rally(converter: &mut AcRallyConverter, data: &AcRallyTelemetry) -> RallyData {
        match converter.convert(data) {
            DisplayUpdate::Rally(data) => data,
            other => panic!("expected a rally dashboard, got {other:?}"),
        }
    }

    fn once(data: &AcRallyTelemetry) -> RallyData {
        rally(&mut AcRallyConverter::new(), data)
    }

    #[test]
    fn convert_returns_a_rally_dashboard() {
        assert_eq!(
            AcRallyConverter::new()
                .convert(&AcRallyTelemetry::default())
                .display_type(),
            DisplayType::RallyDashboard
        );
    }

    #[test]
    fn gear_uses_the_assetto_encoding() {
        assert_eq!(gear_label(0), "R");
        assert_eq!(gear_label(1), "N");
        assert_eq!(gear_label(2), "1");
    }

    #[test]
    fn speed_rounds_rather_than_truncating() {
        // Unlike the Assetto Corsa converter, which casts and so truncates.
        let data = once(&AcRallyTelemetry {
            speed_kmh: 108.6,
            ..Default::default()
        });
        assert_eq!(data.speed, 109);
    }

    #[test]
    fn speed_is_floored_at_zero() {
        let data = once(&AcRallyTelemetry {
            speed_kmh: -4.0,
            ..Default::default()
        });
        assert_eq!(data.speed, 0);
    }

    #[test]
    fn the_rev_limit_falls_back_to_the_physics_page() {
        let from_static = once(&AcRallyTelemetry {
            max_rpm: 7000,
            current_max_rpm: 9999.0,
            ..Default::default()
        });
        assert_eq!(from_static.rpm_max, 7000);

        let from_physics = once(&AcRallyTelemetry {
            max_rpm: 0,
            current_max_rpm: 7500.4,
            ..Default::default()
        });
        assert_eq!(from_physics.rpm_max, 7500);
    }

    #[test]
    fn progress_is_clamped_to_the_stage() {
        let past_the_end = once(&AcRallyTelemetry {
            normalized_car_position: 1.3,
            ..Default::default()
        });
        assert_eq!(past_the_end.completed_pct, 100);

        let before_the_start = once(&AcRallyTelemetry {
            normalized_car_position: -0.2,
            ..Default::default()
        });
        assert_eq!(before_the_start.completed_pct, 0);
    }

    #[test]
    fn distance_prefers_the_metres_the_game_reports() {
        let data = once(&AcRallyTelemetry {
            distance_traveled: 1234.6,
            normalized_car_position: 0.5,
            track_spline_length: 10_000.0,
            ..Default::default()
        });
        assert_eq!(data.distance_travelled, 1235);
    }

    #[test]
    fn distance_falls_back_to_the_spline_before_the_counter_starts() {
        let data = once(&AcRallyTelemetry {
            distance_traveled: 0.0,
            normalized_car_position: 0.25,
            track_spline_length: 10_000.0,
            ..Default::default()
        });
        assert_eq!(data.distance_travelled, 2500);
    }

    #[test]
    fn distance_reads_zero_without_a_stage_length() {
        let data = once(&AcRallyTelemetry {
            normalized_car_position: 0.5,
            ..Default::default()
        });
        assert_eq!(data.distance_travelled, 0);
    }

    #[test]
    fn position_defaults_to_first_on_a_solo_stage() {
        assert_eq!(once(&AcRallyTelemetry::default()).position, 1);

        let data = once(&AcRallyTelemetry {
            position: 4,
            ..Default::default()
        });
        assert_eq!(data.position, 4);
    }

    #[test]
    fn splits_are_recorded_as_the_car_crosses_each_sector() {
        let mut converter = AcRallyConverter::new();

        // Running in the first sector: no split yet.
        let first = rally(
            &mut converter,
            &AcRallyTelemetry {
                current_sector_index: 0,
                current_time: 30_000,
                ..Default::default()
            },
        );
        assert_eq!(first.sector1_time, 0.0);

        // Crossing into the second sector records the first split.
        let second = rally(
            &mut converter,
            &AcRallyTelemetry {
                current_sector_index: 1,
                current_time: 45_500,
                ..Default::default()
            },
        );
        assert_eq!(second.sector1_time, 45.5);
        assert_eq!(second.sector2_time, 0.0);

        // And into the third records the second.
        let third = rally(
            &mut converter,
            &AcRallyTelemetry {
                current_sector_index: 2,
                current_time: 92_250,
                ..Default::default()
            },
        );
        assert_eq!(third.sector1_time, 45.5);
        assert_eq!(third.sector2_time, 92.25);
    }

    #[test]
    fn splits_survive_frames_within_the_same_sector() {
        let mut converter = AcRallyConverter::new();
        rally(
            &mut converter,
            &AcRallyTelemetry {
                current_sector_index: 1,
                current_time: 45_500,
                ..Default::default()
            },
        );
        let later = rally(
            &mut converter,
            &AcRallyTelemetry {
                current_sector_index: 1,
                current_time: 60_000,
                ..Default::default()
            },
        );
        assert_eq!(later.sector1_time, 45.5);
    }

    #[test]
    fn a_restarted_stage_clears_the_splits() {
        let mut converter = AcRallyConverter::new();
        rally(
            &mut converter,
            &AcRallyTelemetry {
                current_sector_index: 1,
                current_time: 45_500,
                ..Default::default()
            },
        );

        // Back to the first sector with the clock reset.
        let restarted = rally(
            &mut converter,
            &AcRallyTelemetry {
                current_sector_index: 0,
                current_time: 0,
                ..Default::default()
            },
        );
        assert_eq!(restarted.sector1_time, 0.0);
        assert_eq!(restarted.sector2_time, 0.0);
    }

    #[test]
    fn a_clock_that_runs_backwards_also_counts_as_a_restart() {
        let mut converter = AcRallyConverter::new();
        rally(
            &mut converter,
            &AcRallyTelemetry {
                current_sector_index: 1,
                current_time: 45_500,
                ..Default::default()
            },
        );

        let restarted = rally(
            &mut converter,
            &AcRallyTelemetry {
                current_sector_index: 1,
                current_time: 1_000,
                ..Default::default()
            },
        );
        assert_eq!(restarted.sector1_time, 0.0);
    }

    #[test]
    fn the_stage_clock_is_reported_in_seconds() {
        let data = once(&AcRallyTelemetry {
            current_time: 123_456,
            ..Default::default()
        });
        assert_eq!(data.lap_time, 123.456);
    }
}

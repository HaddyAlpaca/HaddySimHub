//! Ported from `HaddySimHub/Displays/IRacing/IRacingDataConverter.cs`.
//!
//! Unlike the other converters this one carries state: fuel use is only
//! observable across laps, so the converter tracks stint history and resets it
//! when the session changes or the car leaves the pits.

use simhub_model::{DisplayUpdate, RaceData, telemetry::IRacingSample};

fn gear_label(gear: i32) -> String {
    match gear {
        -1 => "R".to_string(),
        0 => "N".to_string(),
        g => g.to_string(),
    }
}

/// The SDK does not publish a rev limit, so it is hard-coded per car.
pub fn rpm_max_for(car_screen_name: &str) -> i32 {
    match car_screen_name {
        "FIA F4" => 7000,
        _ => 0,
    }
}

#[derive(Debug, Default)]
pub struct IRacingConverter {
    session_num: Option<i32>,
    last_player_lap: i32,
    last_lap_start_fuel: f32,
    last_player_car_in_pit_stall: bool,
    last_on_pit_road: bool,
    fuel_stint_history: Vec<f32>,
}

impl IRacingConverter {
    pub fn new() -> Self {
        Self::default()
    }

    pub fn convert(&mut self, sample: &IRacingSample) -> DisplayUpdate {
        let telemetry = &sample.telemetry;
        let session = &sample.session;

        let current_player_lap = telemetry
            .car_idx_lap
            .get(telemetry.player_car_idx as usize)
            .copied()
            .unwrap_or(0);

        if self.session_num != Some(telemetry.session_num) {
            self.session_num = Some(telemetry.session_num);
            self.last_player_lap = current_player_lap;
            self.last_lap_start_fuel = telemetry.fuel_level;
            self.fuel_stint_history.clear();
            self.last_player_car_in_pit_stall = telemetry.player_car_in_pit_stall;
            self.last_on_pit_road = telemetry.on_pit_road;
        }

        // Leaving the pit stall or pit road starts a fresh stint, so the history
        // gathered before the stop no longer describes the current fuel load.
        let left_pit_stall =
            self.last_player_car_in_pit_stall && !telemetry.player_car_in_pit_stall;
        let left_pit_road = self.last_on_pit_road && !telemetry.on_pit_road;
        if left_pit_stall || left_pit_road {
            self.fuel_stint_history.clear();
            self.last_lap_start_fuel = telemetry.fuel_level;
        }

        if current_player_lap > self.last_player_lap {
            let fuel_used = self.last_lap_start_fuel - telemetry.fuel_level;
            // Refuelling makes the difference negative, which is not consumption.
            if fuel_used > 0.0 && telemetry.fuel_level <= self.last_lap_start_fuel {
                self.fuel_stint_history.push(fuel_used);
            }
            self.last_lap_start_fuel = telemetry.fuel_level;
            self.last_player_lap = current_player_lap;
        }

        self.last_player_car_in_pit_stall = telemetry.player_car_in_pit_stall;
        self.last_on_pit_road = telemetry.on_pit_road;

        let fuel_average = if self.fuel_stint_history.is_empty() {
            0.0
        } else {
            self.fuel_stint_history.iter().sum::<f32>() / self.fuel_stint_history.len() as f32
        };
        let fuel_last_lap = self.fuel_stint_history.last().copied().unwrap_or(0.0);
        let fuel_est_laps = if fuel_average > 0.0 {
            telemetry.fuel_level / fuel_average
        } else {
            0.0
        };

        // The wheel reports -half..+half of its full span; the dashboard wants 0..100.
        let steering_pct = if telemetry.steering_wheel_angle_max == 0.0 {
            50
        } else {
            let span = telemetry.steering_wheel_angle_max as f64;
            let fraction = (telemetry.steering_wheel_angle as f64 + span / 2.0) / span;
            ((fraction * 100.0).round() as i32).clamp(0, 100)
        };

        let strength_of_field = if telemetry.race_car_iratings.len() > 1 {
            let total: i64 = telemetry.race_car_iratings.iter().map(|r| *r as i64).sum();
            (total as f64 / telemetry.race_car_iratings.len() as f64).round() as i32
        } else {
            0
        };

        DisplayUpdate::Race(RaceData {
            session_type: session.session_type.clone(),
            is_limited_time: session.is_limited_time,
            is_limited_session_laps: session.is_limited_session_laps,
            current_lap: telemetry.lap,
            total_laps: session.session_laps,
            session_time_remaining: telemetry.session_time_remain as f32,
            position: Some(telemetry.player_car_position),
            current_lap_time: telemetry.lap_current_lap_time,
            speed: (telemetry.speed as f64 * 3.6).round() as i32,
            gear: gear_label(telemetry.gear),
            rpm: telemetry.rpm as i32,
            rpm_max: rpm_max_for(&telemetry.car_screen_name),
            track_temp: telemetry.track_temp,
            air_temp: telemetry.air_temp,
            fuel_remaining: Some(telemetry.fuel_level),
            fuel_avg_lap: Some(fuel_average),
            fuel_last_lap: Some(fuel_last_lap),
            fuel_est_laps,
            last_lap_time: telemetry.lap_last_lap_time.max(0.0),
            // A non-positive last lap means there is no completed lap to compare.
            last_lap_time_delta: Some(if telemetry.lap_last_lap_time <= 0.0 {
                0.0
            } else {
                telemetry.lap_delta_to_session_last_lap
            }),
            best_lap_time: Some(telemetry.lap_best_lap_time.max(0.0)),
            best_lap_time_delta: Some(if telemetry.lap_delta_to_best_lap <= 0.0 {
                0.0
            } else {
                telemetry.lap_delta_to_best_lap
            }),
            // Not available in standard iRacing telemetry.
            clutch_pct: 0,
            throttle_pct: (telemetry.throttle as f64 * 100.0).round() as i32,
            brake_pct: (telemetry.brake as f64 * 100.0).round() as i32,
            pit_limiter_on: telemetry.pit_speed_limiter,
            steering_pct,
            expected_position: session.player.as_ref().map(|p| p.car_number.clone()),
            brake_bias: Some(telemetry.dc_brake_bias),
            strength_of_field: Some(strength_of_field),
            incidents: Some(telemetry.player_car_driver_incident_count.max(0) as i64),
            max_incidents: Some(session.incident_limit.clamp(0, 999) as i64),
            i_rating: session.player.as_ref().map(|p| p.i_rating),
            safety_rating: session.player.as_ref().map(|p| p.lic_level),
            ..Default::default()
        })
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use simhub_model::telemetry::{IRacingDriver, IRacingSession, IRacingTelemetry};

    fn sample(telemetry: IRacingTelemetry) -> IRacingSample {
        IRacingSample {
            telemetry,
            session: IRacingSession::default(),
        }
    }

    /// A frame for the player, who is car 0, on the given lap with the given fuel.
    fn lap_frame(lap: i32, fuel: f32) -> IRacingTelemetry {
        IRacingTelemetry {
            player_car_idx: 0,
            car_idx_lap: vec![lap],
            fuel_level: fuel,
            ..Default::default()
        }
    }

    fn race(update: DisplayUpdate) -> RaceData {
        match update {
            DisplayUpdate::Race(data) => data,
            other => panic!("expected a race dashboard, got {other:?}"),
        }
    }

    #[test]
    fn gear_labels_differ_from_the_assetto_encoding() {
        assert_eq!(gear_label(-1), "R");
        assert_eq!(gear_label(0), "N");
        assert_eq!(gear_label(3), "3");
    }

    #[test]
    fn speed_converts_metres_per_second_to_kilometres_per_hour() {
        let mut converter = IRacingConverter::new();
        let data = race(converter.convert(&sample(IRacingTelemetry {
            speed: 30.0,
            ..Default::default()
        })));
        assert_eq!(data.speed, 108);
    }

    #[test]
    fn halves_round_away_from_zero() {
        // 0.125 * 100 = 12.5. C# rounded this to 12 because .NET defaults to
        // banker's rounding, which was incidental rather than intended.
        let mut converter = IRacingConverter::new();
        let data = race(converter.convert(&sample(IRacingTelemetry {
            throttle: 0.125,
            ..Default::default()
        })));
        assert_eq!(data.throttle_pct, 13);
    }

    #[test]
    fn fuel_use_accumulates_across_laps() {
        let mut converter = IRacingConverter::new();
        converter.convert(&sample(lap_frame(1, 50.0)));
        converter.convert(&sample(lap_frame(2, 47.0)));
        let data = race(converter.convert(&sample(lap_frame(3, 45.0))));

        assert_eq!(data.fuel_last_lap, Some(2.0));
        assert_eq!(data.fuel_avg_lap, Some(2.5));
        // 45 litres left at 2.5 per lap.
        assert_eq!(data.fuel_est_laps, 18.0);
    }

    #[test]
    fn refuelling_is_not_counted_as_consumption() {
        let mut converter = IRacingConverter::new();
        converter.convert(&sample(lap_frame(1, 20.0)));
        // Fuel went up, so the lap tells us nothing about consumption.
        let data = race(converter.convert(&sample(lap_frame(2, 60.0))));

        assert_eq!(data.fuel_avg_lap, Some(0.0));
        assert_eq!(data.fuel_est_laps, 0.0);
    }

    #[test]
    fn leaving_the_pits_starts_a_fresh_stint() {
        let mut converter = IRacingConverter::new();
        converter.convert(&sample(lap_frame(1, 50.0)));
        converter.convert(&sample(lap_frame(2, 47.0)));

        let mut in_pits = lap_frame(2, 47.0);
        in_pits.on_pit_road = true;
        converter.convert(&sample(in_pits));

        // Back on track: the history from before the stop is discarded.
        let data = race(converter.convert(&sample(lap_frame(2, 60.0))));
        assert_eq!(data.fuel_avg_lap, Some(0.0));
    }

    #[test]
    fn a_new_session_resets_the_stint_history() {
        let mut converter = IRacingConverter::new();
        converter.convert(&sample(lap_frame(1, 50.0)));
        converter.convert(&sample(lap_frame(2, 47.0)));

        let mut next_session = lap_frame(1, 50.0);
        next_session.session_num = 1;
        let data = race(converter.convert(&sample(next_session)));
        assert_eq!(data.fuel_avg_lap, Some(0.0));
    }

    #[test]
    fn steering_maps_the_wheel_span_onto_a_percentage() {
        let mut converter = IRacingConverter::new();
        let centred = race(converter.convert(&sample(IRacingTelemetry {
            steering_wheel_angle: 0.0,
            steering_wheel_angle_max: 12.0,
            ..Default::default()
        })));
        assert_eq!(centred.steering_pct, 50);

        let full_left = race(converter.convert(&sample(IRacingTelemetry {
            steering_wheel_angle: -6.0,
            steering_wheel_angle_max: 12.0,
            ..Default::default()
        })));
        assert_eq!(full_left.steering_pct, 0);

        let full_right = race(converter.convert(&sample(IRacingTelemetry {
            steering_wheel_angle: 6.0,
            steering_wheel_angle_max: 12.0,
            ..Default::default()
        })));
        assert_eq!(full_right.steering_pct, 100);
    }

    #[test]
    fn steering_falls_back_to_centre_when_the_span_is_unknown() {
        let mut converter = IRacingConverter::new();
        let data = race(converter.convert(&sample(IRacingTelemetry {
            steering_wheel_angle: 3.0,
            steering_wheel_angle_max: 0.0,
            ..Default::default()
        })));
        assert_eq!(data.steering_pct, 50);
    }

    #[test]
    fn steering_clamps_beyond_the_reported_span() {
        let mut converter = IRacingConverter::new();
        let data = race(converter.convert(&sample(IRacingTelemetry {
            steering_wheel_angle: 90.0,
            steering_wheel_angle_max: 12.0,
            ..Default::default()
        })));
        assert_eq!(data.steering_pct, 100);
    }

    #[test]
    fn strength_of_field_needs_more_than_one_car() {
        let mut converter = IRacingConverter::new();
        let alone = race(converter.convert(&sample(IRacingTelemetry {
            race_car_iratings: vec![3000],
            ..Default::default()
        })));
        assert_eq!(alone.strength_of_field, Some(0));

        let field = race(converter.convert(&sample(IRacingTelemetry {
            race_car_iratings: vec![3000, 2000],
            ..Default::default()
        })));
        assert_eq!(field.strength_of_field, Some(2500));
    }

    #[test]
    fn incident_limit_is_clamped() {
        let mut converter = IRacingConverter::new();
        let update = converter.convert(&IRacingSample {
            telemetry: IRacingTelemetry::default(),
            session: IRacingSession {
                incident_limit: i32::MAX,
                ..Default::default()
            },
        });
        assert_eq!(race(update).max_incidents, Some(999));
    }

    #[test]
    fn deltas_are_zero_until_a_lap_is_completed() {
        let mut converter = IRacingConverter::new();
        let data = race(converter.convert(&sample(IRacingTelemetry {
            lap_last_lap_time: -1.0,
            lap_delta_to_session_last_lap: 4.2,
            lap_best_lap_time: -1.0,
            ..Default::default()
        })));
        assert_eq!(data.last_lap_time, 0.0);
        assert_eq!(data.last_lap_time_delta, Some(0.0));
        assert_eq!(data.best_lap_time, Some(0.0));
    }

    #[test]
    fn driver_details_come_from_the_resolved_player() {
        let mut converter = IRacingConverter::new();
        let update = converter.convert(&IRacingSample {
            telemetry: IRacingTelemetry::default(),
            session: IRacingSession {
                player: Some(IRacingDriver {
                    car_number: "42".to_string(),
                    i_rating: 2750,
                    lic_level: 18,
                }),
                ..Default::default()
            },
        });
        let data = race(update);
        assert_eq!(data.expected_position.as_deref(), Some("42"));
        assert_eq!(data.i_rating, Some(2750));
        assert_eq!(data.safety_rating, Some(18));
    }

    #[test]
    fn rpm_max_is_known_for_a_few_cars_only() {
        assert_eq!(rpm_max_for("FIA F4"), 7000);
        assert_eq!(rpm_max_for("Something Else"), 0);
    }
}

//! Ported from `HaddySimHub/Displays/ETS/EtsDataConverter.cs`.
//!
//! Stateful for one reason: average fuel consumption reads zero whenever the
//! game has nothing to report, which would make the dashboard flicker between a
//! figure and nothing. The converter keeps the last non-zero value instead.

use simhub_model::{DisplayUpdate, TruckData, telemetry::EtsTelemetry};

/// The rev band the gear advice aims for.
const GEAR_ADVICE_TARGET_RPM: f32 = 1300.0;
/// Below this the truck is manoeuvring, where gear advice is noise.
const GEAR_ADVICE_MIN_SPEED_KPH: f32 = 15.0;

/// ETS2 numbers gears from 1, but a 14-speed box is a 12-speed with two crawler
/// gears in front, so its labels are shifted.
fn format_gear(gear: i32, forward_gear_count: u32) -> String {
    if gear == 0 {
        return "N".to_string();
    }
    if gear < 0 {
        return format!("R{}", gear.abs());
    }
    if forward_gear_count == 14 {
        return match gear {
            1 => "C1".to_string(),
            2 => "C2".to_string(),
            g => (g - 2).to_string(),
        };
    }
    gear.to_string()
}

/// Picks the forward gear whose predicted engine speed sits closest to the
/// target band, skipping any gear that would over-rev. Returns `None` while
/// manoeuvring or when the truck constants are not populated yet.
fn recommend_forward_gear(data: &EtsTelemetry) -> Option<i32> {
    let constants = &data.truck.constants;
    let ratios = &constants.gear_ratios_forward;
    let differential = constants.differential_ratio;
    let rpm_max = constants.engine_rpm_max;

    if ratios.is_empty()
        || differential <= 0.0
        || rpm_max <= 0.0
        || constants.wheel_radii.is_empty()
    {
        return None;
    }

    let wheel_radius = constants
        .wheel_radii
        .iter()
        .copied()
        .find(|radius| *radius > 0.0)
        .unwrap_or(0.0);
    if wheel_radius <= 0.0 {
        return None;
    }

    let speed_mps = data.truck.dashboard.speed_mps.abs();
    if speed_mps * 3.6 < GEAR_ADVICE_MIN_SPEED_KPH {
        return None;
    }

    let wheel_revs_per_second = speed_mps / (2.0 * std::f32::consts::PI * wheel_radius);

    let mut best_gear = None;
    let mut best_score = f32::MAX;
    for (index, ratio) in ratios.iter().copied().enumerate() {
        if ratio <= 0.0 {
            continue;
        }

        let predicted_rpm = wheel_revs_per_second * 60.0 * differential * ratio;
        if predicted_rpm > rpm_max * 0.98 {
            continue;
        }

        let score = (predicted_rpm - GEAR_ADVICE_TARGET_RPM).abs();
        if score < best_score {
            best_score = score;
            best_gear = Some(index as i32 + 1);
        }
    }

    best_gear
}

#[derive(Debug, Default)]
pub struct EtsConverter {
    fuel_average_consumption: f32,
}

impl EtsConverter {
    pub fn new() -> Self {
        Self::default()
    }

    pub fn convert(&mut self, data: &EtsTelemetry) -> DisplayUpdate {
        let dashboard = &data.truck.dashboard;
        let constants = &data.truck.constants;
        let scale = data.common.scale;

        // Reported per unit; the dashboard shows litres per 100 km, to one decimal.
        let consumption = (dashboard.fuel_average_consumption * 100.0 * 10.0).round() / 10.0;
        if consumption > 0.0 {
            self.fuel_average_consumption = consumption;
        }

        let gear = format_gear(data.truck.selected_gear, constants.forward_gear_count);
        let recommended_gear = recommend_forward_gear(data)
            .map(|advice| format_gear(advice, constants.forward_gear_count))
            .unwrap_or_default();

        let navigation_minutes = data.navigation.time_seconds.max(0.0) / 60.0;
        let rest_minutes = data.common.next_rest_stop_minutes.max(0);
        let job_minutes = data.job.remaining_delivery_time_minutes.max(0);

        let trailer_damage = |pick: fn(&simhub_model::telemetry::ets::EtsTrailerDamage) -> f32| {
            if data.trailers.is_empty() {
                0
            } else {
                let total: f32 = data.trailers.iter().map(pick).sum();
                ((total / data.trailers.len() as f32) * 100.0).round() as i32
            }
        };

        DisplayUpdate::Truck(TruckData {
            source_city: data.job.source_city.clone(),
            source_company: data.job.source_company.clone(),
            destination_city: data.job.destination_city.clone(),
            destination_company: data.job.destination_company.clone(),
            distance_remaining: (data.navigation.distance_metres.max(0.0) / 1000.0).round(),
            time_remaining: navigation_minutes.round() as i32,
            time_remaining_irl: (navigation_minutes / scale).round() as i32,
            rest_time_remaining: rest_minutes,
            rest_time_remaining_irl: (rest_minutes as f32 / scale).round() as i32,
            game_time: data.common.game_time,

            job_time_remaining: job_minutes,
            job_time_remaining_irl: (job_minutes as f32 / scale).round() as i64,
            job_income: data.job.income,
            job_cargo_name: data.job.cargo_name.clone(),
            job_cargo_mass: data.job.cargo_mass.ceil() as i32,
            job_cargo_damage: (data.job.cargo_damage * 100.0).round() as i32,

            damage_truck_cabin: (data.truck.damage.cabin * 100.0).round() as i32,
            damage_truck_wheels: (data.truck.damage.wheels_avg * 100.0).round() as i32,
            damage_truck_transmission: (data.truck.damage.transmission * 100.0).round() as i32,
            damage_truck_engine: (data.truck.damage.engine * 100.0).round() as i32,
            damage_truck_chassis: (data.truck.damage.chassis * 100.0).round() as i32,
            damage_trailer_chassis: trailer_damage(|t| t.chassis),
            damage_trailer_cargo: trailer_damage(|t| t.cargo),
            damage_trailer_wheels: trailer_damage(|t| t.wheels),
            damage_trailer_body: trailer_damage(|t| t.body),
            number_of_trailers_attached: data.trailers.len() as i32,

            truck_name: format!("{} {}", constants.brand, constants.name),
            gear,
            recommended_gear,
            rpm: dashboard.rpm as i32,
            rpm_max: constants.engine_rpm_max as i32,
            speed: dashboard.speed_kph.max(0.0).round() as i16,
            speed_limit: data.navigation.speed_limit_kph.max(0.0).round() as i16,
            cruise_control_on: dashboard.cruise_control,
            cruise_control_speed: dashboard.cruise_control_speed_kph.round() as i16,
            throttle: (data.throttle * 100.0).round() as i32,
            engine_on: data.truck.engine_enabled,
            odometer: dashboard.odometer,

            fuel_distance: dashboard.fuel_range,
            fuel_amount: dashboard.fuel_amount,
            fuel_capacity: constants.fuel_capacity,
            fuel_average_consumption: self.fuel_average_consumption,
            fuel_warning_on: data.truck.warnings.fuel,
            ad_blue_amount: dashboard.ad_blue,
            ad_blue_capacity: constants.ad_blue_capacity,
            ad_blue_warning_on: data.truck.warnings.ad_blue,

            parking_lights_on: data.truck.lights.parking,
            low_beam_on: data.truck.lights.beam_low,
            high_beam_on: data.truck.lights.beam_high,
            hazard_lights_on: data.truck.lights.hazard_warning,
            blinker_left_on: data.truck.lights.blinker_left_on,
            blinker_right_on: data.truck.lights.blinker_right_on,
            beacon_on: data.truck.lights.beacon,
            dashboard_backlight: data.truck.lights.dashboard_backlight,

            parking_brake_on: data.truck.brakes.parking_brake,
            motor_brake_on: data.truck.brakes.motor_brake,
            brake_temp: data.truck.brakes.temperature,
            brake_air_pressure: data.truck.brakes.air_pressure,
            air_pressure_warning_on: data.truck.warnings.air_pressure,
            air_pressure_emergency_on: data.truck.warnings.air_pressure_emergency,
            retarder_level: data.truck.brakes.retarder_level,
            retarder_step_count: constants.retarder_step_count,
            differential_lock: data.truck.differential_lock,
            lift_axle_indicator_on: data.truck.lift_axle_indicator,

            oil_pressure: dashboard.oil_pressure,
            oil_pressure_warning_on: data.truck.warnings.oil_pressure,
            oil_temp: dashboard.oil_temperature,
            water_temp: dashboard.water_temperature,
            water_temp_warning_on: data.truck.warnings.water_temperature,
            battery_voltage: dashboard.battery_voltage,
            battery_voltage_warning_on: data.truck.warnings.battery_voltage,

            wipers_on: dashboard.wipers,
        })
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use simhub_model::telemetry::ets::*;

    /// A telemetry frame with a usable world scale, so the "irl" divisions work.
    fn frame() -> EtsTelemetry {
        EtsTelemetry {
            common: EtsCommon {
                scale: 20.0,
                ..Default::default()
            },
            ..Default::default()
        }
    }

    fn truck(update: DisplayUpdate) -> TruckData {
        match update {
            DisplayUpdate::Truck(data) => data,
            other => panic!("expected a truck dashboard, got {other:?}"),
        }
    }

    fn convert(data: &EtsTelemetry) -> TruckData {
        truck(EtsConverter::new().convert(data))
    }

    #[test]
    fn gear_labels_cover_neutral_and_reverse() {
        assert_eq!(format_gear(0, 12), "N");
        assert_eq!(format_gear(-1, 12), "R1");
        assert_eq!(format_gear(-2, 12), "R2");
        assert_eq!(format_gear(5, 12), "5");
    }

    #[test]
    fn a_fourteen_speed_box_labels_its_crawler_gear() {
        // A 14-speed is a 12-speed with two crawlers ahead of it, so the labels shift.
        assert_eq!(format_gear(1, 14), "C1");
        assert_eq!(format_gear(2, 14), "C2");
        // The twelve ordinary gears follow the crawlers.
        assert_eq!(format_gear(3, 14), "1");
        assert_eq!(format_gear(14, 14), "12");
    }

    #[test]
    fn fuel_consumption_keeps_the_last_reported_figure() {
        let mut converter = EtsConverter::new();
        let mut data = frame();
        data.truck.dashboard.fuel_average_consumption = 0.3456;
        let first = truck(converter.convert(&data));
        // Scaled to litres per 100 km and rounded to one decimal.
        assert_eq!(first.fuel_average_consumption, 34.6);

        // The game reports nothing; the dashboard should not blank out.
        data.truck.dashboard.fuel_average_consumption = 0.0;
        let second = truck(converter.convert(&data));
        assert_eq!(second.fuel_average_consumption, 34.6);
    }

    #[test]
    fn gear_advice_is_withheld_while_manoeuvring() {
        let mut data = frame();
        data.truck.constants.gear_ratios_forward = vec![1.0, 1.2];
        data.truck.constants.differential_ratio = 3.0;
        data.truck.constants.engine_rpm_max = 2500.0;
        data.truck.constants.wheel_radii = vec![0.5];
        // 2 m/s is 7.2 km/h, below the 15 km/h floor.
        data.truck.dashboard.speed_mps = 2.0;

        assert_eq!(convert(&data).recommended_gear, "");
    }

    #[test]
    fn gear_advice_is_withheld_until_the_truck_constants_arrive() {
        let mut data = frame();
        data.truck.dashboard.speed_mps = 20.0;
        assert_eq!(convert(&data).recommended_gear, "");
    }

    #[test]
    fn gear_advice_picks_the_ratio_closest_to_the_target_band() {
        let mut data = frame();
        data.truck.constants.forward_gear_count = 12;
        data.truck.constants.gear_ratios_forward = vec![1.0, 1.2, 2.0, 3.0];
        data.truck.constants.differential_ratio = 3.0;
        data.truck.constants.engine_rpm_max = 2500.0;
        data.truck.constants.wheel_radii = vec![0.5];
        data.truck.dashboard.speed_mps = 20.0;

        // At 20 m/s the second ratio lands nearest 1300 rpm, and the fourth
        // would over-rev, so it is not considered.
        assert_eq!(convert(&data).recommended_gear, "2");
    }

    #[test]
    fn gear_advice_is_withheld_when_every_gear_would_over_rev() {
        let mut data = frame();
        data.truck.constants.gear_ratios_forward = vec![10.0];
        data.truck.constants.differential_ratio = 3.0;
        data.truck.constants.engine_rpm_max = 2500.0;
        data.truck.constants.wheel_radii = vec![0.5];
        data.truck.dashboard.speed_mps = 20.0;

        assert_eq!(convert(&data).recommended_gear, "");
    }

    #[test]
    fn gear_advice_skips_a_leading_zero_wheel_radius() {
        let mut data = frame();
        data.truck.constants.forward_gear_count = 12;
        data.truck.constants.gear_ratios_forward = vec![1.2];
        data.truck.constants.differential_ratio = 3.0;
        data.truck.constants.engine_rpm_max = 2500.0;
        // Unpowered axles report a zero radius; the first real wheel is used.
        data.truck.constants.wheel_radii = vec![0.0, 0.5];
        data.truck.dashboard.speed_mps = 20.0;

        assert_eq!(convert(&data).recommended_gear, "1");
    }

    #[test]
    fn trailer_damage_averages_across_attached_trailers() {
        let mut data = frame();
        data.trailers = vec![
            EtsTrailerDamage {
                chassis: 0.10,
                cargo: 0.20,
                wheels: 0.30,
                body: 0.40,
            },
            EtsTrailerDamage {
                chassis: 0.20,
                cargo: 0.40,
                wheels: 0.50,
                body: 0.60,
            },
        ];

        let result = convert(&data);
        assert_eq!(result.number_of_trailers_attached, 2);
        assert_eq!(result.damage_trailer_chassis, 15);
        assert_eq!(result.damage_trailer_cargo, 30);
        assert_eq!(result.damage_trailer_wheels, 40);
        assert_eq!(result.damage_trailer_body, 50);
    }

    #[test]
    fn trailer_damage_reads_zero_without_a_trailer() {
        let result = convert(&frame());
        assert_eq!(result.number_of_trailers_attached, 0);
        assert_eq!(result.damage_trailer_chassis, 0);
    }

    #[test]
    fn real_time_estimates_divide_by_the_world_scale() {
        let mut data = frame();
        data.navigation.time_seconds = 3600.0;
        data.common.next_rest_stop_minutes = 200;
        data.job.remaining_delivery_time_minutes = 400;

        let result = convert(&data);
        assert_eq!(result.time_remaining, 60);
        assert_eq!(result.time_remaining_irl, 3);
        assert_eq!(result.rest_time_remaining, 200);
        assert_eq!(result.rest_time_remaining_irl, 10);
        assert_eq!(result.job_time_remaining, 400);
        assert_eq!(result.job_time_remaining_irl, 20);
    }

    #[test]
    fn negative_countdowns_are_floored_at_zero() {
        let mut data = frame();
        data.navigation.time_seconds = -500.0;
        data.navigation.distance_metres = -1000.0;
        data.common.next_rest_stop_minutes = -30;
        data.job.remaining_delivery_time_minutes = -90;

        let result = convert(&data);
        assert_eq!(result.time_remaining, 0);
        assert_eq!(result.distance_remaining, 0.0);
        assert_eq!(result.rest_time_remaining, 0);
        assert_eq!(result.job_time_remaining, 0);
    }

    #[test]
    fn distance_is_reported_in_kilometres() {
        let mut data = frame();
        data.navigation.distance_metres = 123_400.0;
        assert_eq!(convert(&data).distance_remaining, 123.0);
    }

    #[test]
    fn cargo_mass_rounds_up() {
        let mut data = frame();
        data.job.cargo_mass = 12_000.2;
        assert_eq!(convert(&data).job_cargo_mass, 12_001);
    }

    #[test]
    fn truck_name_joins_brand_and_model() {
        let mut data = frame();
        data.truck.constants.brand = "Scania".to_string();
        data.truck.constants.name = "R 2016".to_string();
        assert_eq!(convert(&data).truck_name, "Scania R 2016");
    }

    #[test]
    fn convert_returns_a_truck_dashboard() {
        use simhub_model::DisplayType;
        let update = EtsConverter::new().convert(&frame());
        assert_eq!(update.display_type(), DisplayType::TruckDashboard);
    }
}

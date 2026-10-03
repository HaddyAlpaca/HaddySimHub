//! Decodes the `Local\SCSTelemetry` shared memory map.
//!
//! # Where this layout comes from
//!
//! The map is written by the community `scs-sdk-plugin`, not by the SCS SDK
//! itself, so its layout is that plugin's contract and moves when the plugin
//! does. These offsets were derived by walking `SCSSdkClient/SCSSdkConvert.cs`,
//! which reads the map as a cursor rather than as a struct.
//!
//! # Zones
//!
//! The cursor does not run straight through. It fills fourteen zones that each
//! start at a fixed offset, jumping to the next regardless of where reading
//! left off. That is a useful property: a mistake inside one zone cannot shift
//! the ones after it. The arithmetic checks out at both ends — the last string
//! zone ends exactly where the trailer zone begins.
//!
//! # Alignment
//!
//! The C# getters for `u32`, `i32`, `f32` and `f64` round the cursor up to the
//! next multiple of four before reading; the ones for `bool` and 64-bit
//! integers do not. The offsets below already account for that.

use crate::read::{f32_at, i32_at};
use simhub_model::telemetry::ets::*;

pub const MAP_NAME: &str = r"Local\SCSTelemetry";

/// The map the plugin publishes is a fixed 32 KiB.
pub const MAP_SIZE: usize = 32 * 1024;

/// Where each zone begins, from `SCSSdkConvert._offsetAreas`.
pub const ZONE_STARTS: [usize; 14] = [
    0, 40, 500, 700, 1500, 1640, 2000, 2200, 2300, 4000, 4200, 4300, 4400, 6000,
];

/// Fixed width of every string field in the map.
const STRING_SIZE: usize = 64;

/// Zone 2: unsigned integers.
mod zone2 {
    pub const GAME_TIME: usize = 64;
    pub const FORWARD_GEAR_COUNT: usize = 68;
    pub const RETARDER_STEP_COUNT: usize = 76;
    pub const DELIVERY_TIME: usize = 88;
    pub const RETARDER_LEVEL: usize = 108;
}

/// Zone 3: signed integers.
mod zone3 {
    pub const NEXT_REST_STOP: usize = 500;
    pub const SELECTED_GEAR: usize = 504;
}

/// Zone 4: floats.
mod zone4 {
    pub const SCALE: usize = 700;
    pub const FUEL_CAPACITY: usize = 704;
    pub const ADBLUE_CAPACITY: usize = 712;
    pub const ENGINE_RPM_MAX: usize = 740;
    pub const DIFFERENTIAL_RATIO: usize = 744;
    pub const CARGO_MASS: usize = 748;
    pub const WHEEL_RADII: usize = 752;
    pub const GEAR_RATIOS_FORWARD: usize = 816;
    pub const SPEED: usize = 948;
    pub const RPM: usize = 952;
    pub const GAME_THROTTLE: usize = 976;
    pub const CRUISE_CONTROL_SPEED: usize = 988;
    pub const BRAKE_AIR_PRESSURE: usize = 992;
    pub const BRAKE_TEMPERATURE: usize = 996;
    pub const FUEL_AMOUNT: usize = 1000;
    pub const FUEL_AVERAGE_CONSUMPTION: usize = 1004;
    pub const FUEL_RANGE: usize = 1008;
    pub const ADBLUE: usize = 1012;
    pub const OIL_PRESSURE: usize = 1016;
    pub const OIL_TEMPERATURE: usize = 1020;
    pub const WATER_TEMPERATURE: usize = 1024;
    pub const BATTERY_VOLTAGE: usize = 1028;
    pub const DASHBOARD_BACKLIGHT: usize = 1032;
    pub const DAMAGE_ENGINE: usize = 1036;
    pub const DAMAGE_TRANSMISSION: usize = 1040;
    pub const DAMAGE_CABIN: usize = 1044;
    pub const DAMAGE_CHASSIS: usize = 1048;
    pub const DAMAGE_WHEELS_AVG: usize = 1052;
    pub const ODOMETER: usize = 1056;
    pub const NAVIGATION_DISTANCE: usize = 1060;
    pub const NAVIGATION_TIME: usize = 1064;
    pub const SPEED_LIMIT: usize = 1068;
    pub const CARGO_DAMAGE: usize = 1468;
}

/// Zone 5: booleans, one byte each and unaligned.
mod zone5 {
    pub const PARKING_BRAKE: usize = 1566;
    pub const MOTOR_BRAKE: usize = 1567;
    pub const WARN_AIR_PRESSURE: usize = 1568;
    pub const WARN_AIR_PRESSURE_EMERGENCY: usize = 1569;
    pub const WARN_FUEL: usize = 1570;
    pub const WARN_ADBLUE: usize = 1571;
    pub const WARN_OIL_PRESSURE: usize = 1572;
    pub const WARN_WATER_TEMPERATURE: usize = 1573;
    pub const WARN_BATTERY_VOLTAGE: usize = 1574;
    pub const ENGINE_ENABLED: usize = 1576;
    pub const WIPERS: usize = 1577;
    pub const BLINKER_LEFT_ON: usize = 1580;
    pub const BLINKER_RIGHT_ON: usize = 1581;
    pub const PARKING_LIGHTS: usize = 1582;
    pub const BEAM_LOW: usize = 1583;
    pub const BEAM_HIGH: usize = 1584;
    pub const BEACON: usize = 1585;
    pub const HAZARD_WARNING: usize = 1588;
    pub const CRUISE_CONTROL: usize = 1589;
    pub const DIFFERENTIAL_LOCK: usize = 1608;
    pub const LIFT_AXLE_INDICATOR: usize = 1610;
}

/// Zone 9: fixed-width UTF-8 strings.
mod zone9 {
    pub const TRUCK_BRAND: usize = 2364;
    pub const TRUCK_NAME: usize = 2492;
    pub const CARGO_NAME: usize = 2620;
    pub const CITY_DESTINATION: usize = 2748;
    pub const COMPANY_DESTINATION: usize = 2876;
    pub const CITY_SOURCE: usize = 3004;
    pub const COMPANY_SOURCE: usize = 3132;
}

/// Zone 10: the job income, a 64-bit value that is not alignment-adjusted.
mod zone10 {
    pub const INCOME: usize = 4000;
}

/// Zone 14: ten trailer blocks of identical shape.
mod trailers {
    pub const START: usize = 6000;
    /// What one trailer block consumes: 84 bytes of flags, 68 of wheel
    /// substances, 464 of floats, 256 of vectors, 48 of placement, 640 of
    /// strings.
    pub const STRIDE: usize = 1560;
    pub const COUNT: usize = 10;

    // Offsets within one block.
    pub const ATTACHED: usize = 80;
    pub const DAMAGE_CARGO: usize = 152;
    pub const DAMAGE_CHASSIS: usize = 156;
    pub const DAMAGE_WHEELS: usize = 160;
    pub const DAMAGE_BODY: usize = 164;
}

fn bool_at(map: &[u8], offset: usize) -> bool {
    map[offset] > 0
}

fn u32_at(map: &[u8], offset: usize) -> u32 {
    u32::from_le_bytes(map[offset..offset + 4].try_into().expect("checked length"))
}

fn u64_at(map: &[u8], offset: usize) -> u64 {
    u64::from_le_bytes(map[offset..offset + 8].try_into().expect("checked length"))
}

fn f32_array(map: &[u8], offset: usize, count: usize) -> Vec<f32> {
    (0..count).map(|i| f32_at(map, offset + i * 4)).collect()
}

/// Reads a fixed-width field as UTF-8, treating NULs as padding.
fn string_at(map: &[u8], offset: usize) -> String {
    String::from_utf8_lossy(&map[offset..offset + STRING_SIZE])
        .replace('\0', " ")
        .trim()
        .to_string()
}

/// Metres per second to kilometres per hour, which is what the dashboard shows.
fn kph(metres_per_second: f32) -> f32 {
    metres_per_second * 3.6
}

/// Decodes the map, or `None` when it is shorter than the plugin publishes.
pub fn decode(map: &[u8]) -> Option<EtsTelemetry> {
    if map.len() < MAP_SIZE {
        return None;
    }

    let game_time = u32_at(map, zone2::GAME_TIME);
    let delivery_time = u32_at(map, zone2::DELIVERY_TIME);

    Some(EtsTelemetry {
        common: EtsCommon {
            scale: f32_at(map, zone4::SCALE),
            next_rest_stop_minutes: i32_at(map, zone3::NEXT_REST_STOP),
            game_time: game_time as u64,
        },
        job: EtsJob {
            source_city: string_at(map, zone9::CITY_SOURCE),
            source_company: string_at(map, zone9::COMPANY_SOURCE),
            destination_city: string_at(map, zone9::CITY_DESTINATION),
            destination_company: string_at(map, zone9::COMPANY_DESTINATION),
            remaining_delivery_time_minutes: remaining_delivery_time(delivery_time, game_time),
            income: u64_at(map, zone10::INCOME),
            cargo_name: string_at(map, zone9::CARGO_NAME),
            cargo_mass: f32_at(map, zone4::CARGO_MASS),
            cargo_damage: f32_at(map, zone4::CARGO_DAMAGE),
        },
        navigation: EtsNavigation {
            distance_metres: f32_at(map, zone4::NAVIGATION_DISTANCE),
            time_seconds: f32_at(map, zone4::NAVIGATION_TIME),
            speed_limit_kph: kph(f32_at(map, zone4::SPEED_LIMIT)),
        },
        truck: EtsTruck {
            constants: EtsTruckConstants {
                brand: string_at(map, zone9::TRUCK_BRAND),
                name: string_at(map, zone9::TRUCK_NAME),
                forward_gear_count: u32_at(map, zone2::FORWARD_GEAR_COUNT),
                engine_rpm_max: f32_at(map, zone4::ENGINE_RPM_MAX),
                gear_ratios_forward: f32_array(map, zone4::GEAR_RATIOS_FORWARD, 24),
                differential_ratio: f32_at(map, zone4::DIFFERENTIAL_RATIO),
                retarder_step_count: u32_at(map, zone2::RETARDER_STEP_COUNT),
                wheel_radii: f32_array(map, zone4::WHEEL_RADII, 16),
                fuel_capacity: f32_at(map, zone4::FUEL_CAPACITY),
                ad_blue_capacity: f32_at(map, zone4::ADBLUE_CAPACITY),
            },
            dashboard: EtsDashboard {
                rpm: f32_at(map, zone4::RPM),
                speed_kph: kph(f32_at(map, zone4::SPEED)),
                speed_mps: f32_at(map, zone4::SPEED),
                cruise_control: bool_at(map, zone5::CRUISE_CONTROL),
                cruise_control_speed_kph: kph(f32_at(map, zone4::CRUISE_CONTROL_SPEED)),
                fuel_average_consumption: f32_at(map, zone4::FUEL_AVERAGE_CONSUMPTION),
                fuel_range: f32_at(map, zone4::FUEL_RANGE),
                fuel_amount: f32_at(map, zone4::FUEL_AMOUNT),
                ad_blue: f32_at(map, zone4::ADBLUE),
                odometer: f32_at(map, zone4::ODOMETER),
                oil_pressure: f32_at(map, zone4::OIL_PRESSURE),
                oil_temperature: f32_at(map, zone4::OIL_TEMPERATURE),
                water_temperature: f32_at(map, zone4::WATER_TEMPERATURE),
                battery_voltage: f32_at(map, zone4::BATTERY_VOLTAGE),
                wipers: bool_at(map, zone5::WIPERS),
            },
            warnings: EtsWarnings {
                fuel: bool_at(map, zone5::WARN_FUEL),
                ad_blue: bool_at(map, zone5::WARN_ADBLUE),
                oil_pressure: bool_at(map, zone5::WARN_OIL_PRESSURE),
                water_temperature: bool_at(map, zone5::WARN_WATER_TEMPERATURE),
                battery_voltage: bool_at(map, zone5::WARN_BATTERY_VOLTAGE),
                air_pressure: bool_at(map, zone5::WARN_AIR_PRESSURE),
                air_pressure_emergency: bool_at(map, zone5::WARN_AIR_PRESSURE_EMERGENCY),
            },
            lights: EtsLights {
                parking: bool_at(map, zone5::PARKING_LIGHTS),
                beam_low: bool_at(map, zone5::BEAM_LOW),
                beam_high: bool_at(map, zone5::BEAM_HIGH),
                hazard_warning: bool_at(map, zone5::HAZARD_WARNING),
                blinker_left_on: bool_at(map, zone5::BLINKER_LEFT_ON),
                blinker_right_on: bool_at(map, zone5::BLINKER_RIGHT_ON),
                beacon: bool_at(map, zone5::BEACON),
                dashboard_backlight: f32_at(map, zone4::DASHBOARD_BACKLIGHT),
            },
            brakes: EtsBrakes {
                parking_brake: bool_at(map, zone5::PARKING_BRAKE),
                motor_brake: bool_at(map, zone5::MOTOR_BRAKE),
                temperature: f32_at(map, zone4::BRAKE_TEMPERATURE),
                air_pressure: f32_at(map, zone4::BRAKE_AIR_PRESSURE),
                retarder_level: u32_at(map, zone2::RETARDER_LEVEL),
            },
            damage: EtsTruckDamage {
                cabin: f32_at(map, zone4::DAMAGE_CABIN),
                wheels_avg: f32_at(map, zone4::DAMAGE_WHEELS_AVG),
                transmission: f32_at(map, zone4::DAMAGE_TRANSMISSION),
                engine: f32_at(map, zone4::DAMAGE_ENGINE),
                chassis: f32_at(map, zone4::DAMAGE_CHASSIS),
            },
            selected_gear: i32_at(map, zone3::SELECTED_GEAR),
            engine_enabled: bool_at(map, zone5::ENGINE_ENABLED),
            differential_lock: bool_at(map, zone5::DIFFERENTIAL_LOCK),
            lift_axle_indicator: bool_at(map, zone5::LIFT_AXLE_INDICATOR),
        },
        throttle: f32_at(map, zone4::GAME_THROTTLE),
        trailers: attached_trailers(map),
    })
}

/// The job clock counts down from the delivery deadline, and only once the game
/// clock is running and a deadline exists.
fn remaining_delivery_time(delivery_time: u32, game_time: u32) -> i64 {
    if game_time > 0 && game_time < 4_000_000_000 && delivery_time > 0 {
        (delivery_time as i64) - (game_time as i64)
    } else {
        0
    }
}

/// Reads the trailer blocks and keeps only the ones actually hitched.
///
/// The map always carries ten blocks whether or not a trailer is attached. The
/// C# reader returned all ten, so the dashboard's trailer count was always ten
/// and its damage averages were divided by ten regardless of how many trailers
/// were really there. Filtering on the block's own `attached` flag is a
/// deliberate change from that; see the module tests.
fn attached_trailers(map: &[u8]) -> Vec<EtsTrailerDamage> {
    (0..trailers::COUNT)
        .map(|index| trailers::START + index * trailers::STRIDE)
        .filter(|block| bool_at(map, block + trailers::ATTACHED))
        .map(|block| EtsTrailerDamage {
            chassis: f32_at(map, block + trailers::DAMAGE_CHASSIS),
            cargo: f32_at(map, block + trailers::DAMAGE_CARGO),
            wheels: f32_at(map, block + trailers::DAMAGE_WHEELS),
            body: f32_at(map, block + trailers::DAMAGE_BODY),
        })
        .collect()
}

#[cfg(test)]
mod tests {
    use super::*;

    struct Map(Vec<u8>);

    impl Map {
        fn new() -> Self {
            Self(vec![0u8; MAP_SIZE])
        }
        fn f32(mut self, offset: usize, value: f32) -> Self {
            self.0[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
            self
        }
        fn u32(mut self, offset: usize, value: u32) -> Self {
            self.0[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
            self
        }
        fn u64(mut self, offset: usize, value: u64) -> Self {
            self.0[offset..offset + 8].copy_from_slice(&value.to_le_bytes());
            self
        }
        fn flag(mut self, offset: usize, value: bool) -> Self {
            self.0[offset] = value as u8;
            self
        }
        fn text(mut self, offset: usize, value: &str) -> Self {
            let bytes = value.as_bytes();
            self.0[offset..offset + bytes.len()].copy_from_slice(bytes);
            self
        }
        fn trailer(self, index: usize, attached: bool, damage: [f32; 4]) -> Self {
            let block = trailers::START + index * trailers::STRIDE;
            self.flag(block + trailers::ATTACHED, attached)
                .f32(block + trailers::DAMAGE_CARGO, damage[0])
                .f32(block + trailers::DAMAGE_CHASSIS, damage[1])
                .f32(block + trailers::DAMAGE_WHEELS, damage[2])
                .f32(block + trailers::DAMAGE_BODY, damage[3])
        }
    }

    #[test]
    fn a_short_map_is_rejected() {
        assert!(decode(&[0u8; MAP_SIZE - 1]).is_none());
        assert!(decode(&[]).is_none());
    }

    #[test]
    fn the_zone_layout_fits_the_published_map() {
        // The string zone runs right up to where the trailers start, and the
        // ten trailer blocks fit inside the map. Both are a check on the
        // stride arithmetic rather than on any single field.
        let strings_zone = ZONE_STARTS[12];
        assert_eq!(strings_zone + 25 * STRING_SIZE, trailers::START);
        assert!(trailers::START + trailers::COUNT * trailers::STRIDE <= MAP_SIZE);
    }

    #[test]
    fn dashboard_fields_are_read_from_their_documented_offsets() {
        let map = Map::new()
            .f32(zone4::RPM, 1450.0)
            .f32(zone4::SPEED, 25.0)
            .f32(zone4::FUEL_AMOUNT, 420.5)
            .f32(zone4::FUEL_RANGE, 870.0)
            .f32(zone4::ADBLUE, 38.0)
            .f32(zone4::ODOMETER, 123_456.0)
            .f32(zone4::OIL_PRESSURE, 4.2)
            .f32(zone4::OIL_TEMPERATURE, 92.0)
            .f32(zone4::WATER_TEMPERATURE, 88.5)
            .f32(zone4::BATTERY_VOLTAGE, 24.3)
            .flag(zone5::WIPERS, true)
            .flag(zone5::CRUISE_CONTROL, true)
            .f32(zone4::CRUISE_CONTROL_SPEED, 25.0);

        let t = decode(&map.0).expect("decodes");
        assert_eq!(t.truck.dashboard.rpm, 1450.0);
        assert_eq!(t.truck.dashboard.fuel_amount, 420.5);
        assert_eq!(t.truck.dashboard.fuel_range, 870.0);
        assert_eq!(t.truck.dashboard.ad_blue, 38.0);
        assert_eq!(t.truck.dashboard.odometer, 123_456.0);
        assert_eq!(t.truck.dashboard.oil_pressure, 4.2);
        assert_eq!(t.truck.dashboard.oil_temperature, 92.0);
        assert_eq!(t.truck.dashboard.water_temperature, 88.5);
        assert_eq!(t.truck.dashboard.battery_voltage, 24.3);
        assert!(t.truck.dashboard.wipers);
        assert!(t.truck.dashboard.cruise_control);
    }

    #[test]
    fn speeds_are_converted_to_kilometres_per_hour() {
        // The map reports metres per second; the dashboard wants km/h.
        let map = Map::new()
            .f32(zone4::SPEED, 25.0)
            .f32(zone4::SPEED_LIMIT, 25.0)
            .f32(zone4::CRUISE_CONTROL_SPEED, 25.0);
        let t = decode(&map.0).expect("decodes");
        assert_eq!(t.truck.dashboard.speed_kph, 90.0);
        assert_eq!(t.truck.dashboard.speed_mps, 25.0);
        assert_eq!(t.navigation.speed_limit_kph, 90.0);
        assert_eq!(t.truck.dashboard.cruise_control_speed_kph, 90.0);
    }

    #[test]
    fn every_warning_lamp_has_its_own_byte() {
        // They sit adjacent and unaligned, so an off-by-one lights the wrong one.
        let map = Map::new()
            .flag(zone5::WARN_FUEL, true)
            .flag(zone5::WARN_OIL_PRESSURE, true);
        let t = decode(&map.0).expect("decodes");
        assert!(t.truck.warnings.fuel);
        assert!(t.truck.warnings.oil_pressure);
        assert!(!t.truck.warnings.ad_blue);
        assert!(!t.truck.warnings.water_temperature);
        assert!(!t.truck.warnings.battery_voltage);
        assert!(!t.truck.warnings.air_pressure);
        assert!(!t.truck.warnings.air_pressure_emergency);
    }

    #[test]
    fn truck_constants_include_the_gearbox_and_wheels() {
        let mut map = Map::new()
            .u32(zone2::FORWARD_GEAR_COUNT, 12)
            .u32(zone2::RETARDER_STEP_COUNT, 3)
            .f32(zone4::ENGINE_RPM_MAX, 2500.0)
            .f32(zone4::DIFFERENTIAL_RATIO, 3.08)
            .text(zone9::TRUCK_BRAND, "Scania")
            .text(zone9::TRUCK_NAME, "R 2016");
        map = map
            .f32(zone4::GEAR_RATIOS_FORWARD, 11.32)
            .f32(zone4::GEAR_RATIOS_FORWARD + 4, 9.19)
            .f32(zone4::WHEEL_RADII, 0.0)
            .f32(zone4::WHEEL_RADII + 4, 0.512);

        let t = decode(&map.0).expect("decodes");
        let c = &t.truck.constants;
        assert_eq!(c.forward_gear_count, 12);
        assert_eq!(c.retarder_step_count, 3);
        assert_eq!(c.engine_rpm_max, 2500.0);
        assert_eq!(c.differential_ratio, 3.08);
        assert_eq!(c.brand, "Scania");
        assert_eq!(c.name, "R 2016");
        assert_eq!(c.gear_ratios_forward.len(), 24);
        assert_eq!(c.gear_ratios_forward[0], 11.32);
        assert_eq!(c.gear_ratios_forward[1], 9.19);
        assert_eq!(c.wheel_radii.len(), 16);
        assert_eq!(c.wheel_radii[1], 0.512);
    }

    #[test]
    fn job_strings_are_trimmed_of_their_padding() {
        let map = Map::new()
            .text(zone9::CITY_SOURCE, "Rotterdam")
            .text(zone9::COMPANY_SOURCE, "Euroacres")
            .text(zone9::CITY_DESTINATION, "Dortmund")
            .text(zone9::COMPANY_DESTINATION, "Posped")
            .text(zone9::CARGO_NAME, "Steel Pipes");

        let t = decode(&map.0).expect("decodes");
        assert_eq!(t.job.source_city, "Rotterdam");
        assert_eq!(t.job.source_company, "Euroacres");
        assert_eq!(t.job.destination_city, "Dortmund");
        assert_eq!(t.job.destination_company, "Posped");
        assert_eq!(t.job.cargo_name, "Steel Pipes");
    }

    #[test]
    fn the_delivery_clock_counts_down_from_the_deadline() {
        let map = Map::new()
            .u32(zone2::GAME_TIME, 1000)
            .u32(zone2::DELIVERY_TIME, 1600);
        assert_eq!(
            decode(&map.0)
                .expect("decodes")
                .job
                .remaining_delivery_time_minutes,
            600
        );
    }

    #[test]
    fn the_delivery_clock_reads_zero_without_a_job_or_a_running_game() {
        // No deadline set.
        let no_job = Map::new().u32(zone2::GAME_TIME, 1000);
        assert_eq!(
            decode(&no_job.0)
                .expect("decodes")
                .job
                .remaining_delivery_time_minutes,
            0
        );

        // Game clock not running yet.
        let not_started = Map::new().u32(zone2::DELIVERY_TIME, 1600);
        assert_eq!(
            decode(&not_started.0)
                .expect("decodes")
                .job
                .remaining_delivery_time_minutes,
            0
        );
    }

    #[test]
    fn the_income_is_a_sixty_four_bit_value() {
        let map = Map::new().u64(zone10::INCOME, 12_345_678_901);
        assert_eq!(decode(&map.0).expect("decodes").job.income, 12_345_678_901);
    }

    #[test]
    fn only_hitched_trailers_are_reported() {
        // The map always carries ten blocks. The C# reader averaged damage over
        // all ten and reported the count as ten, so one trailer at 50% damage
        // showed as 5% behind a count that never changed.
        let map = Map::new()
            .trailer(0, true, [0.2, 0.1, 0.3, 0.4])
            .trailer(1, true, [0.4, 0.2, 0.5, 0.6])
            .trailer(2, false, [0.9, 0.9, 0.9, 0.9]);

        let trailers = decode(&map.0).expect("decodes").trailers;
        assert_eq!(trailers.len(), 2);
        assert_eq!(trailers[0].chassis, 0.1);
        assert_eq!(trailers[1].chassis, 0.2);
    }

    #[test]
    fn no_trailer_is_reported_when_nothing_is_hitched() {
        assert!(decode(&Map::new().0).expect("decodes").trailers.is_empty());
    }

    #[test]
    fn the_tenth_trailer_block_is_still_addressable() {
        // Catches a stride that drifts across ten blocks.
        let map = Map::new().trailer(trailers::COUNT - 1, true, [0.1, 0.25, 0.3, 0.4]);
        let trailers = decode(&map.0).expect("decodes").trailers;
        assert_eq!(trailers.len(), 1);
        assert_eq!(trailers[0].chassis, 0.25);
    }

    #[test]
    fn an_empty_map_decodes_to_a_parked_truck() {
        let t = decode(&Map::new().0).expect("decodes");
        assert_eq!(t.truck.dashboard.rpm, 0.0);
        assert!(!t.truck.engine_enabled);
        assert_eq!(t.common.scale, 0.0);
    }
}

/// Maps `simetry`'s view of the SCS map onto [`EtsTelemetry`].
///
/// `simetry` hands back the plugin's own `scsTelemetryMap_t`, generated with
/// bindgen from the header this module's offsets were derived from, so the two
/// read the same layout by construction. Type-checked against `simetry` but not
/// unit-tested; the byte decoder above stays the tested path.
pub mod simetry_source {
    use simetry::truck_simulator::SimState;
    use simhub_model::telemetry::ets::*;

    /// Metres per second to kilometres per hour.
    fn kph(metres_per_second: f32) -> f32 {
        metres_per_second * 3.6
    }

    pub fn telemetry_from(state: &SimState) -> EtsTelemetry {
        let map = &state.shared;
        let text = SimState::parse_string;

        let game_time = map.common_ui.time_abs;
        let delivery_time = map.config_ui.time_abs_delivery;

        EtsTelemetry {
            common: EtsCommon {
                scale: map.common_f.scale,
                next_rest_stop_minutes: map.common_i.restStop,
                game_time: game_time as u64,
            },
            job: EtsJob {
                source_city: text(&map.config_s.citySrc),
                source_company: text(&map.config_s.compSrc),
                destination_city: text(&map.config_s.cityDst),
                destination_company: text(&map.config_s.compDst),
                remaining_delivery_time_minutes: super::remaining_delivery_time(
                    delivery_time,
                    game_time,
                ),
                income: map.config_ull.jobIncome,
                cargo_name: text(&map.config_s.cargo),
                cargo_mass: map.config_f.cargoMass,
                cargo_damage: map.job_f.cargoDamage,
            },
            navigation: EtsNavigation {
                distance_metres: map.truck_f.routeDistance,
                time_seconds: map.truck_f.routeTime,
                speed_limit_kph: kph(map.truck_f.speedLimit),
            },
            truck: EtsTruck {
                constants: EtsTruckConstants {
                    brand: text(&map.config_s.truckBrand),
                    name: text(&map.config_s.truckName),
                    forward_gear_count: map.config_ui.gears,
                    engine_rpm_max: map.config_f.engineRpmMax,
                    gear_ratios_forward: map.config_f.gearRatiosForward.to_vec(),
                    differential_ratio: map.config_f.gearDifferential,
                    retarder_step_count: map.config_ui.retarderStepCount,
                    wheel_radii: map.config_f.truckWheelRadius.to_vec(),
                    fuel_capacity: map.config_f.fuelCapacity,
                    ad_blue_capacity: map.config_f.adblueCapacity,
                },
                dashboard: EtsDashboard {
                    rpm: map.truck_f.engineRpm,
                    speed_kph: kph(map.truck_f.speed),
                    speed_mps: map.truck_f.speed,
                    cruise_control: map.truck_b.cruiseControl,
                    cruise_control_speed_kph: kph(map.truck_f.cruiseControlSpeed),
                    fuel_average_consumption: map.truck_f.fuelAvgConsumption,
                    fuel_range: map.truck_f.fuelRange,
                    fuel_amount: map.truck_f.fuel,
                    ad_blue: map.truck_f.adblue,
                    odometer: map.truck_f.truckOdometer,
                    oil_pressure: map.truck_f.oilPressure,
                    oil_temperature: map.truck_f.oilTemperature,
                    water_temperature: map.truck_f.waterTemperature,
                    battery_voltage: map.truck_f.batteryVoltage,
                    wipers: map.truck_b.wipers,
                },
                warnings: EtsWarnings {
                    fuel: map.truck_b.fuelWarning,
                    ad_blue: map.truck_b.adblueWarning,
                    oil_pressure: map.truck_b.oilPressureWarning,
                    water_temperature: map.truck_b.waterTemperatureWarning,
                    battery_voltage: map.truck_b.batteryVoltageWarning,
                    air_pressure: map.truck_b.airPressureWarning,
                    air_pressure_emergency: map.truck_b.airPressureEmergency,
                },
                lights: EtsLights {
                    parking: map.truck_b.lightsParking,
                    beam_low: map.truck_b.lightsBeamLow,
                    beam_high: map.truck_b.lightsBeamHigh,
                    hazard_warning: map.truck_b.lightsHazard,
                    blinker_left_on: map.truck_b.blinkerLeftOn,
                    blinker_right_on: map.truck_b.blinkerRightOn,
                    beacon: map.truck_b.lightsBeacon,
                    dashboard_backlight: map.truck_f.lightsDashboard,
                },
                brakes: EtsBrakes {
                    parking_brake: map.truck_b.parkBrake,
                    motor_brake: map.truck_b.motorBrake,
                    temperature: map.truck_f.brakeTemperature,
                    air_pressure: map.truck_f.airPressure,
                    retarder_level: map.truck_ui.retarderBrake,
                },
                damage: EtsTruckDamage {
                    cabin: map.truck_f.wearCabin,
                    wheels_avg: map.truck_f.wearWheels,
                    transmission: map.truck_f.wearTransmission,
                    engine: map.truck_f.wearEngine,
                    chassis: map.truck_f.wearChassis,
                },
                selected_gear: map.truck_i.gear,
                engine_enabled: map.truck_b.engineEnabled,
                differential_lock: map.truck_b.differentialLock,
                lift_axle_indicator: map.truck_b.liftAxleIndicator,
            },
            throttle: map.truck_f.gameThrottle,
            // Same filter as the byte decoder: the map always carries ten
            // blocks, and only the hitched ones belong in the average.
            trailers: map
                .trailer
                .trailer
                .iter()
                .filter(|trailer| trailer.com_b.attached)
                .map(|trailer| EtsTrailerDamage {
                    chassis: trailer.com_f.wearChassis,
                    cargo: trailer.com_f.cargoDamage,
                    wheels: trailer.com_f.wearWheels,
                    body: trailer.com_f.wearBody,
                })
                .collect(),
        }
    }
}

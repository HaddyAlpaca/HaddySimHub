//! Ported from the `SCSTelemetry` tree the C# converter consumed.
//!
//! The SCS map nests values several levels deep (`TruckValues.CurrentValues.
//! DashboardValues.WarningValues...`). The grouping here follows that shape so
//! the reader maps across mechanically, but it carries only the fields the
//! dashboard uses rather than the whole 32 KiB map.

#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsCommon {
    /// World scale: in-game time runs this many times faster than real time.
    pub scale: f32,
    /// Minutes until the driver must rest.
    pub next_rest_stop_minutes: i32,
    pub game_time: u64,
}

#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsJob {
    pub source_city: String,
    pub source_company: String,
    pub destination_city: String,
    pub destination_company: String,
    pub remaining_delivery_time_minutes: i64,
    pub income: u64,
    pub cargo_name: String,
    pub cargo_mass: f32,
    /// Fraction from 0 to 1.
    pub cargo_damage: f32,
}

#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsNavigation {
    pub distance_metres: f32,
    pub time_seconds: f32,
    pub speed_limit_kph: f32,
}

#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsWarnings {
    pub fuel: bool,
    pub ad_blue: bool,
    pub oil_pressure: bool,
    pub water_temperature: bool,
    pub battery_voltage: bool,
    pub air_pressure: bool,
    pub air_pressure_emergency: bool,
}

#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsLights {
    pub parking: bool,
    pub beam_low: bool,
    pub beam_high: bool,
    pub hazard_warning: bool,
    pub blinker_left_on: bool,
    pub blinker_right_on: bool,
    pub beacon: bool,
    pub dashboard_backlight: f32,
}

#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsBrakes {
    pub parking_brake: bool,
    pub motor_brake: bool,
    pub temperature: f32,
    pub air_pressure: f32,
    pub retarder_level: u32,
}

/// Damage fractions from 0 to 1.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsTruckDamage {
    pub cabin: f32,
    pub wheels_avg: f32,
    pub transmission: f32,
    pub engine: f32,
    pub chassis: f32,
}

/// Damage fractions from 0 to 1, for one attached trailer.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsTrailerDamage {
    pub chassis: f32,
    pub cargo: f32,
    pub wheels: f32,
    pub body: f32,
}

#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsDashboard {
    pub rpm: f32,
    pub speed_kph: f32,
    /// Metres per second, signed: negative when reversing.
    pub speed_mps: f32,
    pub cruise_control: bool,
    pub cruise_control_speed_kph: f32,
    /// Litres per unit, before the converter scales it to per 100 km.
    pub fuel_average_consumption: f32,
    pub fuel_range: f32,
    pub fuel_amount: f32,
    pub ad_blue: f32,
    pub odometer: f32,
    pub oil_pressure: f32,
    pub oil_temperature: f32,
    pub water_temperature: f32,
    pub battery_voltage: f32,
    pub wipers: bool,
}

/// Values that do not change while a truck is driven.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsTruckConstants {
    pub brand: String,
    pub name: String,
    pub forward_gear_count: u32,
    pub engine_rpm_max: f32,
    pub gear_ratios_forward: Vec<f32>,
    pub differential_ratio: f32,
    pub retarder_step_count: u32,
    pub wheel_radii: Vec<f32>,
    pub fuel_capacity: f32,
    pub ad_blue_capacity: f32,
}

#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsTruck {
    pub constants: EtsTruckConstants,
    pub dashboard: EtsDashboard,
    pub warnings: EtsWarnings,
    pub lights: EtsLights,
    pub brakes: EtsBrakes,
    pub damage: EtsTruckDamage,
    /// Negative is a reverse gear, 0 is neutral, positive is a forward gear.
    pub selected_gear: i32,
    pub engine_enabled: bool,
    pub differential_lock: bool,
    pub lift_axle_indicator: bool,
}

#[derive(Clone, Debug, Default, PartialEq)]
pub struct EtsTelemetry {
    pub common: EtsCommon,
    pub job: EtsJob,
    pub navigation: EtsNavigation,
    pub truck: EtsTruck,
    /// Fraction from 0 to 1.
    pub throttle: f32,
    pub trailers: Vec<EtsTrailerDamage>,
}

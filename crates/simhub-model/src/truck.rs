//! Ported from `HaddySimHub/Models/TruckData.cs`.

/// A truck dashboard payload.
///
/// The "irl" variants are the in-game values divided by the world scale, so the
/// dashboard can show both how long a leg takes in game and how long the player
/// will actually be sitting there.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct TruckData {
    // Navigation.
    pub source_city: String,
    pub source_company: String,
    pub destination_city: String,
    pub destination_company: String,
    /// Kilometres to the destination.
    pub distance_remaining: f32,
    /// Minutes to the destination, in game time.
    pub time_remaining: i32,
    /// Minutes to the destination, in real time.
    pub time_remaining_irl: i32,
    /// Minutes until the driver must rest, in game time.
    pub rest_time_remaining: i32,
    /// Minutes until the driver must rest, in real time.
    pub rest_time_remaining_irl: i32,
    pub game_time: u64,

    // Job.
    pub job_time_remaining: i64,
    pub job_time_remaining_irl: i64,
    pub job_income: u64,
    pub job_cargo_name: String,
    pub job_cargo_mass: i32,
    pub job_cargo_damage: i32,

    // Damage, as whole percentages.
    pub damage_truck_cabin: i32,
    pub damage_truck_transmission: i32,
    pub damage_truck_wheels: i32,
    pub damage_truck_engine: i32,
    pub damage_truck_chassis: i32,
    pub damage_trailer_chassis: i32,
    pub damage_trailer_cargo: i32,
    pub damage_trailer_wheels: i32,
    pub damage_trailer_body: i32,
    pub number_of_trailers_attached: i32,

    // Dashboard.
    pub truck_name: String,
    pub gear: String,
    pub recommended_gear: String,
    pub rpm: i32,
    pub rpm_max: i32,
    pub speed: i16,
    pub speed_limit: i16,
    pub cruise_control_on: bool,
    pub cruise_control_speed: i16,
    pub throttle: i32,
    pub engine_on: bool,
    pub odometer: f32,

    // Fuel and AdBlue.
    pub fuel_distance: f32,
    pub fuel_amount: f32,
    pub fuel_capacity: f32,
    pub fuel_average_consumption: f32,
    pub fuel_warning_on: bool,
    pub ad_blue_amount: f32,
    pub ad_blue_capacity: f32,
    pub ad_blue_warning_on: bool,

    // Lights.
    pub parking_lights_on: bool,
    pub low_beam_on: bool,
    pub high_beam_on: bool,
    pub hazard_lights_on: bool,
    pub blinker_left_on: bool,
    pub blinker_right_on: bool,
    pub beacon_on: bool,
    pub dashboard_backlight: f32,

    // Brakes and drivetrain.
    pub parking_brake_on: bool,
    pub motor_brake_on: bool,
    pub brake_temp: f32,
    pub brake_air_pressure: f32,
    pub air_pressure_warning_on: bool,
    pub air_pressure_emergency_on: bool,
    pub retarder_level: u32,
    pub retarder_step_count: u32,
    pub differential_lock: bool,
    pub lift_axle_indicator_on: bool,

    // Engine instrumentation.
    pub oil_pressure: f32,
    pub oil_pressure_warning_on: bool,
    pub oil_temp: f32,
    pub water_temp: f32,
    pub water_temp_warning_on: bool,
    pub battery_voltage: f32,
    pub battery_voltage_warning_on: bool,

    pub wipers_on: bool,
}

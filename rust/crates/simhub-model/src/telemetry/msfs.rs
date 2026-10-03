//! Ported from `HaddySimHub/Displays/Msfs/MsfsTelemetry.cs`.

/// One SimConnect telemetry block.
///
/// SimConnect returns every numeric simvar as a `double`, including the flags,
/// which arrive as 0 or 1. The field order is part of the contract with the
/// data definition registered at connect time.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct MsfsTelemetry {
    // Speed.
    pub indicated_airspeed: f64,
    pub true_airspeed: f64,
    /// Knots, which is what makes the distance-to-destination arithmetic work.
    pub ground_speed: f64,
    pub mach: f64,
    pub stall_warning: f64,
    pub overspeed_warning: f64,

    // Attitude. Both angles arrive in radians despite "DEGREES" in the simvar
    // names, and are signed the opposite way round from how an instrument reads.
    pub pitch_radians: f64,
    pub bank_radians: f64,
    /// -127 to 127.
    pub turn_coordinator_ball: f64,
    pub turn_rate_radians_per_second: f64,

    // Altitude.
    pub indicated_altitude: f64,
    pub altitude_above_ground: f64,
    pub vertical_speed: f64,
    pub altimeter_setting_mb: f64,
    pub on_ground: f64,

    // Heading.
    pub heading_magnetic: f64,
    pub heading_true: f64,
    pub ground_track: f64,
    pub wind_direction: f64,
    pub wind_velocity: f64,

    // Autopilot.
    pub autopilot_master: f64,
    pub autopilot_heading_lock: f64,
    pub autopilot_heading_bug: f64,
    pub autopilot_altitude_lock: f64,
    pub autopilot_altitude_target: f64,
    pub autopilot_airspeed_hold: f64,
    pub autopilot_airspeed_target: f64,
    pub autopilot_vertical_speed_target: f64,
    pub autopilot_nav_lock: f64,
    pub autopilot_approach_hold: f64,

    // Flight plan.
    pub flight_plan_active: f64,
    pub waypoint_distance_meters: f64,
    pub waypoint_ete_seconds: f64,
    pub destination_ete_seconds: f64,
    pub destination_eta_seconds: f64,
    pub cross_track_meters: f64,
    pub approach_active: f64,

    // Navigation radio.
    pub nav_has_signal: f64,
    pub nav_has_localizer: f64,
    pub nav_has_glide_slope: f64,
    /// -127 to 127.
    pub nav_cdi: f64,
    /// -119 to 119.
    pub nav_gsi: f64,
    pub nav_to_from: f64,
    pub nav_obs: f64,

    // Engine.
    pub engine_count: f64,
    pub engine_type: f64,
    pub turbine_n1_pct: f64,
    pub piston_pct_max_rpm: f64,
    pub engine_rpm: f64,
    pub fuel_flow_pph: f64,
    pub oil_temperature: f64,
    pub oil_pressure: f64,
    pub manifold_pressure: f64,

    // Fuel.
    pub fuel_quantity_lbs: f64,
    pub fuel_capacity_gallons: f64,
    pub fuel_weight_per_gallon: f64,

    // Configuration.
    pub flaps_handle_index: f64,
    pub flaps_handle_positions: f64,
    pub gear_percent_extended: f64,
    pub gear_handle_down: f64,
    pub spoilers_handle_pct: f64,
    pub spoilers_armed: f64,
    pub parking_brake_on: f64,
    pub elevator_trim_pct: f64,

    // Lights.
    pub light_landing: f64,
    pub light_taxi: f64,
    pub light_strobe: f64,
    pub light_nav: f64,
    pub light_beacon: f64,

    pub zulu_time_seconds: f64,

    // Strings.
    pub next_waypoint_id: String,
    pub destination_id: String,
    pub nav_ident: String,
    pub aircraft_title: String,
}

//! Ported from `HaddySimHub/Models/FlightData.cs` and the enums beside it.

/// Mirrors the MSFS `ENGINE TYPE` simvar, values included. The dashboard uses it
/// to label the primary engine readout: turbines report N1, pistons a percentage
/// of maximum RPM.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
#[repr(i32)]
pub enum EngineType {
    #[default]
    Piston = 0,
    Jet = 1,
    None = 2,
    HeloBellTurbine = 3,
    Unsupported = 4,
    Turboprop = 5,
}

/// Mirrors the MSFS `NAV TOFROM` simvar, values included: whether the selected
/// radial leads to the station or away from it. Only meaningful for a VOR.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
#[repr(i32)]
pub enum NavToFrom {
    #[default]
    Off = 0,
    To = 1,
    From = 2,
}

/// What is driving the course deviation indicator.
///
/// The navigation radio wins over the flight plan when it is receiving a
/// station, which is how a pilot flying an approach expects the needle to behave.
#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub enum CourseDeviationSource {
    /// No guidance at all: no station received and no flight plan.
    #[default]
    None,
    /// Deviation from the active flight plan leg.
    Gps,
    /// Deviation from the selected radial of a VOR.
    Vor,
    /// Deviation from a localizer, which may also carry a glideslope.
    Localizer,
}

/// A flight dashboard payload.
///
/// Values the aircraft or situation does not provide stay `None`: a jet has no
/// crankshaft speed, and without a flight plan there is no waypoint to be a
/// distance from.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct FlightData {
    // Speed.
    pub indicated_airspeed: f32,
    pub true_airspeed: f32,
    pub ground_speed: f32,
    pub mach_number: f32,
    pub stall_warning: bool,
    pub overspeed_warning: bool,

    // Attitude.
    pub pitch_degrees: f32,
    pub bank_degrees: f32,
    /// -1 to 1, left to right.
    pub slip_ball: f32,
    /// Degrees per second.
    pub turn_rate: f32,

    // Altitude.
    pub indicated_altitude: f32,
    pub altitude_above_ground: f32,
    pub vertical_speed: f32,
    pub altimeter_setting_hpa: f32,
    pub on_ground: bool,

    // Heading.
    pub heading_magnetic: f32,
    pub heading_true: f32,
    pub ground_track: f32,
    pub wind_direction: f32,
    pub wind_speed: f32,

    // Autopilot.
    pub autopilot_master: bool,
    pub heading_hold: bool,
    pub heading_bug: f32,
    pub altitude_hold: bool,
    pub altitude_target: f32,
    pub speed_hold: bool,
    pub speed_target: f32,
    pub vertical_speed_target: f32,
    pub nav_hold: bool,
    pub approach_hold: bool,

    // Flight plan.
    pub has_active_flight_plan: bool,
    pub next_waypoint_id: Option<String>,
    pub distance_to_waypoint_nm: Option<f32>,
    pub waypoint_ete_seconds: Option<i32>,
    pub destination_id: Option<String>,
    pub distance_to_destination_nm: Option<f32>,
    pub destination_ete_seconds: Option<i32>,
    pub destination_eta_utc_seconds: Option<i32>,
    pub cross_track_error_nm: Option<f32>,

    // Course deviation.
    pub deviation_source: CourseDeviationSource,
    pub deviation_source_id: Option<String>,
    pub selected_course: Option<f32>,
    /// -1 to 1 of full-scale deflection.
    pub lateral_deviation: Option<f32>,
    /// What full-scale deflection means in nautical miles, where that is fixed.
    pub lateral_full_scale_nm: Option<f32>,
    /// -1 to 1 of full-scale deflection.
    pub glideslope_deviation: Option<f32>,
    pub to_from: NavToFrom,

    // Engine.
    pub engine_count: i32,
    pub engine_type: EngineType,
    /// N1 for a turbine, percentage of maximum RPM for a piston.
    pub engine_primary_pct: Option<f32>,
    pub engine_rpm: Option<i32>,
    pub fuel_flow_pph: Option<f32>,
    pub oil_temperature: Option<f32>,
    pub oil_pressure: Option<f32>,
    pub manifold_pressure: Option<f32>,

    // Fuel.
    pub fuel_quantity_lbs: f32,
    pub fuel_capacity_lbs: f32,
    pub fuel_endurance_seconds: Option<i32>,

    // Configuration.
    pub flaps_handle_index: i32,
    pub flaps_handle_positions: i32,
    pub gear_percent_extended: f32,
    pub gear_handle_down: bool,
    pub spoilers_pct: f32,
    pub spoilers_armed: bool,
    pub parking_brake_on: bool,
    pub elevator_trim_pct: f32,

    // Lights.
    pub landing_lights_on: bool,
    pub taxi_lights_on: bool,
    pub strobe_lights_on: bool,
    pub nav_lights_on: bool,
    pub beacon_on: bool,

    pub aircraft_title: String,
    /// Seconds into the UTC day.
    pub sim_time_utc_seconds: i32,
}

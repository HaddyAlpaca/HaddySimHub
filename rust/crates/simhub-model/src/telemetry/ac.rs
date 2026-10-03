//! Ported from `HaddySimHub/Displays/AC/ACTelemetry.cs`.

/// Assetto Corsa telemetry, read from the `acpmf_physics`, `acpmf_graphics` and
/// `acpmf_static` shared memory pages.
///
/// The page names are shared with ACC and AC Rally but the layouts are not, so
/// this is a separate type even where the field names line up.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct AcTelemetry {
    // Physics page.
    pub gas: f32,
    pub brake: f32,
    pub clutch: f32,
    pub steer_angle: f32,
    /// 0 = reverse, 1 = neutral, 2 = first.
    pub gear: i32,
    pub rpms: i32,
    pub speed_kmh: f32,
    pub fuel: f32,
    pub air_temp: f32,
    pub road_temp: f32,
    pub brake_bias: f32,
    pub pit_limiter_on: i32,

    // Graphics page.
    /// -1 = unknown, 0 = practice, through to 6 = drag.
    pub session_type: i32,
    /// Laps *completed*, not the lap being driven.
    pub completed_laps: i32,
    pub number_of_laps: i32,
    /// Zero or negative when the game does not expose a position.
    pub position: i32,
    /// Milliseconds.
    pub current_time: i32,
    /// Milliseconds.
    pub last_time: i32,
    /// Milliseconds, or `i32::MAX` when no lap has been set.
    pub best_time: i32,
    /// Milliseconds, as a float.
    pub session_time_left: f32,

    // Static page, written once per session. Used for the session log line
    // rather than the dashboard.
    pub max_rpm: i32,
    pub max_fuel: f32,
    pub car_model: String,
    pub track: String,
    pub sm_version: String,
    pub ac_version: String,
}

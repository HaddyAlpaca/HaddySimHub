//! Ported from `HaddySimHub/Displays/ACC/ACCTelemetry.cs`.

/// Assetto Corsa Competizione telemetry, as read from the `acpmf_physics` and
/// `acpmf_graphics` shared memory pages. Only the fields the dashboard consumes
/// are modelled; the pages carry considerably more.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct AccTelemetry {
    // Physics page.
    pub gas: f32,
    pub brake: f32,
    pub clutch: f32,
    pub fuel: f32,
    /// Raw gear index: 0 = reverse, 1 = neutral, 2 = first.
    pub gear: i32,
    pub rpms: i32,
    pub max_rpm: f32,
    pub steer_angle: f32,
    pub speed_kmh: f32,
    pub air_temp: f32,
    pub road_temp: f32,
    pub brake_bias: f32,
    pub pit_limiter_on: i32,

    // Graphics page.
    pub session_type: i32,
    pub current_time_ms: i32,
    pub last_time_ms: i32,
    pub best_time_ms: i32,
    pub delta_ms: i32,
    /// Laps *completed*, not the lap being driven.
    pub current_lap: i32,
    pub number_of_laps: i32,
    pub position: i32,
    pub session_time_left_ms: i32,
    pub fuel_per_lap: f32,
    pub fuel_estimated_laps: f32,
    pub rain_intensity: i32,
    pub wind_speed: f32,
    pub wind_direction: f32,
    pub track_grip_status: i32,
}

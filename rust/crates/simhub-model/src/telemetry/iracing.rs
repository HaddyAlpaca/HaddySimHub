//! Ported from the `iRacingSDK` types the C# converter consumed.
//!
//! The SDK handed the converter a whole `IDataSample` and let it walk the
//! session tree — picking the session by `SessionNum`, the driver by `CarIdx`.
//! That traversal belongs with the reader, so this type is already flattened:
//! the reader resolves the lookups and the converter sees only what it uses.

/// The competing driver the player is, resolved from `CarIdx`.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct IRacingDriver {
    pub car_number: String,
    pub i_rating: i32,
    pub lic_level: i32,
}

/// Session-level information, resolved for the session the sample belongs to.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct IRacingSession {
    pub session_type: String,
    pub is_limited_time: bool,
    pub is_limited_session_laps: bool,
    pub session_laps: i32,
    /// `WeekendInfo.WeekendOptions.IncidentLimit`, before clamping.
    pub incident_limit: i32,
    /// `None` when the player is not among the competing drivers.
    pub player: Option<IRacingDriver>,
}

/// One telemetry frame from the shared-memory ring buffer.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct IRacingTelemetry {
    pub session_num: i32,
    pub player_car_idx: i32,
    /// Lap count per car, indexed by `CarIdx`.
    pub car_idx_lap: Vec<i32>,
    pub lap: i32,
    pub fuel_level: f32,
    pub player_car_in_pit_stall: bool,
    pub on_pit_road: bool,
    pub session_time_remain: f64,
    pub player_car_position: i32,
    pub lap_current_lap_time: f32,
    /// Metres per second; the dashboard shows km/h.
    pub speed: f32,
    /// -1 = reverse, 0 = neutral, 1 = first.
    pub gear: i32,
    pub rpm: f32,
    pub track_temp: f32,
    pub air_temp: f32,
    pub lap_last_lap_time: f32,
    pub lap_delta_to_session_last_lap: f32,
    pub lap_best_lap_time: f32,
    pub lap_delta_to_best_lap: f32,
    pub throttle: f32,
    pub brake: f32,
    /// The `PitSpeedLimiter` bit of `EngineWarnings`.
    pub pit_speed_limiter: bool,
    pub steering_wheel_angle: f32,
    /// The full span, e.g. 12 when the wheel travels -6..+6.
    pub steering_wheel_angle_max: f32,
    pub dc_brake_bias: f32,
    pub car_screen_name: String,
    pub player_car_driver_incident_count: i32,
    /// iRating of every car in the race, used for strength of field.
    pub race_car_iratings: Vec<i32>,
}

/// One sample: a telemetry frame plus the session it belongs to.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct IRacingSample {
    pub telemetry: IRacingTelemetry,
    pub session: IRacingSession,
}

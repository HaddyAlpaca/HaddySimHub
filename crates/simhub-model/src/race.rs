//! Ported from `HaddySimHub/Models/RaceData.cs`.

/// A race dashboard payload. Fields a simulator does not expose stay `None`
/// rather than defaulting to zero, so the UI can tell "not available" from
/// "actually zero" — the C# record used nullables for the same reason.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct RaceData {
    // Universal mandatory fields.
    pub session_type: String,
    pub is_limited_time: bool,
    pub is_limited_session_laps: bool,
    pub current_lap: i32,
    pub total_laps: i32,
    pub session_time_remaining: f32,
    /// Current race position. `None` when the sim does not expose it (e.g. Assetto Corsa).
    pub position: Option<i32>,
    pub speed: i32,
    pub gear: String,
    pub rpm: i32,
    pub rpm_max: i32,
    pub track_temp: f32,
    pub air_temp: f32,
    /// Fuel remaining in litres. `None` when the sim does not expose it.
    pub fuel_remaining: Option<f32>,
    /// Average fuel used per lap. `None` when the sim does not expose it.
    pub fuel_avg_lap: Option<f32>,
    /// Fuel used on the last lap. `None` when the sim does not expose it.
    pub fuel_last_lap: Option<f32>,
    pub fuel_est_laps: f32,
    pub current_lap_time: f32,
    pub last_lap_time: f32,
    /// Delta of the last lap to the reference lap. `None` when not exposed.
    pub last_lap_time_delta: Option<f32>,
    /// Best lap time. `None` when the sim does not expose it.
    pub best_lap_time: Option<f32>,
    /// Delta to the best lap. `None` when the sim does not expose it.
    pub best_lap_time_delta: Option<f32>,
    pub clutch_pct: i32,
    pub throttle_pct: i32,
    pub brake_pct: i32,
    pub pit_limiter_on: bool,
    pub steering_pct: i32,

    // Forza-specific fields.
    /// Numeric car identifier from the telemetry packet.
    pub car_ordinal: Option<i32>,
    /// Performance class from D (0) through X (7).
    pub car_class: Option<i32>,
    /// Performance index from 100 through 999.
    pub car_performance_index: Option<i32>,
    /// Drivetrain (0=FWD, 1=RWD, 2=AWD).
    pub drivetrain_type: Option<i32>,
    pub num_cylinders: Option<i32>,
    pub power: Option<f32>,
    pub torque: Option<f32>,

    // iRacing-specific fields.
    /// Expected finish position, derived from the assigned car number.
    pub expected_position: Option<String>,
    pub strength_of_field: Option<i32>,
    pub incidents: Option<i64>,
    pub max_incidents: Option<i64>,
    pub i_rating: Option<i32>,
    pub safety_rating: Option<i32>,

    /// iRacing and ACC: brake bias percentage.
    pub brake_bias: Option<f32>,

    // ACC weather and track condition fields.
    /// Rain intensity (0=none, 1=light, 2=medium, 3=heavy).
    pub rain_intensity: Option<i32>,
    /// Wind speed in m/s.
    pub wind_speed: Option<f32>,
    /// Wind direction in radians.
    pub wind_direction: Option<f32>,
    /// Track grip status (0=green, 1=fast, 2=optimum, 3=wet).
    pub track_grip_status: Option<i32>,
}

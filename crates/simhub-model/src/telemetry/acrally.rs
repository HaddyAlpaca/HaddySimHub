//! Ported from `HaddySimHub/Displays/ACRally/ACRallyTelemetry.cs`.

/// Assetto Corsa Rally telemetry, read from the `acpmf_physics`,
/// `acpmf_graphics` and `acpmf_static` shared memory pages.
///
/// The page layouts were derived from ACC's and have never been validated
/// against the running game, so treat the field meanings as unconfirmed.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct AcRallyTelemetry {
    // Physics page.
    /// 0 = reverse, 1 = neutral, 2 = first.
    pub gear: i32,
    pub rpms: i32,
    /// Rev limit for the current car, where the static page has none.
    pub current_max_rpm: f32,
    pub speed_kmh: f32,

    // Graphics page.
    /// Milliseconds elapsed on the stage.
    pub current_time: i32,
    /// Zero when the game does not expose a position.
    pub position: i32,
    /// Position along the stage spline, 0 to 1.
    pub normalized_car_position: f32,
    /// Metres covered, which stays zero for the first frames of a stage.
    pub distance_traveled: f32,
    pub current_sector_index: i32,

    // Static page.
    pub max_rpm: i32,
    /// Stage length in metres.
    pub track_spline_length: f32,
}

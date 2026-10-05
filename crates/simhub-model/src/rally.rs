//! Ported from `HaddySimHub/Models/RallyData.cs`.

/// A rally dashboard payload. Stages are driven alone against the clock, so
/// there is no field here for gaps to other cars.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct RallyData {
    /// Kilometres per hour.
    pub speed: i32,
    pub gear: String,
    pub rpm: i32,
    pub rpm_max: i32,
    /// Metres covered on this stage.
    pub distance_travelled: i32,
    /// Stage completion from 0 to 100.
    pub completed_pct: i32,
    /// Seconds.
    pub sector1_time: f32,
    /// Seconds.
    pub sector2_time: f32,
    /// Stage time in seconds.
    pub lap_time: f32,
    pub position: i32,
}

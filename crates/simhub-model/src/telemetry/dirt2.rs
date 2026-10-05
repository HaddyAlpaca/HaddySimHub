//! Ported from `HaddySimHub/Displays/Dirt2/Packet.cs`.

/// One Codemasters EGO telemetry datagram, as sent on UDP port 20777.
///
/// Every field arrives as a float, including the gear and position, which is
/// why the converter rounds rather than truncates.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct Dirt2Telemetry {
    /// Stage time in seconds.
    pub lap_time: f32,
    /// Metres covered on this stage.
    pub distance: f32,
    /// Stage completion from 0 to 1.
    pub progress: f32,
    /// Metres per second.
    pub speed_ms: f32,
    /// 0 is neutral, negative is reverse.
    pub gear: f32,
    /// Needs scaling by ten for a realistic figure.
    pub rpm: f32,
    /// Needs scaling by ten for a realistic figure.
    pub max_rpm: f32,
    pub car_pos: f32,
    pub sector_1_time: f32,
    pub sector_2_time: f32,
}

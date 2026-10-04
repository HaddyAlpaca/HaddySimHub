//! Ported from `HaddySimHub/Displays/Forza/ForzaTelemetry.cs`.

/// Forza Horizon 5 "Data Out" telemetry, as sent on UDP port 5300.
///
/// The packet carries far more than this — suspension travel, tyre slip, wheel
/// rotation per corner — but only the fields the dashboard uses are modelled.
/// The reader decodes the full datagram and projects onto this.
#[derive(Clone, Debug, Default, PartialEq)]
pub struct ForzaTelemetry {
    /// Zero while the game is paused or in a menu.
    pub is_race_on: i32,
    pub engine_max_rpm: f32,
    pub current_engine_rpm: f32,
    /// Metres per second.
    pub speed: f32,
    pub power: f32,
    pub torque: f32,
    pub fuel: f32,
    /// Seconds; zero when no lap has been set.
    pub best_lap: f32,
    /// Seconds.
    pub last_lap: f32,
    /// Seconds elapsed on the current lap, despite the name in the packet.
    pub current_lap: f32,
    pub lap_number: u16,
    /// Zero outside a race.
    pub race_position: u8,
    /// 0 to 255.
    pub throttle: u8,
    /// 0 to 255.
    pub brake: u8,
    /// 0 to 255.
    pub clutch: u8,
    /// 0 = reverse, 1 = neutral, 2 = first.
    pub gear: u8,
    /// -127 to 127, signed by direction.
    pub steer: i8,

    // Car identification.
    pub car_ordinal: i32,
    /// D (0) through X (7).
    pub car_class: i32,
    /// 100 through 999.
    pub car_performance_index: i32,
    /// 0 = FWD, 1 = RWD, 2 = AWD.
    pub drivetrain_type: i32,
    pub num_cylinders: i32,
}

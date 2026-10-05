//! The display contract the converters produce, ported from `HaddySimHub/Models`.
//!
//! ADR-0001 froze `DisplayUpdate` as the seam between telemetry and the UI, so
//! the field set here mirrors the C# records. The one deliberate change is the
//! shape: C# carried `object? Data` beside a `DisplayType` discriminant, which
//! Rust expresses as an enum without losing the numeric mapping the SSE client
//! relied on.

pub mod flight;
pub mod race;
pub mod rally;
pub mod telemetry;
pub mod truck;

pub use flight::{CourseDeviationSource, EngineType, FlightData, NavToFrom};
pub use race::RaceData;
pub use rally::RallyData;
pub use truck::TruckData;

/// Mirrors `HaddySimHub.Models.DisplayType`. The discriminants are part of the
/// contract: the SSE client mirrored them positionally, so append, never insert.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
#[repr(i32)]
pub enum DisplayType {
    None = 0,
    TruckDashboard = 1,
    RaceDashboard = 2,
    RallyDashboard = 3,
    FlightDashboard = 4,
}

/// One dashboard update. Replaces the C# `DisplayUpdate` record, which paired a
/// `DisplayType` with a loosely typed payload.
#[derive(Clone, Debug, PartialEq)]
pub enum DisplayUpdate {
    None,
    Truck(TruckData),
    Race(RaceData),
    Rally(RallyData),
    Flight(FlightData),
}

impl DisplayUpdate {
    pub fn display_type(&self) -> DisplayType {
        match self {
            Self::None => DisplayType::None,
            Self::Truck(_) => DisplayType::TruckDashboard,
            Self::Race(_) => DisplayType::RaceDashboard,
            Self::Rally(_) => DisplayType::RallyDashboard,
            Self::Flight(_) => DisplayType::FlightDashboard,
        }
    }
}

//! Decoded source telemetry, one module per simulator.
//!
//! These types are the boundary between the platform-specific readers (shared
//! memory, UDP, SimConnect — Windows only) and the converters, which are pure
//! and build everywhere. Keeping them here is what lets the converters and
//! their tests run on a development machine without a simulator.

pub mod ac;
pub mod acc;
pub mod acrally;
pub mod dirt2;
pub mod ets;
pub mod forza;
pub mod iracing;
pub mod msfs;

pub use ac::AcTelemetry;
pub use acc::AccTelemetry;
pub use acrally::AcRallyTelemetry;
pub use dirt2::Dirt2Telemetry;
pub use ets::EtsTelemetry;
pub use forza::ForzaTelemetry;
pub use iracing::{IRacingDriver, IRacingSample, IRacingSession, IRacingTelemetry};
pub use msfs::MsfsTelemetry;

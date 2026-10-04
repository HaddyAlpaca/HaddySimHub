//! Telemetry-to-dashboard converters, ported from `HaddySimHub/Displays/*/*DataConverter.cs`.
//!
//! Every converter is a pure function from decoded source telemetry to a
//! [`DisplayUpdate`], so the whole module builds and tests on any platform. The
//! simulator-specific reading happens upstream, in the Windows-only readers.

pub mod ac;
pub mod acc;
pub mod acrally;
pub mod dirt2;
pub mod ets;
pub mod forza;
pub mod iracing;
pub mod msfs;

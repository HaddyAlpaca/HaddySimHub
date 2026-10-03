//! Decodes the Codemasters EGO telemetry datagram Dirt Rally 2.0 sends.
//!
//! The game must be configured with `extradata="3"` in
//! `hardware_settings_config.xml`, which is what produces this packet length.
//! Every field is a little-endian `f32`, including the gear and position.

use crate::read::f32_at;
use crate::udp::UdpSource;
use simhub_model::telemetry::Dirt2Telemetry;
use std::io;
use std::net::SocketAddr;
use std::time::Duration;

/// The UDP port the game sends to.
pub const PORT: u16 = 20777;

/// Length of the `extradata="3"` packet.
pub const PACKET_SIZE: usize = 264;

// Offsets within the packet, matching the generated layout manifest.
const LAP_TIME: usize = 4;
const DISTANCE: usize = 8;
const PROGRESS: usize = 12;
const SPEED_MS: usize = 28;
const GEAR: usize = 132;
const RPM: usize = 148;
const CAR_POS: usize = 156;
const SECTOR_1_TIME: usize = 196;
const SECTOR_2_TIME: usize = 200;
const MAX_RPM: usize = 252;

/// Decodes one datagram, or `None` when it is too short to be one.
pub fn decode(bytes: &[u8]) -> Option<Dirt2Telemetry> {
    if bytes.len() < PACKET_SIZE {
        return None;
    }

    Some(Dirt2Telemetry {
        lap_time: f32_at(bytes, LAP_TIME),
        distance: f32_at(bytes, DISTANCE),
        progress: f32_at(bytes, PROGRESS),
        speed_ms: f32_at(bytes, SPEED_MS),
        gear: f32_at(bytes, GEAR),
        rpm: f32_at(bytes, RPM),
        max_rpm: f32_at(bytes, MAX_RPM),
        car_pos: f32_at(bytes, CAR_POS),
        sector_1_time: f32_at(bytes, SECTOR_1_TIME),
        sector_2_time: f32_at(bytes, SECTOR_2_TIME),
    })
}

/// Receives and decodes Dirt Rally 2.0 telemetry.
#[derive(Debug)]
pub struct Dirt2Source {
    udp: UdpSource,
}

impl Dirt2Source {
    /// Binds the port the game is configured to send to.
    pub fn bind() -> io::Result<Self> {
        Ok(Self {
            udp: UdpSource::bind(PORT)?,
        })
    }

    pub fn bind_to(address: &SocketAddr) -> io::Result<Self> {
        Ok(Self {
            udp: UdpSource::bind_to(address)?,
        })
    }

    pub fn local_addr(&self) -> io::Result<SocketAddr> {
        self.udp.local_addr()
    }

    pub fn set_read_timeout(&self, timeout: Option<Duration>) -> io::Result<()> {
        self.udp.set_read_timeout(timeout)
    }

    /// Waits for one datagram. `None` means it arrived but was too short to be
    /// telemetry, which is worth ignoring rather than treating as a failure.
    pub fn recv(&mut self) -> io::Result<Option<Dirt2Telemetry>> {
        Ok(decode(self.udp.recv()?))
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn packet_with(fields: &[(usize, f32)]) -> Vec<u8> {
        let mut bytes = vec![0u8; PACKET_SIZE];
        for (offset, value) in fields {
            bytes[*offset..*offset + 4].copy_from_slice(&value.to_le_bytes());
        }
        bytes
    }

    #[test]
    fn a_short_datagram_is_rejected() {
        assert!(decode(&[0u8; PACKET_SIZE - 1]).is_none());
        assert!(decode(&[]).is_none());
    }

    #[test]
    fn every_field_is_read_from_its_documented_offset() {
        let bytes = packet_with(&[
            (LAP_TIME, 123.5),
            (DISTANCE, 4200.0),
            (PROGRESS, 0.42),
            (SPEED_MS, 30.0),
            (GEAR, 3.0),
            (RPM, 620.5),
            (MAX_RPM, 750.0),
            (CAR_POS, 2.0),
            (SECTOR_1_TIME, 41.5),
            (SECTOR_2_TIME, 80.25),
        ]);

        let decoded = decode(&bytes).expect("a full packet decodes");
        assert_eq!(decoded.lap_time, 123.5);
        assert_eq!(decoded.distance, 4200.0);
        assert_eq!(decoded.progress, 0.42);
        assert_eq!(decoded.speed_ms, 30.0);
        assert_eq!(decoded.gear, 3.0);
        assert_eq!(decoded.rpm, 620.5);
        assert_eq!(decoded.max_rpm, 750.0);
        assert_eq!(decoded.car_pos, 2.0);
        assert_eq!(decoded.sector_1_time, 41.5);
        assert_eq!(decoded.sector_2_time, 80.25);
    }

    #[test]
    fn a_longer_datagram_still_decodes() {
        // Other extradata settings produce longer packets that start the same way.
        let mut bytes = packet_with(&[(SPEED_MS, 25.0)]);
        bytes.extend_from_slice(&[0xAB; 32]);
        assert_eq!(decode(&bytes).expect("decodes").speed_ms, 25.0);
    }

    #[test]
    fn telemetry_arrives_over_a_real_socket() {
        use std::net::{SocketAddr, UdpSocket};

        let mut source = Dirt2Source::bind_to(&SocketAddr::from(([127, 0, 0, 1], 0)))
            .expect("binds a free port");
        let address = source.local_addr().expect("has an address");
        let game = UdpSocket::bind(SocketAddr::from(([127, 0, 0, 1], 0))).expect("binds");

        game.send_to(&packet_with(&[(SPEED_MS, 30.0), (GEAR, 3.0)]), address)
            .expect("sends");

        let telemetry = source.recv().expect("receives").expect("decodes");
        assert_eq!(telemetry.speed_ms, 30.0);
        assert_eq!(telemetry.gear, 3.0);
    }

    #[test]
    fn a_runt_datagram_is_ignored_rather_than_failing() {
        use std::net::{SocketAddr, UdpSocket};

        let mut source = Dirt2Source::bind_to(&SocketAddr::from(([127, 0, 0, 1], 0)))
            .expect("binds a free port");
        let address = source.local_addr().expect("has an address");
        let game = UdpSocket::bind(SocketAddr::from(([127, 0, 0, 1], 0))).expect("binds");

        game.send_to(&[0u8; 8], address).expect("sends");
        assert_eq!(source.recv().expect("receives"), None);
    }

    #[test]
    fn an_all_zero_packet_decodes_to_a_stationary_car() {
        let decoded = decode(&[0u8; PACKET_SIZE]).expect("decodes");
        assert_eq!(decoded, Dirt2Telemetry::default());
    }
}

/// Maps `simetry`'s decoded datagram onto [`Dirt2Telemetry`].
///
/// Type-checked against `simetry` but not unit-tested. The byte decoder above
/// stays the tested path, and remains the fuller one: `simetry` does not expose
/// the sector split times, so those are lost here.
pub mod simetry_source {
    use simetry::dirt_rally_2::SimState;
    use simhub_model::telemetry::Dirt2Telemetry;

    pub fn telemetry_from(state: &SimState) -> Dirt2Telemetry {
        Dirt2Telemetry {
            lap_time: state.time_of_current_lap,
            distance: state.distance_driven_on_current_lap,
            // The C# struct names the field at this offset `progress` and reads
            // it as a 0-1 fraction; `simetry` reads the same offset as the
            // overall distance driven, matching the published Codemasters
            // layout. Mapped the way the converter expects, so behaviour is
            // unchanged — but one of the two readings is wrong.
            progress: state.distance_driven_overall,
            speed_ms: state.velocity_ms,
            gear: state.gear,
            rpm: state.speed_of_engine_rpm_div_10,
            max_rpm: state.maximum_rpm_div_10,
            car_pos: state.current_lap,
            // Not exposed by `simetry`; the byte decoder reads both.
            sector_1_time: 0.0,
            sector_2_time: 0.0,
        }
    }
}

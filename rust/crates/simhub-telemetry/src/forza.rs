//! Decodes the Forza Horizon 5 "Data Out" datagram.
//!
//! # Packet length
//!
//! The layout is the Forza Motorsport 7 "car dash" format with twelve bytes
//! inserted after the 232-byte sled block, which Horizon uses for its own
//! values. That gives 232 + 12 + 79 = 323 bytes of documented fields.
//!
//! Published descriptions of the Horizon dash format give the datagram as 324
//! bytes, one more than the fields account for. The decoder therefore requires
//! [`PACKET_SIZE`] bytes and ignores anything beyond, which handles both
//! readings — but **which is right has not been confirmed against the running
//! game**. If the trailing byte turns out to belong to a field, every offset
//! after it would be wrong and the dashboard would still look plausible.
//! Checking one real datagram settles it.

use crate::read::{f32_at, i32_at, u16_at};
use crate::udp::UdpSource;
use simhub_model::telemetry::ForzaTelemetry;
use std::io;
use std::net::SocketAddr;
use std::time::Duration;

/// The UDP port the game is configured to send to.
pub const DEFAULT_PORT: u16 = 5300;

/// Bytes of documented fields in the Horizon dash layout.
pub const PACKET_SIZE: usize = 323;

/// The datagram length published descriptions give for FH4 and FH5.
pub const DASH_HORIZON_DATAGRAM_SIZE: usize = 324;

/// Where the shared sled block ends and Horizon's own values begin.
pub const SLED_BLOCK_SIZE: usize = 232;

// Offsets within the packet, matching the generated layout manifest.
const IS_RACE_ON: usize = 0;
const ENGINE_MAX_RPM: usize = 8;
const CURRENT_ENGINE_RPM: usize = 16;
const CAR_ORDINAL: usize = 212;
const CAR_CLASS: usize = 216;
const CAR_PERFORMANCE_INDEX: usize = 220;
const DRIVETRAIN_TYPE: usize = 224;
const NUM_CYLINDERS: usize = 228;
const SPEED: usize = 256;
const POWER: usize = 260;
const TORQUE: usize = 264;
const FUEL: usize = 288;
const BEST_LAP: usize = 296;
const LAST_LAP: usize = 300;
const CURRENT_LAP: usize = 304;
const LAP_NUMBER: usize = 312;
const RACE_POSITION: usize = 314;
const THROTTLE: usize = 315;
const BRAKE: usize = 316;
const CLUTCH: usize = 317;
const GEAR: usize = 319;
const STEER: usize = 320;

/// Decodes one datagram, or `None` when it is too short to be one.
pub fn decode(bytes: &[u8]) -> Option<ForzaTelemetry> {
    if bytes.len() < PACKET_SIZE {
        return None;
    }

    Some(ForzaTelemetry {
        is_race_on: i32_at(bytes, IS_RACE_ON),
        engine_max_rpm: f32_at(bytes, ENGINE_MAX_RPM),
        current_engine_rpm: f32_at(bytes, CURRENT_ENGINE_RPM),
        speed: f32_at(bytes, SPEED),
        power: f32_at(bytes, POWER),
        torque: f32_at(bytes, TORQUE),
        fuel: f32_at(bytes, FUEL),
        best_lap: f32_at(bytes, BEST_LAP),
        last_lap: f32_at(bytes, LAST_LAP),
        current_lap: f32_at(bytes, CURRENT_LAP),
        lap_number: u16_at(bytes, LAP_NUMBER),
        race_position: bytes[RACE_POSITION],
        throttle: bytes[THROTTLE],
        brake: bytes[BRAKE],
        clutch: bytes[CLUTCH],
        gear: bytes[GEAR],
        steer: bytes[STEER] as i8,
        car_ordinal: i32_at(bytes, CAR_ORDINAL),
        car_class: i32_at(bytes, CAR_CLASS),
        car_performance_index: i32_at(bytes, CAR_PERFORMANCE_INDEX),
        drivetrain_type: i32_at(bytes, DRIVETRAIN_TYPE),
        num_cylinders: i32_at(bytes, NUM_CYLINDERS),
    })
}

/// Receives and decodes Forza Horizon 5 telemetry.
#[derive(Debug)]
pub struct ForzaSource {
    udp: UdpSource,
}

impl ForzaSource {
    /// Binds the port the game's Data Out is configured to send to.
    pub fn bind() -> io::Result<Self> {
        Ok(Self {
            udp: UdpSource::bind(DEFAULT_PORT)?,
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
    /// telemetry.
    pub fn recv(&mut self) -> io::Result<Option<ForzaTelemetry>> {
        Ok(decode(self.udp.recv()?))
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    struct Packet(Vec<u8>);

    impl Packet {
        fn new(len: usize) -> Self {
            Self(vec![0u8; len])
        }
        fn f32(mut self, offset: usize, value: f32) -> Self {
            self.0[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
            self
        }
        fn i32(mut self, offset: usize, value: i32) -> Self {
            self.0[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
            self
        }
        fn u16(mut self, offset: usize, value: u16) -> Self {
            self.0[offset..offset + 2].copy_from_slice(&value.to_le_bytes());
            self
        }
        fn byte(mut self, offset: usize, value: u8) -> Self {
            self.0[offset] = value;
            self
        }
    }

    #[test]
    fn a_short_datagram_is_rejected() {
        assert!(decode(&[0u8; PACKET_SIZE - 1]).is_none());
        assert!(decode(&[]).is_none());
    }

    #[test]
    fn every_field_is_read_from_its_documented_offset() {
        let packet = Packet::new(PACKET_SIZE)
            .i32(IS_RACE_ON, 1)
            .f32(ENGINE_MAX_RPM, 7500.0)
            .f32(CURRENT_ENGINE_RPM, 4200.5)
            .f32(SPEED, 55.5)
            .f32(POWER, 410_000.0)
            .f32(TORQUE, 720.0)
            .f32(FUEL, 0.75)
            .f32(BEST_LAP, 92.5)
            .f32(LAST_LAP, 95.25)
            .f32(CURRENT_LAP, 31.5)
            .u16(LAP_NUMBER, 3)
            .byte(RACE_POSITION, 7)
            .byte(THROTTLE, 255)
            .byte(BRAKE, 128)
            .byte(CLUTCH, 64)
            .byte(GEAR, 5)
            .byte(STEER, 0x80) // -128 as a signed byte
            .i32(CAR_ORDINAL, 2402)
            .i32(CAR_CLASS, 5)
            .i32(CAR_PERFORMANCE_INDEX, 812)
            .i32(DRIVETRAIN_TYPE, 2)
            .i32(NUM_CYLINDERS, 8);

        let decoded = decode(&packet.0).expect("a full packet decodes");
        assert_eq!(decoded.is_race_on, 1);
        assert_eq!(decoded.engine_max_rpm, 7500.0);
        assert_eq!(decoded.current_engine_rpm, 4200.5);
        assert_eq!(decoded.speed, 55.5);
        assert_eq!(decoded.power, 410_000.0);
        assert_eq!(decoded.torque, 720.0);
        assert_eq!(decoded.fuel, 0.75);
        assert_eq!(decoded.best_lap, 92.5);
        assert_eq!(decoded.last_lap, 95.25);
        assert_eq!(decoded.current_lap, 31.5);
        assert_eq!(decoded.lap_number, 3);
        assert_eq!(decoded.race_position, 7);
        assert_eq!(decoded.throttle, 255);
        assert_eq!(decoded.brake, 128);
        assert_eq!(decoded.clutch, 64);
        assert_eq!(decoded.gear, 5);
        assert_eq!(decoded.steer, -128);
        assert_eq!(decoded.car_ordinal, 2402);
        assert_eq!(decoded.car_class, 5);
        assert_eq!(decoded.car_performance_index, 812);
        assert_eq!(decoded.drivetrain_type, 2);
        assert_eq!(decoded.num_cylinders, 8);
    }

    #[test]
    fn the_published_324_byte_datagram_decodes_the_same_as_323() {
        // Whether the real game sends 323 or 324 is unconfirmed, so the decoder
        // must not care. A trailing byte is ignored rather than shifting fields.
        let base = Packet::new(PACKET_SIZE).f32(SPEED, 55.5).byte(GEAR, 5);
        let mut longer = base.0.clone();
        longer.push(0xFF);
        assert_eq!(longer.len(), DASH_HORIZON_DATAGRAM_SIZE);

        assert_eq!(decode(&base.0), decode(&longer));
    }

    #[test]
    fn horizon_values_sit_directly_after_the_sled_block() {
        // The twelve bytes Horizon inserts start where the shared sled ends, so
        // everything the dashboard reads lives past them.
        assert!(SPEED >= SLED_BLOCK_SIZE + 12);
        assert_eq!(SLED_BLOCK_SIZE + 12 + 79, PACKET_SIZE);
    }

    #[test]
    fn a_published_length_datagram_arrives_over_a_real_socket() {
        use std::net::{SocketAddr, UdpSocket};

        let mut source = ForzaSource::bind_to(&SocketAddr::from(([127, 0, 0, 1], 0)))
            .expect("binds a free port");
        let address = source.local_addr().expect("has an address");
        let game = UdpSocket::bind(SocketAddr::from(([127, 0, 0, 1], 0))).expect("binds");

        // Sent at the published 324 bytes rather than the 323 the fields occupy.
        let mut datagram = Packet::new(PACKET_SIZE).f32(SPEED, 55.5).byte(GEAR, 5).0;
        datagram.push(0);
        assert_eq!(datagram.len(), DASH_HORIZON_DATAGRAM_SIZE);
        game.send_to(&datagram, address).expect("sends");

        let telemetry = source.recv().expect("receives").expect("decodes");
        assert_eq!(telemetry.speed, 55.5);
        assert_eq!(telemetry.gear, 5);
    }

    #[test]
    fn an_all_zero_packet_decodes_to_a_paused_game() {
        let decoded = decode(&[0u8; PACKET_SIZE]).expect("decodes");
        assert_eq!(decoded, ForzaTelemetry::default());
        assert_eq!(decoded.is_race_on, 0);
    }
}

//! Decodes the Assetto Corsa shared memory pages.
//!
//! The page names are shared with ACC and AC Rally, but this layout is the
//! oldest and smallest of the three and is not interchangeable with them.

use crate::read::{f32_at, i32_at, utf16_at};
use simhub_model::telemetry::AcTelemetry;

pub const PHYSICS_PAGE: &str = r"Local\acpmf_physics";
pub const GRAPHICS_PAGE: &str = r"Local\acpmf_graphics";
pub const STATIC_PAGE: &str = r"Local\acpmf_static";

pub const PHYSICS_SIZE: usize = 580;
pub const GRAPHICS_SIZE: usize = 252;
pub const STATIC_SIZE: usize = 420;

mod physics {
    pub const PACKET_ID: usize = 0;
    pub const GAS: usize = 4;
    pub const BRAKE: usize = 8;
    pub const FUEL: usize = 12;
    pub const GEAR: usize = 16;
    pub const RPMS: usize = 20;
    pub const STEER_ANGLE: usize = 24;
    pub const SPEED_KMH: usize = 28;
    pub const PIT_LIMITER_ON: usize = 248;
    pub const AIR_TEMP: usize = 288;
    pub const ROAD_TEMP: usize = 292;
    pub const CLUTCH: usize = 364;
    pub const BRAKE_BIAS: usize = 564;
}

mod graphics {
    pub const SESSION_TYPE: usize = 8;
    pub const COMPLETED_LAPS: usize = 132;
    pub const POSITION: usize = 136;
    pub const CURRENT_TIME: usize = 140;
    pub const LAST_TIME: usize = 144;
    pub const BEST_TIME: usize = 148;
    pub const SESSION_TIME_LEFT: usize = 152;
    pub const NUMBER_OF_LAPS: usize = 172;
}

mod statics {
    /// Byte lengths, not character counts: these are UTF-16.
    pub const SM_VERSION: (usize, usize) = (0, 30);
    pub const AC_VERSION: (usize, usize) = (30, 30);
    pub const CAR_MODEL: (usize, usize) = (68, 66);
    pub const TRACK: (usize, usize) = (134, 66);
    pub const MAX_RPM: usize = 412;
    pub const MAX_FUEL: usize = 416;
}

pub fn packet_id(page: &[u8]) -> Option<i32> {
    (page.len() >= 4).then(|| i32_at(page, physics::PACKET_ID))
}

/// Decodes one frame from the three pages, or `None` when any is too short.
pub fn decode(
    physics_page: &[u8],
    graphics_page: &[u8],
    static_page: &[u8],
) -> Option<AcTelemetry> {
    if physics_page.len() < PHYSICS_SIZE
        || graphics_page.len() < GRAPHICS_SIZE
        || static_page.len() < STATIC_SIZE
    {
        return None;
    }
    let (p, g, s) = (physics_page, graphics_page, static_page);

    Some(AcTelemetry {
        gas: f32_at(p, physics::GAS),
        brake: f32_at(p, physics::BRAKE),
        clutch: f32_at(p, physics::CLUTCH),
        steer_angle: f32_at(p, physics::STEER_ANGLE),
        gear: i32_at(p, physics::GEAR),
        rpms: i32_at(p, physics::RPMS),
        speed_kmh: f32_at(p, physics::SPEED_KMH),
        fuel: f32_at(p, physics::FUEL),
        air_temp: f32_at(p, physics::AIR_TEMP),
        road_temp: f32_at(p, physics::ROAD_TEMP),
        brake_bias: f32_at(p, physics::BRAKE_BIAS),
        pit_limiter_on: i32_at(p, physics::PIT_LIMITER_ON),

        session_type: i32_at(g, graphics::SESSION_TYPE),
        completed_laps: i32_at(g, graphics::COMPLETED_LAPS),
        number_of_laps: i32_at(g, graphics::NUMBER_OF_LAPS),
        position: i32_at(g, graphics::POSITION),
        current_time: i32_at(g, graphics::CURRENT_TIME),
        last_time: i32_at(g, graphics::LAST_TIME),
        best_time: i32_at(g, graphics::BEST_TIME),
        // Taken as milliseconds here. The ACC reader scales the same offset by
        // a thousand instead; see the note in that module.
        session_time_left: f32_at(g, graphics::SESSION_TIME_LEFT),

        max_rpm: i32_at(s, statics::MAX_RPM),
        max_fuel: f32_at(s, statics::MAX_FUEL),
        car_model: utf16_at(s, statics::CAR_MODEL.0, statics::CAR_MODEL.1),
        track: utf16_at(s, statics::TRACK.0, statics::TRACK.1),
        sm_version: utf16_at(s, statics::SM_VERSION.0, statics::SM_VERSION.1),
        ac_version: utf16_at(s, statics::AC_VERSION.0, statics::AC_VERSION.1),
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    struct Page(Vec<u8>);

    impl Page {
        fn new(size: usize) -> Self {
            Self(vec![0u8; size])
        }
        fn f32(mut self, offset: usize, value: f32) -> Self {
            self.0[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
            self
        }
        fn i32(mut self, offset: usize, value: i32) -> Self {
            self.0[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
            self
        }
        /// Writes a UTF-16 string into a fixed-width field, NUL padded.
        fn text(mut self, field: (usize, usize), value: &str) -> Self {
            let (offset, byte_len) = field;
            let mut encoded: Vec<u8> = value.encode_utf16().flat_map(u16::to_le_bytes).collect();
            encoded.resize(byte_len, 0);
            self.0[offset..offset + byte_len].copy_from_slice(&encoded);
            self
        }
    }

    #[test]
    fn a_short_page_is_rejected() {
        assert!(
            decode(
                &[0u8; PHYSICS_SIZE - 1],
                &[0u8; GRAPHICS_SIZE],
                &[0u8; STATIC_SIZE]
            )
            .is_none()
        );
        assert!(
            decode(
                &[0u8; PHYSICS_SIZE],
                &[0u8; GRAPHICS_SIZE - 1],
                &[0u8; STATIC_SIZE]
            )
            .is_none()
        );
        assert!(
            decode(
                &[0u8; PHYSICS_SIZE],
                &[0u8; GRAPHICS_SIZE],
                &[0u8; STATIC_SIZE - 1]
            )
            .is_none()
        );
    }

    #[test]
    fn fields_are_read_from_their_documented_offsets() {
        let p = Page::new(PHYSICS_SIZE)
            .f32(physics::GAS, 0.75)
            .f32(physics::BRAKE, 0.1)
            .f32(physics::CLUTCH, 1.0)
            .i32(physics::GEAR, 4)
            .i32(physics::RPMS, 5500)
            .f32(physics::SPEED_KMH, 142.0)
            .f32(physics::FUEL, 30.0)
            .f32(physics::BRAKE_BIAS, 0.6)
            .i32(physics::PIT_LIMITER_ON, 1);
        let g = Page::new(GRAPHICS_SIZE)
            .i32(graphics::SESSION_TYPE, 2)
            .i32(graphics::COMPLETED_LAPS, 3)
            .i32(graphics::POSITION, 5)
            .i32(graphics::CURRENT_TIME, 45_000)
            .i32(graphics::BEST_TIME, i32::MAX)
            .f32(graphics::SESSION_TIME_LEFT, 600_000.0);
        let s = Page::new(STATIC_SIZE)
            .i32(statics::MAX_RPM, 7600)
            .f32(statics::MAX_FUEL, 65.0);

        let t = decode(&p.0, &g.0, &s.0).expect("decodes");
        assert_eq!(t.gas, 0.75);
        assert_eq!(t.gear, 4);
        assert_eq!(t.rpms, 5500);
        assert_eq!(t.speed_kmh, 142.0);
        assert_eq!(t.pit_limiter_on, 1);
        assert_eq!(t.session_type, 2);
        assert_eq!(t.completed_laps, 3);
        assert_eq!(t.position, 5);
        assert_eq!(t.current_time, 45_000);
        // The sentinel for "no lap set" survives decoding intact.
        assert_eq!(t.best_time, i32::MAX);
        assert_eq!(t.session_time_left, 600_000.0);
        assert_eq!(t.max_rpm, 7600);
        assert_eq!(t.max_fuel, 65.0);
    }

    #[test]
    fn static_strings_are_utf16_and_stop_at_the_padding() {
        let s = Page::new(STATIC_SIZE)
            .text(statics::CAR_MODEL, "ks_mazda_mx5_cup")
            .text(statics::TRACK, "ks_nordschleife")
            .text(statics::SM_VERSION, "1.7")
            .text(statics::AC_VERSION, "1.16.3");

        let t = decode(&[0u8; PHYSICS_SIZE], &[0u8; GRAPHICS_SIZE], &s.0).expect("decodes");
        assert_eq!(t.car_model, "ks_mazda_mx5_cup");
        assert_eq!(t.track, "ks_nordschleife");
        assert_eq!(t.sm_version, "1.7");
        assert_eq!(t.ac_version, "1.16.3");
    }

    #[test]
    fn the_version_strings_do_not_bleed_into_each_other() {
        // They sit adjacent at offsets 0 and 30, so a missing NUL check would
        // run the first straight into the second.
        let s = Page::new(STATIC_SIZE)
            .text(statics::SM_VERSION, "1.7")
            .text(statics::AC_VERSION, "1.16.3");
        let t = decode(&[0u8; PHYSICS_SIZE], &[0u8; GRAPHICS_SIZE], &s.0).expect("decodes");
        assert_eq!(t.sm_version, "1.7");
    }

    #[test]
    fn empty_pages_decode_to_a_stationary_car() {
        let t = decode(
            &[0u8; PHYSICS_SIZE],
            &[0u8; GRAPHICS_SIZE],
            &[0u8; STATIC_SIZE],
        )
        .expect("decodes");
        assert_eq!(t, AcTelemetry::default());
    }
}

/// Maps `simetry`'s view of the Assetto Corsa pages onto [`AcTelemetry`].
///
/// Type-checked against `simetry` but not unit-tested: its page types exist
/// only on Windows, so there is no fixture to build off it. The byte decoder
/// above stays the tested path.
pub mod simetry_source {
    use simetry::assetto_corsa::{SessionType, SimState};
    use simhub_model::telemetry::AcTelemetry;

    /// Our model carries the raw page value; `simetry` has already turned it
    /// into an enum, so it is mapped back to the numbering the converter knows.
    fn session_type(session: &SessionType) -> i32 {
        match session {
            SessionType::Practice => 0,
            SessionType::Qualify => 1,
            SessionType::Race => 2,
            SessionType::Hotlap => 3,
            SessionType::TimeAttack => 4,
            SessionType::Drift => 5,
            SessionType::Drag => 6,
            SessionType::HotStint => 7,
            SessionType::HotlapSuperPole => 8,
            SessionType::Unknown => -1,
        }
    }

    pub fn telemetry_from(state: &SimState) -> AcTelemetry {
        let physics = &state.physics;
        let graphics = &state.graphics;
        let statics = &state.static_data;

        AcTelemetry {
            gas: physics.gas,
            brake: physics.brake,
            clutch: physics.clutch,
            steer_angle: physics.steer_angle,
            gear: physics.gear,
            rpms: physics.rpm,
            speed_kmh: physics.speed_kmh,
            fuel: physics.fuel,
            air_temp: physics.air_temp,
            road_temp: physics.road_temp,
            brake_bias: physics.brake_bias,
            pit_limiter_on: physics.pit_limiter_on,

            session_type: session_type(&graphics.session),
            completed_laps: graphics.completed_laps,
            number_of_laps: graphics.number_of_laps,
            position: graphics.position,
            // The page carries each time twice, as text and as milliseconds.
            current_time: graphics.i_current_time,
            last_time: graphics.i_last_time,
            best_time: graphics.i_best_time,
            session_time_left: graphics.session_time_left,

            max_rpm: statics.max_rpm,
            max_fuel: statics.max_fuel,
            car_model: statics.car_model.clone(),
            track: statics.track.clone(),
            sm_version: statics.sm_version.clone(),
            ac_version: statics.ac_version.clone(),
        }
    }
}

//! Decodes the Assetto Corsa Rally shared memory pages.
//!
//! # Unverified layout
//!
//! These offsets were derived from ACC's pages rather than from any published
//! specification, and the implementation they came from has never been run
//! against the game. The physics and graphics layouts are byte-identical to
//! ACC's, which is consistent with AC Rally being ACC-derived but is equally
//! consistent with the structs simply having been copied. Offset 1404 is
//! already known to differ in meaning between the two.
//!
//! Treat every field here as a hypothesis until a running game confirms it.

use crate::read::{f32_at, i32_at};
use simhub_model::telemetry::AcRallyTelemetry;

pub const PHYSICS_PAGE: &str = r"Local\acpmf_physics";
pub const GRAPHICS_PAGE: &str = r"Local\acpmf_graphics";
pub const STATIC_PAGE: &str = r"Local\acpmf_static";

pub const PHYSICS_SIZE: usize = 800;
pub const GRAPHICS_SIZE: usize = 1588;
pub const STATIC_SIZE: usize = 820;

mod physics {
    pub const PACKET_ID: usize = 0;
    pub const GEAR: usize = 16;
    pub const RPMS: usize = 20;
    pub const SPEED_KMH: usize = 28;
    pub const CURRENT_MAX_RPM: usize = 588;
}

mod graphics {
    pub const POSITION: usize = 136;
    pub const CURRENT_TIME: usize = 140;
    pub const DISTANCE_TRAVELED: usize = 156;
    pub const CURRENT_SECTOR_INDEX: usize = 164;
    pub const NORMALIZED_CAR_POSITION: usize = 248;
}

mod statics {
    pub const MAX_RPM: usize = 412;
    pub const TRACK_SPLINE_LENGTH: usize = 520;
}

pub fn packet_id(page: &[u8]) -> Option<i32> {
    (page.len() >= 4).then(|| i32_at(page, physics::PACKET_ID))
}

/// Decodes one frame from the three pages, or `None` when any is too short.
pub fn decode(
    physics_page: &[u8],
    graphics_page: &[u8],
    static_page: &[u8],
) -> Option<AcRallyTelemetry> {
    if physics_page.len() < PHYSICS_SIZE
        || graphics_page.len() < GRAPHICS_SIZE
        || static_page.len() < STATIC_SIZE
    {
        return None;
    }
    let (p, g, s) = (physics_page, graphics_page, static_page);

    Some(AcRallyTelemetry {
        gear: i32_at(p, physics::GEAR),
        rpms: i32_at(p, physics::RPMS),
        current_max_rpm: f32_at(p, physics::CURRENT_MAX_RPM),
        speed_kmh: f32_at(p, physics::SPEED_KMH),

        current_time: i32_at(g, graphics::CURRENT_TIME),
        position: i32_at(g, graphics::POSITION),
        normalized_car_position: f32_at(g, graphics::NORMALIZED_CAR_POSITION),
        distance_traveled: f32_at(g, graphics::DISTANCE_TRAVELED),
        current_sector_index: i32_at(g, graphics::CURRENT_SECTOR_INDEX),

        max_rpm: i32_at(s, statics::MAX_RPM),
        track_spline_length: f32_at(s, statics::TRACK_SPLINE_LENGTH),
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
    }

    fn empty() -> (Vec<u8>, Vec<u8>, Vec<u8>) {
        (
            vec![0u8; PHYSICS_SIZE],
            vec![0u8; GRAPHICS_SIZE],
            vec![0u8; STATIC_SIZE],
        )
    }

    #[test]
    fn a_short_page_is_rejected() {
        let (p, g, s) = empty();
        assert!(decode(&p[..PHYSICS_SIZE - 1], &g, &s).is_none());
        assert!(decode(&p, &g[..GRAPHICS_SIZE - 1], &s).is_none());
        assert!(decode(&p, &g, &s[..STATIC_SIZE - 1]).is_none());
    }

    #[test]
    fn fields_are_read_from_their_assumed_offsets() {
        let p = Page::new(PHYSICS_SIZE)
            .i32(physics::GEAR, 3)
            .i32(physics::RPMS, 4800)
            .f32(physics::CURRENT_MAX_RPM, 7200.0)
            .f32(physics::SPEED_KMH, 96.5);
        let g = Page::new(GRAPHICS_SIZE)
            .i32(graphics::CURRENT_TIME, 123_456)
            .i32(graphics::POSITION, 1)
            .f32(graphics::NORMALIZED_CAR_POSITION, 0.42)
            .f32(graphics::DISTANCE_TRAVELED, 4200.0)
            .i32(graphics::CURRENT_SECTOR_INDEX, 2);
        let s = Page::new(STATIC_SIZE)
            .i32(statics::MAX_RPM, 7000)
            .f32(statics::TRACK_SPLINE_LENGTH, 10_000.0);

        let t = decode(&p.0, &g.0, &s.0).expect("decodes");
        assert_eq!(t.gear, 3);
        assert_eq!(t.rpms, 4800);
        assert_eq!(t.current_max_rpm, 7200.0);
        assert_eq!(t.speed_kmh, 96.5);
        assert_eq!(t.current_time, 123_456);
        assert_eq!(t.position, 1);
        assert_eq!(t.normalized_car_position, 0.42);
        assert_eq!(t.distance_traveled, 4200.0);
        assert_eq!(t.current_sector_index, 2);
        assert_eq!(t.max_rpm, 7000);
        assert_eq!(t.track_spline_length, 10_000.0);
    }

    #[test]
    fn the_physics_and_graphics_pages_match_the_competizione_sizes() {
        // Not evidence that the layouts agree, only that the assumption this
        // decoder rests on is the one the C# structs encode.
        assert_eq!(PHYSICS_SIZE, crate::acc::PHYSICS_SIZE);
        assert_eq!(GRAPHICS_SIZE, crate::acc::GRAPHICS_SIZE);
    }

    #[test]
    fn empty_pages_decode_to_a_stationary_car() {
        let (p, g, s) = empty();
        assert_eq!(
            decode(&p, &g, &s).expect("decodes"),
            AcRallyTelemetry::default()
        );
    }
}

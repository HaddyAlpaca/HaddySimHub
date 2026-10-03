//! Decodes the Assetto Corsa Competizione shared memory pages.
//!
//! ACC publishes under the same page names as Assetto Corsa and AC Rally but
//! with its own layouts, so the presence of a page says nothing about which
//! title is running; the display decides that from the process.

use crate::read::{f32_at, i32_at};
use simhub_model::telemetry::AccTelemetry;

pub const PHYSICS_PAGE: &str = r"Local\acpmf_physics";
pub const GRAPHICS_PAGE: &str = r"Local\acpmf_graphics";

pub const PHYSICS_SIZE: usize = 800;
pub const GRAPHICS_SIZE: usize = 1588;

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
    pub const CURRENT_MAX_RPM: usize = 588;
}

mod graphics {
    pub const SESSION_TYPE: usize = 8;
    pub const COMPLETED_LAP: usize = 132;
    pub const POSITION: usize = 136;
    pub const CURRENT_TIME: usize = 140;
    pub const LAST_TIME: usize = 144;
    pub const BEST_TIME: usize = 148;
    pub const SESSION_TIME_LEFT: usize = 152;
    pub const NUMBER_OF_LAPS: usize = 172;
    pub const WIND_SPEED: usize = 1248;
    pub const WIND_DIRECTION: usize = 1252;
    pub const FUEL_PER_LAP: usize = 1284;
    pub const DELTA_LAP_TIME: usize = 1360;
    pub const FUEL_ESTIMATED_LAPS: usize = 1412;
    pub const TRACK_GRIP_STATUS: usize = 1556;
    pub const RAIN_INTENSITY: usize = 1560;
}

/// The counter both pages carry, which advances once per published frame.
/// Equal ids mean the two pages describe the same moment.
pub fn packet_id(page: &[u8]) -> Option<i32> {
    // Both pages carry the counter first, at the same offset.
    (page.len() >= 4).then(|| i32_at(page, physics::PACKET_ID))
}

/// Decodes one frame from the two pages, or `None` when either is too short.
pub fn decode(physics_page: &[u8], graphics_page: &[u8]) -> Option<AccTelemetry> {
    if physics_page.len() < PHYSICS_SIZE || graphics_page.len() < GRAPHICS_SIZE {
        return None;
    }
    let p = physics_page;
    let g = graphics_page;

    Some(AccTelemetry {
        gas: f32_at(p, physics::GAS),
        brake: f32_at(p, physics::BRAKE),
        clutch: f32_at(p, physics::CLUTCH),
        fuel: f32_at(p, physics::FUEL),
        gear: i32_at(p, physics::GEAR),
        rpms: i32_at(p, physics::RPMS),
        max_rpm: f32_at(p, physics::CURRENT_MAX_RPM),
        steer_angle: f32_at(p, physics::STEER_ANGLE),
        speed_kmh: f32_at(p, physics::SPEED_KMH),
        air_temp: f32_at(p, physics::AIR_TEMP),
        road_temp: f32_at(p, physics::ROAD_TEMP),
        brake_bias: f32_at(p, physics::BRAKE_BIAS),
        pit_limiter_on: i32_at(p, physics::PIT_LIMITER_ON),

        session_type: i32_at(g, graphics::SESSION_TYPE),
        current_time_ms: i32_at(g, graphics::CURRENT_TIME),
        last_time_ms: i32_at(g, graphics::LAST_TIME),
        best_time_ms: i32_at(g, graphics::BEST_TIME),
        delta_ms: i32_at(g, graphics::DELTA_LAP_TIME),
        current_lap: i32_at(g, graphics::COMPLETED_LAP),
        number_of_laps: i32_at(g, graphics::NUMBER_OF_LAPS),
        position: i32_at(g, graphics::POSITION),
        // The C# reader scaled this by a thousand, treating the page value as
        // seconds. The Assetto Corsa reader divides the same field at the same
        // offset by a thousand instead, treating it as milliseconds. Only one
        // can be right; this preserves the existing ACC behaviour. See the
        // module tests.
        session_time_left_ms: (f32_at(g, graphics::SESSION_TIME_LEFT) * 1000.0) as i32,
        fuel_per_lap: f32_at(g, graphics::FUEL_PER_LAP),
        fuel_estimated_laps: f32_at(g, graphics::FUEL_ESTIMATED_LAPS),
        rain_intensity: i32_at(g, graphics::RAIN_INTENSITY),
        wind_speed: f32_at(g, graphics::WIND_SPEED),
        wind_direction: f32_at(g, graphics::WIND_DIRECTION),
        track_grip_status: i32_at(g, graphics::TRACK_GRIP_STATUS),
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    pub(crate) struct Page(pub Vec<u8>);

    impl Page {
        pub fn new(size: usize) -> Self {
            Self(vec![0u8; size])
        }
        pub fn f32(mut self, offset: usize, value: f32) -> Self {
            self.0[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
            self
        }
        pub fn i32(mut self, offset: usize, value: i32) -> Self {
            self.0[offset..offset + 4].copy_from_slice(&value.to_le_bytes());
            self
        }
    }

    #[test]
    fn a_short_page_is_rejected() {
        assert!(decode(&[0u8; PHYSICS_SIZE - 1], &[0u8; GRAPHICS_SIZE]).is_none());
        assert!(decode(&[0u8; PHYSICS_SIZE], &[0u8; GRAPHICS_SIZE - 1]).is_none());
    }

    #[test]
    fn physics_fields_are_read_from_their_documented_offsets() {
        let p = Page::new(PHYSICS_SIZE)
            .f32(physics::GAS, 0.5)
            .f32(physics::BRAKE, 0.25)
            .f32(physics::CLUTCH, 1.0)
            .f32(physics::FUEL, 42.5)
            .i32(physics::GEAR, 3)
            .i32(physics::RPMS, 6200)
            .f32(physics::CURRENT_MAX_RPM, 7500.0)
            .f32(physics::STEER_ANGLE, -0.4)
            .f32(physics::SPEED_KMH, 188.5)
            .f32(physics::AIR_TEMP, 21.5)
            .f32(physics::ROAD_TEMP, 31.0)
            .f32(physics::BRAKE_BIAS, 0.57)
            .i32(physics::PIT_LIMITER_ON, 1);

        let t = decode(&p.0, &[0u8; GRAPHICS_SIZE]).expect("decodes");
        assert_eq!(t.gas, 0.5);
        assert_eq!(t.brake, 0.25);
        assert_eq!(t.clutch, 1.0);
        assert_eq!(t.fuel, 42.5);
        assert_eq!(t.gear, 3);
        assert_eq!(t.rpms, 6200);
        assert_eq!(t.max_rpm, 7500.0);
        assert_eq!(t.steer_angle, -0.4);
        assert_eq!(t.speed_kmh, 188.5);
        assert_eq!(t.air_temp, 21.5);
        assert_eq!(t.road_temp, 31.0);
        assert_eq!(t.brake_bias, 0.57);
        assert_eq!(t.pit_limiter_on, 1);
    }

    #[test]
    fn graphics_fields_are_read_from_their_documented_offsets() {
        let g = Page::new(GRAPHICS_SIZE)
            .i32(graphics::SESSION_TYPE, 2)
            .i32(graphics::COMPLETED_LAP, 4)
            .i32(graphics::POSITION, 3)
            .i32(graphics::CURRENT_TIME, 31_500)
            .i32(graphics::LAST_TIME, 92_250)
            .i32(graphics::BEST_TIME, 90_000)
            .i32(graphics::NUMBER_OF_LAPS, 12)
            .i32(graphics::DELTA_LAP_TIME, -1_250)
            .f32(graphics::FUEL_PER_LAP, 2.6)
            .f32(graphics::FUEL_ESTIMATED_LAPS, 16.0)
            .f32(graphics::WIND_SPEED, 3.5)
            .f32(graphics::WIND_DIRECTION, 1.57)
            .i32(graphics::TRACK_GRIP_STATUS, 2)
            .i32(graphics::RAIN_INTENSITY, 1);

        let t = decode(&[0u8; PHYSICS_SIZE], &g.0).expect("decodes");
        assert_eq!(t.session_type, 2);
        assert_eq!(t.current_lap, 4);
        assert_eq!(t.position, 3);
        assert_eq!(t.current_time_ms, 31_500);
        assert_eq!(t.last_time_ms, 92_250);
        assert_eq!(t.best_time_ms, 90_000);
        assert_eq!(t.number_of_laps, 12);
        assert_eq!(t.delta_ms, -1_250);
        assert_eq!(t.fuel_per_lap, 2.6);
        assert_eq!(t.fuel_estimated_laps, 16.0);
        assert_eq!(t.wind_speed, 3.5);
        assert_eq!(t.wind_direction, 1.57);
        assert_eq!(t.track_grip_status, 2);
        assert_eq!(t.rain_intensity, 1);
    }

    #[test]
    fn session_time_is_scaled_by_a_thousand_as_the_c_sharp_reader_did() {
        // Assetto Corsa reads the same offset in the same-lineage page and
        // treats it as milliseconds instead. If a twenty-minute session shows
        // as 1200 seconds here, this scaling is wrong; if it shows as 1.2
        // seconds without it, the Assetto Corsa reader is.
        let g = Page::new(GRAPHICS_SIZE).f32(graphics::SESSION_TIME_LEFT, 1200.0);
        let t = decode(&[0u8; PHYSICS_SIZE], &g.0).expect("decodes");
        assert_eq!(t.session_time_left_ms, 1_200_000);
    }

    #[test]
    fn the_packet_id_identifies_the_frame_each_page_describes() {
        let p = Page::new(PHYSICS_SIZE).i32(physics::PACKET_ID, 77);
        let g = Page::new(GRAPHICS_SIZE).i32(0, 77);
        assert_eq!(packet_id(&p.0), Some(77));
        assert_eq!(packet_id(&g.0), Some(77));
        assert_eq!(packet_id(&[0u8; 2]), None);
    }

    #[test]
    fn empty_pages_decode_to_a_stationary_car() {
        let t = decode(&[0u8; PHYSICS_SIZE], &[0u8; GRAPHICS_SIZE]).expect("decodes");
        assert_eq!(t, AccTelemetry::default());
    }
}

/// Maps `simetry`'s view of the ACC pages onto [`AccTelemetry`].
///
/// Type-checked against `simetry` but not unit-tested: its page types exist
/// only on Windows. The byte decoder above stays the tested path.
///
/// Two things differ from that decoder. The rev limit is read from the static
/// page, which is where ACC actually publishes it. And `number_of_laps` has no
/// counterpart in `simetry`'s model, so a lap-limited session loses its total.
#[cfg(windows)]
pub mod simetry_source {
    use simetry::assetto_corsa_competizione::{
        RainIntensity, SessionType, SimState, TrackGripStatus,
    };
    use simhub_model::telemetry::AccTelemetry;

    /// Our model carries the raw page value; `simetry` has already turned these
    /// into enums, so they are mapped back to the numbering the converter knows.
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

    fn rain_intensity(rain: &RainIntensity) -> i32 {
        match rain {
            RainIntensity::NoRain => 0,
            RainIntensity::Drizzle => 1,
            RainIntensity::LightRain => 2,
            RainIntensity::MediumRain => 3,
            RainIntensity::HeavyRain => 4,
            RainIntensity::Thunderstorm => 5,
        }
    }

    fn track_grip(grip: &TrackGripStatus) -> i32 {
        match grip {
            TrackGripStatus::Green => 0,
            TrackGripStatus::Fast => 1,
            TrackGripStatus::Optimum => 2,
            TrackGripStatus::Greasy => 3,
            TrackGripStatus::Damp => 4,
            TrackGripStatus::Wet => 5,
            TrackGripStatus::Flooded => 6,
        }
    }

    pub fn telemetry_from(state: &SimState) -> AccTelemetry {
        let physics = &state.physics;
        let graphics = &state.graphics;
        let statics = &state.static_data;
        let timing = &graphics.lap_timing;

        AccTelemetry {
            gas: physics.gas,
            brake: physics.brake,
            clutch: physics.clutch,
            fuel: physics.fuel,
            gear: physics.gear,
            rpms: physics.rpm,
            // The physics page has a `current_max_rpm`, but ACC never writes
            // it; the real limit is on the static page. The C# reader used the
            // physics field and so reported a limit the sim never supplied.
            max_rpm: statics.max_rpm as f32,
            steer_angle: physics.steer_angle,
            speed_kmh: physics.speed_kmh,
            air_temp: physics.air_temperature,
            road_temp: physics.road_temperature,
            brake_bias: physics.brake_bias,
            pit_limiter_on: physics.pit_limiter_on as i32,

            session_type: session_type(&graphics.session),
            current_time_ms: timing.current.millis,
            last_time_ms: timing.last.millis,
            best_time_ms: timing.best.millis,
            delta_ms: timing.delta_lap.millis,
            current_lap: graphics.completed_laps,
            // `simetry` does not expose the session's lap total, so a
            // lap-limited session reads as unlimited here.
            number_of_laps: 0,
            position: graphics.position,
            session_time_left_ms: (graphics.session_time_left * 1000.0) as i32,
            fuel_per_lap: graphics.fuel_used_per_lap,
            fuel_estimated_laps: graphics.fuel_estimated_laps,
            rain_intensity: rain_intensity(&graphics.rain_intensity),
            wind_speed: graphics.wind_speed,
            wind_direction: graphics.wind_direction,
            track_grip_status: track_grip(&graphics.track_grip_status),
        }
    }
}

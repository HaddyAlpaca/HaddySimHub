//! Reads iRacing telemetry through `simetry`.
//!
//! iRacing is the one source where the acquisition is genuinely hard: the map
//! holds a ring of buffers behind a variable-length header, plus a session YAML
//! blob that is rewritten as the session changes. `simetry` does that work and
//! hands back a state that can be queried by variable name.
//!
//! The mapping from those names onto [`IRacingTelemetry`] is kept behind
//! [`VarSource`] so it stays pure and testable off Windows; only the
//! implementation for `simetry`'s own state is platform-gated.

use simhub_model::telemetry::IRacingTelemetry;

/// Bit of the `EngineWarnings` mask that marks the pit speed limiter.
pub const PIT_SPEED_LIMITER: i32 = 0x10;

/// iRacing never fields more than this many cars, which bounds the per-car reads.
const MAX_CARS: usize = 64;

/// Reads iRacing telemetry variables by name.
///
/// `None` means the variable is absent from this session's header — not that it
/// is zero. Different cars and session types publish different sets.
pub trait VarSource {
    fn f32(&self, name: &str) -> Option<f32>;
    fn f64(&self, name: &str) -> Option<f64>;
    fn i32(&self, name: &str) -> Option<i32>;
    fn bool(&self, name: &str) -> Option<bool>;
    fn i32_at(&self, name: &str, index: usize) -> Option<i32>;
}

/// Collects the per-car lap counts, stopping where the array ends.
fn car_idx_lap(vars: &impl VarSource) -> Vec<i32> {
    (0..MAX_CARS)
        .map_while(|index| vars.i32_at("CarIdxLap", index))
        .collect()
}

/// Builds a telemetry frame from whatever the session publishes.
///
/// Absent variables fall back to their zero value, which matches how the C#
/// reader behaved: a missing variable left the field at its default rather than
/// failing the frame.
pub fn telemetry_from(vars: &impl VarSource) -> IRacingTelemetry {
    IRacingTelemetry {
        session_num: vars.i32("SessionNum").unwrap_or_default(),
        player_car_idx: vars.i32("PlayerCarIdx").unwrap_or_default(),
        car_idx_lap: car_idx_lap(vars),
        lap: vars.i32("Lap").unwrap_or_default(),
        fuel_level: vars.f32("FuelLevel").unwrap_or_default(),
        player_car_in_pit_stall: vars.bool("PlayerCarInPitStall").unwrap_or_default(),
        on_pit_road: vars.bool("OnPitRoad").unwrap_or_default(),
        session_time_remain: vars.f64("SessionTimeRemain").unwrap_or_default(),
        player_car_position: vars.i32("PlayerCarPosition").unwrap_or_default(),
        lap_current_lap_time: vars.f32("LapCurrentLapTime").unwrap_or_default(),
        speed: vars.f32("Speed").unwrap_or_default(),
        gear: vars.i32("Gear").unwrap_or_default(),
        rpm: vars.f32("RPM").unwrap_or_default(),
        track_temp: vars.f32("TrackTemp").unwrap_or_default(),
        air_temp: vars.f32("AirTemp").unwrap_or_default(),
        lap_last_lap_time: vars.f32("LapLastLapTime").unwrap_or_default(),
        // The misspelling is iRacing's own; the variable really is named this.
        lap_delta_to_session_last_lap: vars.f32("LapDeltaToSessionLastlLap").unwrap_or_default(),
        lap_best_lap_time: vars.f32("LapBestLapTime").unwrap_or_default(),
        lap_delta_to_best_lap: vars.f32("LapDeltaToBestLap").unwrap_or_default(),
        throttle: vars.f32("Throttle").unwrap_or_default(),
        brake: vars.f32("Brake").unwrap_or_default(),
        pit_speed_limiter: vars
            .i32("EngineWarnings")
            .is_some_and(|warnings| warnings & PIT_SPEED_LIMITER != 0),
        steering_wheel_angle: vars.f32("SteeringWheelAngle").unwrap_or_default(),
        steering_wheel_angle_max: vars.f32("SteeringWheelAngleMax").unwrap_or_default(),
        dc_brake_bias: vars.f32("dcBrakeBias").unwrap_or_default(),
        player_car_driver_incident_count: vars
            .i32("PlayerCarDriverIncidentCount")
            .unwrap_or_default(),
        // These two come from the session YAML rather than the telemetry block.
        car_screen_name: String::new(),
        race_car_iratings: Vec::new(),
    }
}

/// Reading the session YAML.
///
/// iRacing publishes session, driver and weekend information as a YAML blob
/// beside the telemetry block, rewritten whenever the session changes. The
/// values it carries are not uniformly typed: `SessionLaps`, `SessionTime` and
/// `IncidentLimit` are a number when bounded and the string `unlimited` when
/// not, so each is read through a helper that accepts either.
pub mod session {
    use simhub_model::telemetry::{IRacingDriver, IRacingSession};
    use yaml_rust::Yaml;

    /// What iRacing writes where a limit would otherwise go.
    const UNLIMITED: &str = "unlimited";

    fn is_unlimited(value: &Yaml) -> bool {
        value.as_str() == Some(UNLIMITED)
    }

    /// Reads a value that may arrive as a number or as a quoted string.
    fn as_i64(value: &Yaml) -> Option<i64> {
        value
            .as_i64()
            .or_else(|| value.as_str().and_then(|text| text.trim().parse().ok()))
    }

    fn as_i32(value: &Yaml) -> i32 {
        as_i64(value).unwrap_or_default() as i32
    }

    fn as_text(value: &Yaml) -> String {
        value
            .as_str()
            .map(str::to_string)
            .or_else(|| value.as_i64().map(|n| n.to_string()))
            .unwrap_or_default()
    }

    /// The session entry matching the telemetry's current `SessionNum`.
    fn session_entry(yaml: &Yaml, session_num: i32) -> Option<&Yaml> {
        yaml["SessionInfo"]["Sessions"]
            .as_vec()?
            .iter()
            .find(|entry| as_i32(&entry["SessionNum"]) == session_num)
    }

    /// The driver entry for a car index.
    fn driver_entry(yaml: &Yaml, car_idx: i32) -> Option<&Yaml> {
        yaml["DriverInfo"]["Drivers"]
            .as_vec()?
            .iter()
            .find(|entry| as_i32(&entry["CarIdx"]) == car_idx)
    }

    /// Builds the session view the converter needs.
    ///
    /// An absent session or driver leaves the corresponding fields at their
    /// defaults rather than failing: the YAML is rewritten mid-session and can
    /// be briefly inconsistent with the telemetry block.
    pub fn session_from(yaml: &Yaml, session_num: i32, player_car_idx: i32) -> IRacingSession {
        let entry = session_entry(yaml, session_num);
        let laps = entry.map(|e| &e["SessionLaps"]);
        let time = entry.map(|e| &e["SessionTime"]);

        IRacingSession {
            session_type: entry
                .map(|e| as_text(&e["SessionType"]))
                .unwrap_or_default(),
            is_limited_time: time.is_some_and(|value| !is_unlimited(value)),
            is_limited_session_laps: laps.is_some_and(|value| !is_unlimited(value)),
            session_laps: laps
                .filter(|value| !is_unlimited(value))
                .map(|value| as_i32(value))
                .unwrap_or_default(),
            incident_limit: incident_limit(yaml),
            player: driver_entry(yaml, player_car_idx).map(|driver| IRacingDriver {
                car_number: as_text(&driver["CarNumber"]),
                i_rating: as_i32(&driver["IRating"]),
                lic_level: as_i32(&driver["LicLevel"]),
            }),
        }
    }

    /// An unlimited incident allowance is reported as the largest value there
    /// is, which the converter then clamps to the figure the dashboard shows.
    fn incident_limit(yaml: &Yaml) -> i32 {
        let value = &yaml["WeekendInfo"]["WeekendOptions"]["IncidentLimit"];
        if is_unlimited(value) {
            i32::MAX
        } else {
            as_i32(value)
        }
    }

    /// The car the player is driving, as the sim names it on screen.
    pub fn car_screen_name(yaml: &Yaml, player_car_idx: i32) -> String {
        driver_entry(yaml, player_car_idx)
            .map(|driver| as_text(&driver["CarScreenName"]))
            .unwrap_or_default()
    }

    /// The iRatings that strength of field is averaged over.
    ///
    /// The pace car and spectators carry entries in the driver list but are not
    /// competing, so they are left out. The C# reader took this from the SDK's
    /// own race-car collection, which excluded them the same way.
    pub fn race_car_iratings(yaml: &Yaml) -> Vec<i32> {
        let Some(drivers) = yaml["DriverInfo"]["Drivers"].as_vec() else {
            return Vec::new();
        };
        drivers
            .iter()
            .filter(|driver| as_i32(&driver["CarIsPaceCar"]) == 0)
            .filter(|driver| as_i32(&driver["IsSpectator"]) == 0)
            .map(|driver| as_i32(&driver["IRating"]))
            .collect()
    }
}

#[cfg(windows)]
mod simetry_source {
    use super::VarSource;
    use simetry::iracing::SimState;

    impl VarSource for SimState {
        fn f32(&self, name: &str) -> Option<f32> {
            self.read_name(name)
        }
        fn f64(&self, name: &str) -> Option<f64> {
            self.read_name(name)
        }
        fn i32(&self, name: &str) -> Option<i32> {
            self.read_name(name)
        }
        fn bool(&self, name: &str) -> Option<bool> {
            self.read_name(name)
        }
        fn i32_at(&self, name: &str, index: usize) -> Option<i32> {
            self.read_name_at(name, index)
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::HashMap;

    /// A session that publishes exactly the variables it is given.
    #[derive(Default)]
    struct FakeVars {
        floats: HashMap<&'static str, f32>,
        doubles: HashMap<&'static str, f64>,
        ints: HashMap<&'static str, i32>,
        bools: HashMap<&'static str, bool>,
        arrays: HashMap<&'static str, Vec<i32>>,
    }

    impl FakeVars {
        fn f32(mut self, name: &'static str, value: f32) -> Self {
            self.floats.insert(name, value);
            self
        }
        fn f64(mut self, name: &'static str, value: f64) -> Self {
            self.doubles.insert(name, value);
            self
        }
        fn i32(mut self, name: &'static str, value: i32) -> Self {
            self.ints.insert(name, value);
            self
        }
        fn flag(mut self, name: &'static str, value: bool) -> Self {
            self.bools.insert(name, value);
            self
        }
        fn array(mut self, name: &'static str, values: &[i32]) -> Self {
            self.arrays.insert(name, values.to_vec());
            self
        }
    }

    impl VarSource for FakeVars {
        fn f32(&self, name: &str) -> Option<f32> {
            self.floats.get(name).copied()
        }
        fn f64(&self, name: &str) -> Option<f64> {
            self.doubles.get(name).copied()
        }
        fn i32(&self, name: &str) -> Option<i32> {
            self.ints.get(name).copied()
        }
        fn bool(&self, name: &str) -> Option<bool> {
            self.bools.get(name).copied()
        }
        fn i32_at(&self, name: &str, index: usize) -> Option<i32> {
            self.arrays.get(name).and_then(|v| v.get(index)).copied()
        }
    }

    #[test]
    fn a_session_publishing_nothing_yields_a_blank_frame() {
        let telemetry = telemetry_from(&FakeVars::default());
        assert_eq!(telemetry, IRacingTelemetry::default());
    }

    #[test]
    fn scalar_variables_are_read_by_their_iracing_names() {
        let vars = FakeVars::default()
            .i32("SessionNum", 2)
            .i32("PlayerCarIdx", 7)
            .i32("Lap", 12)
            .i32("Gear", 4)
            .i32("PlayerCarPosition", 3)
            .i32("PlayerCarDriverIncidentCount", 6)
            .f32("FuelLevel", 42.5)
            .f32("Speed", 55.5)
            .f32("RPM", 7200.0)
            .f32("TrackTemp", 31.0)
            .f32("AirTemp", 21.5)
            .f32("Throttle", 0.75)
            .f32("Brake", 0.25)
            .f32("SteeringWheelAngle", -0.4)
            .f32("SteeringWheelAngleMax", 12.0)
            .f32("dcBrakeBias", 54.0)
            .f32("LapCurrentLapTime", 31.5)
            .f32("LapLastLapTime", 92.25)
            .f32("LapBestLapTime", 90.0)
            .f64("SessionTimeRemain", 1200.0)
            .flag("OnPitRoad", true)
            .flag("PlayerCarInPitStall", true);

        let t = telemetry_from(&vars);
        assert_eq!(t.session_num, 2);
        assert_eq!(t.player_car_idx, 7);
        assert_eq!(t.lap, 12);
        assert_eq!(t.gear, 4);
        assert_eq!(t.player_car_position, 3);
        assert_eq!(t.player_car_driver_incident_count, 6);
        assert_eq!(t.fuel_level, 42.5);
        assert_eq!(t.speed, 55.5);
        assert_eq!(t.rpm, 7200.0);
        assert_eq!(t.track_temp, 31.0);
        assert_eq!(t.air_temp, 21.5);
        assert_eq!(t.throttle, 0.75);
        assert_eq!(t.brake, 0.25);
        assert_eq!(t.steering_wheel_angle, -0.4);
        assert_eq!(t.steering_wheel_angle_max, 12.0);
        assert_eq!(t.dc_brake_bias, 54.0);
        assert_eq!(t.lap_current_lap_time, 31.5);
        assert_eq!(t.lap_last_lap_time, 92.25);
        assert_eq!(t.lap_best_lap_time, 90.0);
        assert_eq!(t.session_time_remain, 1200.0);
        assert!(t.on_pit_road);
        assert!(t.player_car_in_pit_stall);
    }

    #[test]
    fn the_session_delta_keeps_iracings_own_misspelling() {
        // The variable really is "LapDeltaToSessionLastlLap"; correcting the
        // spelling would silently read nothing.
        let vars = FakeVars::default().f32("LapDeltaToSessionLastlLap", -1.25);
        assert_eq!(telemetry_from(&vars).lap_delta_to_session_last_lap, -1.25);

        let misspelled = FakeVars::default().f32("LapDeltaToSessionLastLap", -1.25);
        assert_eq!(
            telemetry_from(&misspelled).lap_delta_to_session_last_lap,
            0.0
        );
    }

    #[test]
    fn the_pit_limiter_comes_from_a_bit_of_the_warning_mask() {
        let engaged = FakeVars::default().i32("EngineWarnings", PIT_SPEED_LIMITER);
        assert!(telemetry_from(&engaged).pit_speed_limiter);

        // Other warnings in the same mask must not light it.
        let rev_limiter = FakeVars::default().i32("EngineWarnings", 0x20);
        assert!(!telemetry_from(&rev_limiter).pit_speed_limiter);

        // And it survives being set alongside others.
        let both = FakeVars::default().i32("EngineWarnings", 0x20 | PIT_SPEED_LIMITER);
        assert!(telemetry_from(&both).pit_speed_limiter);

        assert!(!telemetry_from(&FakeVars::default()).pit_speed_limiter);
    }

    #[test]
    fn per_car_laps_are_collected_until_the_array_ends() {
        let vars = FakeVars::default().array("CarIdxLap", &[3, 4, 2]);
        assert_eq!(telemetry_from(&vars).car_idx_lap, vec![3, 4, 2]);
    }

    #[test]
    fn a_session_without_per_car_laps_yields_an_empty_list() {
        assert!(telemetry_from(&FakeVars::default()).car_idx_lap.is_empty());
    }
}

#[cfg(test)]
mod session_tests {
    use super::session::*;
    use yaml_rust::{Yaml, YamlLoader};

    /// A session blob shaped like the one iRacing publishes.
    fn yaml(text: &str) -> Yaml {
        YamlLoader::load_from_str(text)
            .expect("valid yaml")
            .remove(0)
    }

    fn race_weekend() -> Yaml {
        yaml(
            r#"
WeekendInfo:
  WeekendOptions:
    IncidentLimit: 17
DriverInfo:
  DriverCarIdx: 0
  Drivers:
    - CarIdx: 0
      CarNumber: "42"
      CarScreenName: FIA F4
      IRating: 2750
      LicLevel: 18
      CarIsPaceCar: 0
      IsSpectator: 0
    - CarIdx: 1
      CarNumber: "7"
      CarScreenName: FIA F4
      IRating: 2250
      LicLevel: 14
      CarIsPaceCar: 0
      IsSpectator: 0
SessionInfo:
  Sessions:
    - SessionNum: 0
      SessionType: Practice
      SessionLaps: unlimited
      SessionTime: unlimited
    - SessionNum: 1
      SessionType: Race
      SessionLaps: 25
      SessionTime: 3600.0000 sec
"#,
        )
    }

    #[test]
    fn the_session_is_picked_by_its_number() {
        let info = race_weekend();
        assert_eq!(session_from(&info, 0, 0).session_type, "Practice");
        assert_eq!(session_from(&info, 1, 0).session_type, "Race");
    }

    #[test]
    fn an_unlimited_practice_is_bounded_by_neither_laps_nor_time() {
        let practice = session_from(&race_weekend(), 0, 0);
        assert!(!practice.is_limited_time);
        assert!(!practice.is_limited_session_laps);
        assert_eq!(practice.session_laps, 0);
    }

    #[test]
    fn a_bounded_race_reports_both_limits() {
        let race = session_from(&race_weekend(), 1, 0);
        assert!(race.is_limited_time);
        assert!(race.is_limited_session_laps);
        assert_eq!(race.session_laps, 25);
    }

    #[test]
    fn the_player_is_found_by_car_index() {
        let player = session_from(&race_weekend(), 1, 1)
            .player
            .expect("car 1 is in the field");
        assert_eq!(player.car_number, "7");
        assert_eq!(player.i_rating, 2250);
        assert_eq!(player.lic_level, 14);
    }

    #[test]
    fn a_car_index_that_is_not_in_the_field_has_no_driver() {
        assert!(session_from(&race_weekend(), 1, 99).player.is_none());
    }

    #[test]
    fn a_quoted_car_number_keeps_its_leading_zero() {
        // Numbers are quoted in the blob precisely so "07" stays "07".
        let info = yaml(
            r#"
DriverInfo:
  Drivers:
    - CarIdx: 0
      CarNumber: "07"
      IRating: 1500
"#,
        );
        let player = session_from(&info, 0, 0).player.expect("driver present");
        assert_eq!(player.car_number, "07");
    }

    #[test]
    fn an_unlimited_incident_allowance_reads_as_the_largest_value() {
        let info = yaml(
            r#"
WeekendInfo:
  WeekendOptions:
    IncidentLimit: unlimited
"#,
        );
        // The converter clamps this down to the figure the dashboard shows.
        assert_eq!(session_from(&info, 0, 0).incident_limit, i32::MAX);
    }

    #[test]
    fn a_bounded_incident_allowance_is_read_as_given() {
        assert_eq!(session_from(&race_weekend(), 1, 0).incident_limit, 17);
    }

    #[test]
    fn the_car_name_comes_from_the_players_own_entry() {
        assert_eq!(car_screen_name(&race_weekend(), 0), "FIA F4");
        assert_eq!(car_screen_name(&race_weekend(), 99), "");
    }

    #[test]
    fn strength_of_field_counts_only_competitors() {
        let info = yaml(
            r#"
DriverInfo:
  Drivers:
    - CarIdx: 0
      IRating: 3000
      CarIsPaceCar: 0
      IsSpectator: 0
    - CarIdx: 1
      IRating: 2000
      CarIsPaceCar: 0
      IsSpectator: 0
    - CarIdx: 2
      IRating: 0
      CarIsPaceCar: 1
      IsSpectator: 0
    - CarIdx: 3
      IRating: 9999
      CarIsPaceCar: 0
      IsSpectator: 1
"#,
        );
        // The pace car and the spectator would both drag the average off.
        assert_eq!(race_car_iratings(&info), vec![3000, 2000]);
    }

    #[test]
    fn a_blob_without_the_expected_sections_yields_defaults() {
        let empty = yaml("---\nSomethingElse: 1\n");
        let session = session_from(&empty, 0, 0);
        assert_eq!(session.session_type, "");
        assert!(session.player.is_none());
        assert!(race_car_iratings(&empty).is_empty());
        assert_eq!(car_screen_name(&empty, 0), "");
    }
}

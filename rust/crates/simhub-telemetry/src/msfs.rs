//! Reads Microsoft Flight Simulator 2020 telemetry over SimConnect.
//!
//! Ported from `HaddySimHub/Displays/Msfs/SimVarDefinitions.cs`,
//! `MsfsTelemetry.cs`, `SimConnectClient.cs` and the `Interop` folder.
//!
//! # Why hand-written FFI
//!
//! The SimConnect crates on crates.io do not fit. `simconnect` and FlyByWire's
//! `msfs` run bindgen against the MSFS SDK headers and link `SimConnect.lib`,
//! so they need the SDK installed at build time and bind the DLL statically;
//! `simconnect-sdk` is archived; `flybywireless-simconnect` speaks the named
//! pipe protocol instead of using the DLL (see
//! `docs/rust-telemetry-implementations.md`). Five entry points with a frozen
//! ABI — MSFS 2020 is superseded — are cheaper to own than any of those, and
//! loading the DLL at runtime keeps a machine without the sim working.
//!
//! # The layout contract
//!
//! `SimConnect_GetNextDispatch` hands back a `SIMCONNECT_RECV_SIMOBJECT_DATA`
//! header followed by one opaque data block.
//!
//! That block carries the simvars in exactly the order they were added to
//! the data definition, with no padding. [`DEFINITIONS`] *is* that order, and
//! [`decode`] walks it. Reorder one and not the other and every field after the
//! change reads a plausible but wrong value, with no error from the simulator —
//! which is why the tests below pin the order against the committed layout
//! manifest and each simvar against the field it lands in.
//!
//! The split mirrors the rest of this crate: [`decode`] and the dispatch
//! parsing in [`dispatch`] are pure and tested anywhere; [`MsfsSource`] is the
//! thin part that talks to the DLL.

pub mod dispatch;
mod library;
mod source;

pub use library::{LIBRARY_NAME, candidate_paths};
pub use source::{MsfsSource, SimConnectException};

use crate::read::f64_at;
use simhub_model::telemetry::MsfsTelemetry;

/// `SIMCONNECT_DATATYPE`, restricted to the members the definition uses.
///
/// The C# enum carried all of them and threw when sizing an unused one. Here
/// the unused ones simply do not exist, so a definition cannot name a type the
/// decoder has no width for.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
#[repr(u32)]
pub enum DataType {
    Float64 = 4,
    String32 = 6,
    String256 = 9,
}

impl DataType {
    /// Payload size of the type in the data block.
    pub const fn size(self) -> usize {
        match self {
            DataType::Float64 => 8,
            DataType::String32 => 32,
            DataType::String256 => 256,
        }
    }
}

/// One simulation variable in the data definition.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct SimVar {
    /// The simvar name, exactly as the SDK spells it.
    pub name: &'static str,
    /// The unit to convert into, or `None` for the string types, which are
    /// requested without one.
    pub unit: Option<&'static str>,
    /// How the sim should encode the value in the data block.
    pub data_type: DataType,
}

const fn value(name: &'static str, unit: &'static str) -> SimVar {
    SimVar {
        name,
        unit: Some(unit),
        data_type: DataType::Float64,
    }
}

const fn text(name: &'static str, data_type: DataType) -> SimVar {
    SimVar {
        name,
        unit: None,
        data_type,
    }
}

const BOOL: &str = "bool";
const DEGREES: &str = "degrees";
const FEET: &str = "feet";
const KNOTS: &str = "knots";
const METERS: &str = "meters";
const NUMBER: &str = "number";
const PERCENT: &str = "percent";
const POUNDS: &str = "pounds";
const SECONDS: &str = "seconds";

/// The variables subscribed to, in the order they are added to the definition.
///
/// **This order is the byte layout of the data block.** Every numeric value is
/// requested as `FLOAT64`, flags and enums included, because that keeps the
/// block uniformly eight-byte aligned; the strings sit at the end for the same
/// reason. Percentages are all requested as `percent` so they arrive on a 0-100
/// scale, even where the underlying variable is natively "percent over 100".
pub const DEFINITIONS: &[SimVar] = &[
    // Speed
    value("AIRSPEED INDICATED", KNOTS),
    value("AIRSPEED TRUE", KNOTS),
    value("GROUND VELOCITY", KNOTS),
    value("AIRSPEED MACH", "mach"),
    value("STALL WARNING", BOOL),
    value("OVERSPEED WARNING", BOOL),
    // Attitude
    value("PLANE PITCH DEGREES", "radians"),
    value("PLANE BANK DEGREES", "radians"),
    value("TURN COORDINATOR BALL", "position"),
    value("TURN INDICATOR RATE", "radians per second"),
    // Altitude
    value("INDICATED ALTITUDE", FEET),
    value("PLANE ALT ABOVE GROUND", FEET),
    value("VERTICAL SPEED", "feet per minute"),
    value("KOHLSMAN SETTING MB", "millibars"),
    value("SIM ON GROUND", BOOL),
    // Heading
    value("PLANE HEADING DEGREES MAGNETIC", DEGREES),
    value("PLANE HEADING DEGREES TRUE", DEGREES),
    value("GPS GROUND MAGNETIC TRACK", DEGREES),
    value("AMBIENT WIND DIRECTION", DEGREES),
    value("AMBIENT WIND VELOCITY", KNOTS),
    // Autopilot
    value("AUTOPILOT MASTER", BOOL),
    value("AUTOPILOT HEADING LOCK", BOOL),
    value("AUTOPILOT HEADING LOCK DIR", DEGREES),
    value("AUTOPILOT ALTITUDE LOCK", BOOL),
    value("AUTOPILOT ALTITUDE LOCK VAR", FEET),
    value("AUTOPILOT AIRSPEED HOLD", BOOL),
    value("AUTOPILOT AIRSPEED HOLD VAR", KNOTS),
    value("AUTOPILOT VERTICAL HOLD VAR", "feet per minute"),
    value("AUTOPILOT NAV1 LOCK", BOOL),
    value("AUTOPILOT APPROACH HOLD", BOOL),
    // Navigation
    value("GPS IS ACTIVE FLIGHT PLAN", BOOL),
    value("GPS WP DISTANCE", METERS),
    value("GPS WP ETE", SECONDS),
    value("GPS ETE", SECONDS),
    value("GPS ETA", SECONDS),
    value("GPS WP CROSS TRK", METERS),
    value("GPS IS APPROACH ACTIVE", BOOL),
    // Navigation radio, for the course deviation indicator
    value("NAV HAS NAV:1", BOOL),
    value("NAV HAS LOCALIZER:1", BOOL),
    value("NAV HAS GLIDE SLOPE:1", BOOL),
    value("NAV CDI:1", NUMBER),
    value("NAV GSI:1", NUMBER),
    value("NAV TOFROM:1", "enum"),
    value("NAV OBS:1", DEGREES),
    // Engine
    value("NUMBER OF ENGINES", NUMBER),
    value("ENGINE TYPE", "enum"),
    value("TURB ENG N1:1", PERCENT),
    value("GENERAL ENG PCT MAX RPM:1", PERCENT),
    value("GENERAL ENG RPM:1", "rpm"),
    value("ENG FUEL FLOW PPH:1", "pounds per hour"),
    value("GENERAL ENG OIL TEMPERATURE:1", "celsius"),
    value("GENERAL ENG OIL PRESSURE:1", "psi"),
    value("GENERAL ENG MANIFOLD PRESSURE:1", "inHg"),
    // Fuel
    value("FUEL TOTAL QUANTITY WEIGHT", POUNDS),
    value("FUEL TOTAL CAPACITY", "gallons"),
    value("FUEL WEIGHT PER GALLON", POUNDS),
    // Configuration
    value("FLAPS HANDLE INDEX", NUMBER),
    value("FLAPS NUM HANDLE POSITIONS", NUMBER),
    value("GEAR TOTAL PCT EXTENDED", PERCENT),
    value("GEAR HANDLE POSITION", BOOL),
    value("SPOILERS HANDLE POSITION", PERCENT),
    value("SPOILERS ARMED", BOOL),
    value("BRAKE PARKING POSITION", BOOL),
    value("ELEVATOR TRIM PCT", PERCENT),
    // Lights
    value("LIGHT LANDING", BOOL),
    value("LIGHT TAXI", BOOL),
    value("LIGHT STROBE", BOOL),
    value("LIGHT NAV", BOOL),
    value("LIGHT BEACON", BOOL),
    // Miscellaneous
    value("ZULU TIME", SECONDS),
    // Strings
    text("GPS WP NEXT ID", DataType::String32),
    text("GPS APPROACH AIRPORT ID", DataType::String32),
    text("NAV IDENT:1", DataType::String32),
    text("TITLE", DataType::String256),
];

const fn total_size(definitions: &[SimVar]) -> usize {
    let mut total = 0;
    let mut index = 0;
    while index < definitions.len() {
        total += definitions[index].data_type.size();
        index += 1;
    }
    total
}

/// Size of the data block the sim sends for [`DEFINITIONS`].
pub const BLOCK_SIZE: usize = total_size(DEFINITIONS);

/// Walks the data block in definition order.
///
/// Each read checks, in debug builds, that the definition it is consuming has
/// the type being read. That makes a decoder that drifts from [`DEFINITIONS`]
/// fail the tests rather than quietly read a string as a double.
struct Block<'a> {
    bytes: &'a [u8],
    offset: usize,
    index: usize,
}

impl<'a> Block<'a> {
    fn new(bytes: &'a [u8]) -> Self {
        Self {
            bytes,
            offset: 0,
            index: 0,
        }
    }

    fn next(&mut self, expected_float: bool) -> (usize, usize) {
        let definition = DEFINITIONS[self.index];
        debug_assert_eq!(
            definition.data_type == DataType::Float64,
            expected_float,
            "decode reads '{}' as the wrong type",
            definition.name
        );
        let offset = self.offset;
        let size = definition.data_type.size();
        self.offset += size;
        self.index += 1;
        (offset, size)
    }

    fn f64(&mut self) -> f64 {
        let (offset, _) = self.next(true);
        f64_at(self.bytes, offset)
    }

    fn text(&mut self) -> String {
        let (offset, size) = self.next(false);
        fixed_string(&self.bytes[offset..offset + size])
    }
}

/// Reads a fixed-width SimConnect string, stopping at the first NUL.
///
/// The width is padding, not text: the sim terminates the value and leaves
/// whatever follows undefined. The C# side marshalled these as ANSI. Text that
/// is valid UTF-8 is taken as such; anything else is read byte-per-character,
/// which is exact for ASCII identifiers and close for the Latin-1 range.
fn fixed_string(bytes: &[u8]) -> String {
    let end = bytes.iter().position(|b| *b == 0).unwrap_or(bytes.len());
    let text = &bytes[..end];
    match std::str::from_utf8(text) {
        Ok(text) => text.to_string(),
        Err(_) => text.iter().map(|b| char::from(*b)).collect(),
    }
}

/// Decodes one data block, or `None` when it is too short to be one.
///
/// `bytes` is the block alone — what follows the `SIMCONNECT_RECV_SIMOBJECT_DATA`
/// header. Use [`dispatch::parse`] to get it out of a whole dispatch message.
pub fn decode(bytes: &[u8]) -> Option<MsfsTelemetry> {
    if bytes.len() < BLOCK_SIZE {
        return None;
    }

    let mut b = Block::new(bytes);

    // Struct expressions evaluate their fields in the order written, so this
    // reads the block front to back. The order must stay that of DEFINITIONS.
    let telemetry = MsfsTelemetry {
        indicated_airspeed: b.f64(),
        true_airspeed: b.f64(),
        ground_speed: b.f64(),
        mach: b.f64(),
        stall_warning: b.f64(),
        overspeed_warning: b.f64(),

        pitch_radians: b.f64(),
        bank_radians: b.f64(),
        turn_coordinator_ball: b.f64(),
        turn_rate_radians_per_second: b.f64(),

        indicated_altitude: b.f64(),
        altitude_above_ground: b.f64(),
        vertical_speed: b.f64(),
        altimeter_setting_mb: b.f64(),
        on_ground: b.f64(),

        heading_magnetic: b.f64(),
        heading_true: b.f64(),
        ground_track: b.f64(),
        wind_direction: b.f64(),
        wind_velocity: b.f64(),

        autopilot_master: b.f64(),
        autopilot_heading_lock: b.f64(),
        autopilot_heading_bug: b.f64(),
        autopilot_altitude_lock: b.f64(),
        autopilot_altitude_target: b.f64(),
        autopilot_airspeed_hold: b.f64(),
        autopilot_airspeed_target: b.f64(),
        autopilot_vertical_speed_target: b.f64(),
        autopilot_nav_lock: b.f64(),
        autopilot_approach_hold: b.f64(),

        flight_plan_active: b.f64(),
        waypoint_distance_meters: b.f64(),
        waypoint_ete_seconds: b.f64(),
        destination_ete_seconds: b.f64(),
        destination_eta_seconds: b.f64(),
        cross_track_meters: b.f64(),
        approach_active: b.f64(),

        nav_has_signal: b.f64(),
        nav_has_localizer: b.f64(),
        nav_has_glide_slope: b.f64(),
        nav_cdi: b.f64(),
        nav_gsi: b.f64(),
        nav_to_from: b.f64(),
        nav_obs: b.f64(),

        engine_count: b.f64(),
        engine_type: b.f64(),
        turbine_n1_pct: b.f64(),
        piston_pct_max_rpm: b.f64(),
        engine_rpm: b.f64(),
        fuel_flow_pph: b.f64(),
        oil_temperature: b.f64(),
        oil_pressure: b.f64(),
        manifold_pressure: b.f64(),

        fuel_quantity_lbs: b.f64(),
        fuel_capacity_gallons: b.f64(),
        fuel_weight_per_gallon: b.f64(),

        flaps_handle_index: b.f64(),
        flaps_handle_positions: b.f64(),
        gear_percent_extended: b.f64(),
        gear_handle_down: b.f64(),
        spoilers_handle_pct: b.f64(),
        spoilers_armed: b.f64(),
        parking_brake_on: b.f64(),
        elevator_trim_pct: b.f64(),

        light_landing: b.f64(),
        light_taxi: b.f64(),
        light_strobe: b.f64(),
        light_nav: b.f64(),
        light_beacon: b.f64(),

        zulu_time_seconds: b.f64(),

        next_waypoint_id: b.text(),
        destination_id: b.text(),
        nav_ident: b.text(),
        aircraft_title: b.text(),
    };

    debug_assert_eq!(b.index, DEFINITIONS.len(), "decode skipped a simvar");
    debug_assert_eq!(b.offset, BLOCK_SIZE);
    Some(telemetry)
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::HashSet;

    /// Byte offset of each definition within the block.
    fn offsets() -> Vec<usize> {
        DEFINITIONS
            .iter()
            .scan(0, |offset, definition| {
                let start = *offset;
                *offset += definition.data_type.size();
                Some(start)
            })
            .collect()
    }

    fn offset_of(name: &str) -> usize {
        let index = DEFINITIONS
            .iter()
            .position(|definition| definition.name == name)
            .unwrap_or_else(|| panic!("no simvar named '{name}'"));
        offsets()[index]
    }

    type Field = fn(&MsfsTelemetry) -> f64;

    /// Which field each numeric simvar must land in.
    ///
    /// Written out by name rather than derived from position, so it is an
    /// independent statement of the mapping that `decode` has to agree with.
    const NUMERIC_FIELDS: &[(&str, Field)] = &[
        ("AIRSPEED INDICATED", |t| t.indicated_airspeed),
        ("AIRSPEED TRUE", |t| t.true_airspeed),
        ("GROUND VELOCITY", |t| t.ground_speed),
        ("AIRSPEED MACH", |t| t.mach),
        ("STALL WARNING", |t| t.stall_warning),
        ("OVERSPEED WARNING", |t| t.overspeed_warning),
        ("PLANE PITCH DEGREES", |t| t.pitch_radians),
        ("PLANE BANK DEGREES", |t| t.bank_radians),
        ("TURN COORDINATOR BALL", |t| t.turn_coordinator_ball),
        ("TURN INDICATOR RATE", |t| t.turn_rate_radians_per_second),
        ("INDICATED ALTITUDE", |t| t.indicated_altitude),
        ("PLANE ALT ABOVE GROUND", |t| t.altitude_above_ground),
        ("VERTICAL SPEED", |t| t.vertical_speed),
        ("KOHLSMAN SETTING MB", |t| t.altimeter_setting_mb),
        ("SIM ON GROUND", |t| t.on_ground),
        ("PLANE HEADING DEGREES MAGNETIC", |t| t.heading_magnetic),
        ("PLANE HEADING DEGREES TRUE", |t| t.heading_true),
        ("GPS GROUND MAGNETIC TRACK", |t| t.ground_track),
        ("AMBIENT WIND DIRECTION", |t| t.wind_direction),
        ("AMBIENT WIND VELOCITY", |t| t.wind_velocity),
        ("AUTOPILOT MASTER", |t| t.autopilot_master),
        ("AUTOPILOT HEADING LOCK", |t| t.autopilot_heading_lock),
        ("AUTOPILOT HEADING LOCK DIR", |t| t.autopilot_heading_bug),
        ("AUTOPILOT ALTITUDE LOCK", |t| t.autopilot_altitude_lock),
        ("AUTOPILOT ALTITUDE LOCK VAR", |t| {
            t.autopilot_altitude_target
        }),
        ("AUTOPILOT AIRSPEED HOLD", |t| t.autopilot_airspeed_hold),
        ("AUTOPILOT AIRSPEED HOLD VAR", |t| {
            t.autopilot_airspeed_target
        }),
        ("AUTOPILOT VERTICAL HOLD VAR", |t| {
            t.autopilot_vertical_speed_target
        }),
        ("AUTOPILOT NAV1 LOCK", |t| t.autopilot_nav_lock),
        ("AUTOPILOT APPROACH HOLD", |t| t.autopilot_approach_hold),
        ("GPS IS ACTIVE FLIGHT PLAN", |t| t.flight_plan_active),
        ("GPS WP DISTANCE", |t| t.waypoint_distance_meters),
        ("GPS WP ETE", |t| t.waypoint_ete_seconds),
        ("GPS ETE", |t| t.destination_ete_seconds),
        ("GPS ETA", |t| t.destination_eta_seconds),
        ("GPS WP CROSS TRK", |t| t.cross_track_meters),
        ("GPS IS APPROACH ACTIVE", |t| t.approach_active),
        ("NAV HAS NAV:1", |t| t.nav_has_signal),
        ("NAV HAS LOCALIZER:1", |t| t.nav_has_localizer),
        ("NAV HAS GLIDE SLOPE:1", |t| t.nav_has_glide_slope),
        ("NAV CDI:1", |t| t.nav_cdi),
        ("NAV GSI:1", |t| t.nav_gsi),
        ("NAV TOFROM:1", |t| t.nav_to_from),
        ("NAV OBS:1", |t| t.nav_obs),
        ("NUMBER OF ENGINES", |t| t.engine_count),
        ("ENGINE TYPE", |t| t.engine_type),
        ("TURB ENG N1:1", |t| t.turbine_n1_pct),
        ("GENERAL ENG PCT MAX RPM:1", |t| t.piston_pct_max_rpm),
        ("GENERAL ENG RPM:1", |t| t.engine_rpm),
        ("ENG FUEL FLOW PPH:1", |t| t.fuel_flow_pph),
        ("GENERAL ENG OIL TEMPERATURE:1", |t| t.oil_temperature),
        ("GENERAL ENG OIL PRESSURE:1", |t| t.oil_pressure),
        ("GENERAL ENG MANIFOLD PRESSURE:1", |t| t.manifold_pressure),
        ("FUEL TOTAL QUANTITY WEIGHT", |t| t.fuel_quantity_lbs),
        ("FUEL TOTAL CAPACITY", |t| t.fuel_capacity_gallons),
        ("FUEL WEIGHT PER GALLON", |t| t.fuel_weight_per_gallon),
        ("FLAPS HANDLE INDEX", |t| t.flaps_handle_index),
        ("FLAPS NUM HANDLE POSITIONS", |t| t.flaps_handle_positions),
        ("GEAR TOTAL PCT EXTENDED", |t| t.gear_percent_extended),
        ("GEAR HANDLE POSITION", |t| t.gear_handle_down),
        ("SPOILERS HANDLE POSITION", |t| t.spoilers_handle_pct),
        ("SPOILERS ARMED", |t| t.spoilers_armed),
        ("BRAKE PARKING POSITION", |t| t.parking_brake_on),
        ("ELEVATOR TRIM PCT", |t| t.elevator_trim_pct),
        ("LIGHT LANDING", |t| t.light_landing),
        ("LIGHT TAXI", |t| t.light_taxi),
        ("LIGHT STROBE", |t| t.light_strobe),
        ("LIGHT NAV", |t| t.light_nav),
        ("LIGHT BEACON", |t| t.light_beacon),
        ("ZULU TIME", |t| t.zulu_time_seconds),
    ];

    type TextField = fn(&MsfsTelemetry) -> &str;

    const TEXT_FIELDS: &[(&str, TextField)] = &[
        ("GPS WP NEXT ID", |t| &t.next_waypoint_id),
        ("GPS APPROACH AIRPORT ID", |t| &t.destination_id),
        ("NAV IDENT:1", |t| &t.nav_ident),
        ("TITLE", |t| &t.aircraft_title),
    ];

    fn put_f64(block: &mut [u8], offset: usize, value: f64) {
        block[offset..offset + 8].copy_from_slice(&value.to_le_bytes());
    }

    fn put_text(block: &mut [u8], offset: usize, value: &str) {
        block[offset..offset + value.len()].copy_from_slice(value.as_bytes());
    }

    #[test]
    fn the_block_is_seventy_doubles_then_three_short_strings_and_a_long_one() {
        assert_eq!(BLOCK_SIZE, 70 * 8 + 3 * 32 + 256);
        assert_eq!(BLOCK_SIZE, 912);
    }

    #[test]
    fn the_block_size_is_the_sum_of_the_definitions() {
        let sum: usize = DEFINITIONS.iter().map(|d| d.data_type.size()).sum();
        assert_eq!(BLOCK_SIZE, sum);
    }

    #[test]
    fn there_is_one_definition_per_telemetry_field() {
        assert_eq!(DEFINITIONS.len(), 74);
        assert_eq!(
            NUMERIC_FIELDS.len() + TEXT_FIELDS.len(),
            DEFINITIONS.len(),
            "every simvar needs a field to land in"
        );
    }

    #[test]
    fn numeric_definitions_all_carry_a_unit() {
        for definition in DEFINITIONS
            .iter()
            .filter(|d| d.data_type == DataType::Float64)
        {
            assert!(
                definition.unit.is_some_and(|unit| !unit.trim().is_empty()),
                "'{}' has no unit, so the simulator would pick one for us",
                definition.name
            );
        }
    }

    #[test]
    fn string_definitions_carry_no_unit() {
        for definition in DEFINITIONS
            .iter()
            .filter(|d| d.data_type != DataType::Float64)
        {
            assert_eq!(
                definition.unit, None,
                "'{}' is a string and must be requested without a unit",
                definition.name
            );
        }
    }

    #[test]
    fn strings_sit_after_every_double_so_the_block_stays_aligned() {
        let first_string = DEFINITIONS
            .iter()
            .position(|d| d.data_type != DataType::Float64)
            .expect("there are strings");
        assert!(
            DEFINITIONS[first_string..]
                .iter()
                .all(|d| d.data_type != DataType::Float64)
        );
    }

    #[test]
    fn definitions_have_no_duplicate_names() {
        let mut seen = HashSet::new();
        for definition in DEFINITIONS {
            assert!(
                seen.insert(definition.name),
                "duplicate simvar '{}'",
                definition.name
            );
        }
    }

    #[test]
    fn data_types_carry_the_sdk_numbering() {
        // These values go straight to SimConnect_AddToDataDefinition.
        assert_eq!(DataType::Float64 as u32, 4);
        assert_eq!(DataType::String32 as u32, 6);
        assert_eq!(DataType::String256 as u32, 9);
    }

    #[test]
    fn offsets_and_sizes_match_the_committed_layout_manifest() {
        // The manifest is generated from the C# struct the sim used to write
        // into, so agreeing with it means agreeing with the C# reader.
        let manifest = include_str!("../../../../fixtures/telemetry/manifest/msfs.json");
        let numbers = |key: &str| -> Vec<usize> {
            manifest
                .lines()
                .filter_map(|line| line.trim().strip_prefix(key))
                .map(|rest| rest.trim_end_matches(',').trim().parse().expect("number"))
                .collect()
        };
        let manifest_offsets = numbers("\"Offset\":");
        // The first "Size" is the struct's own; the rest are per field.
        let manifest_sizes = numbers("\"Size\":");

        assert_eq!(manifest_sizes[0], BLOCK_SIZE);
        assert_eq!(manifest_offsets, offsets());
        let sizes: Vec<usize> = DEFINITIONS.iter().map(|d| d.data_type.size()).collect();
        assert_eq!(manifest_sizes[1..], sizes[..]);
    }

    #[test]
    fn a_short_block_is_rejected() {
        assert!(decode(&[0u8; BLOCK_SIZE - 1]).is_none());
        assert!(decode(&[]).is_none());
    }

    #[test]
    fn a_zeroed_block_decodes_to_defaults() {
        assert_eq!(decode(&[0u8; BLOCK_SIZE]), Some(MsfsTelemetry::default()));
    }

    #[test]
    fn trailing_bytes_past_the_block_are_ignored() {
        let mut block = vec![0u8; BLOCK_SIZE + 16];
        put_f64(&mut block, 0, 123.0);
        block[BLOCK_SIZE..].fill(0xFF);
        assert_eq!(decode(&block).unwrap().indicated_airspeed, 123.0);
    }

    #[test]
    fn every_numeric_simvar_is_read_into_its_own_field() {
        // A distinct value in every slot, so a field read from a neighbour's
        // offset shows up as the wrong number rather than as a matching zero.
        let mut block = vec![0u8; BLOCK_SIZE];
        for (name, _) in NUMERIC_FIELDS {
            let offset = offset_of(name);
            put_f64(&mut block, offset, offset as f64 + 0.5);
        }

        let decoded = decode(&block).expect("a full block decodes");
        for (name, field) in NUMERIC_FIELDS {
            assert_eq!(
                field(&decoded),
                offset_of(name) as f64 + 0.5,
                "'{name}' landed in the wrong field"
            );
        }
    }

    #[test]
    fn every_string_simvar_is_read_into_its_own_field() {
        let mut block = vec![0u8; BLOCK_SIZE];
        for (name, _) in TEXT_FIELDS {
            put_text(&mut block, offset_of(name), name);
        }

        let decoded = decode(&block).expect("a full block decodes");
        for (name, field) in TEXT_FIELDS {
            assert_eq!(field(&decoded), *name, "'{name}' landed in the wrong field");
        }
    }

    #[test]
    fn strings_stop_at_the_first_nul_and_ignore_the_padding_after_it() {
        let mut block = vec![0u8; BLOCK_SIZE];
        let ident = offset_of("NAV IDENT:1");
        put_text(&mut block, ident, "IAMS\0garbage");
        // The tail of the 32-byte slot is undefined; it must not leak through.
        block[ident + 20..ident + 32].fill(b'X');

        let decoded = decode(&block).unwrap();
        assert_eq!(decoded.nav_ident, "IAMS");
    }

    #[test]
    fn a_string_that_fills_its_whole_slot_is_kept_whole() {
        let mut block = vec![0u8; BLOCK_SIZE];
        let next = offset_of("GPS WP NEXT ID");
        let full = "A".repeat(32);
        put_text(&mut block, next, &full);
        // The neighbouring slot starts straight after, with no terminator between.
        put_text(&mut block, next + 32, "EHAM");

        let decoded = decode(&block).unwrap();
        assert_eq!(decoded.next_waypoint_id, full);
        assert_eq!(decoded.destination_id, "EHAM");
    }

    #[test]
    fn the_aircraft_title_uses_its_full_256_byte_slot() {
        let mut block = vec![0u8; BLOCK_SIZE];
        let title = offset_of("TITLE");
        assert_eq!(title + 256, BLOCK_SIZE);
        let long = "Cessna Skyhawk ".repeat(17);
        assert_eq!(long.len(), 255);
        put_text(&mut block, title, &long);

        assert_eq!(decode(&block).unwrap().aircraft_title, long);
    }

    #[test]
    fn non_utf8_text_falls_back_to_one_character_per_byte() {
        let mut block = vec![0u8; BLOCK_SIZE];
        let title = offset_of("TITLE");
        // "Zürich" in Windows-1252, which is not valid UTF-8.
        block[title..title + 6].copy_from_slice(&[b'Z', 0xFC, b'r', b'i', b'c', b'h']);

        assert_eq!(decode(&block).unwrap().aircraft_title, "Zürich");
    }
}

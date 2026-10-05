//! Ported from `HaddySimHub/Displays/Msfs/MsfsDataConverter.cs`.
//!
//! This is where the sim's conventions are translated into the dashboard's:
//! radians to degrees, metres to nautical miles, gallons to pounds, and the
//! sim's sign conventions for pitch and bank to the ones a display expects.

use simhub_model::{
    CourseDeviationSource, DisplayUpdate, EngineType, FlightData, NavToFrom,
    telemetry::MsfsTelemetry,
};

const METRES_PER_NAUTICAL_MILE: f64 = 1852.0;
const SECONDS_PER_DAY: i32 = 86_400;

/// Full-scale deflection of NAV CDI.
const NAV_CDI_FULL_SCALE: f64 = 127.0;
/// Full-scale deflection of NAV GSI.
const NAV_GSI_FULL_SCALE: f64 = 119.0;
/// Full-scale deflection of the flight plan course scale while en route.
const GPS_EN_ROUTE_FULL_SCALE_NM: f64 = 2.0;
/// And once an approach is being flown, where the same needle has to mean far less.
const GPS_APPROACH_FULL_SCALE_NM: f64 = 0.3;

/// Every flag arrives as a double, so anything off zero counts as set.
///
/// The C# original compared against `double.Epsilon`, the smallest denormal,
/// which is a roundabout way of writing "not zero". Rust's `f64::EPSILON` is a
/// different and far larger number, so this is a plain comparison instead.
fn is_set(value: f64) -> bool {
    value != 0.0
}

fn to_degrees(radians: f64) -> f64 {
    radians * 180.0 / std::f64::consts::PI
}

fn to_nautical_miles(metres: f64) -> f64 {
    metres / METRES_PER_NAUTICAL_MILE
}

/// Wraps a bearing into 0-360, since the sim can report slightly outside it.
fn normalize(degrees: f64) -> f64 {
    ((degrees % 360.0) + 360.0) % 360.0
}

fn to_seconds(value: f64) -> Option<i32> {
    (value > 0.0).then(|| value.round() as i32)
}

/// Folds an absolute time into a time of day, which is how the dashboard shows it.
fn to_time_of_day(seconds: f64) -> Option<i32> {
    (seconds > 0.0).then(|| (seconds.round() as i32) % SECONDS_PER_DAY)
}

fn trimmed(value: &str) -> Option<String> {
    let trimmed = value.trim();
    (!trimmed.is_empty()).then(|| trimmed.to_string())
}

/// The SDK has no total-distance-to-destination variable, so it is derived from
/// the time en route the sim does report and the current ground speed — the same
/// arithmetic the sim used to produce that estimate.
fn destination_distance_nm(source: &MsfsTelemetry) -> Option<f32> {
    if source.destination_ete_seconds <= 0.0 || source.ground_speed <= 0.0 {
        return None;
    }
    Some((source.ground_speed * source.destination_ete_seconds / 3600.0) as f32)
}

fn endurance_seconds(source: &MsfsTelemetry) -> Option<i32> {
    if source.fuel_flow_pph <= 0.0 {
        return None;
    }
    Some((source.fuel_quantity_lbs / source.fuel_flow_pph * 3600.0).round() as i32)
}

fn to_nav_to_from(value: f64) -> NavToFrom {
    match value.round() as i32 {
        1 => NavToFrom::To,
        2 => NavToFrom::From,
        _ => NavToFrom::Off,
    }
}

/// [`EngineType`] mirrors the simvar's own numbering, so anything the sim
/// reports outside that range maps to the SDK's own "unsupported".
fn to_engine_type(value: f64) -> EngineType {
    match value.round() as i32 {
        0 => EngineType::Piston,
        1 => EngineType::Jet,
        2 => EngineType::None,
        3 => EngineType::HeloBellTurbine,
        5 => EngineType::Turboprop,
        _ => EngineType::Unsupported,
    }
}

/// What the course deviation indicator should show, before it is copied onto the DTO.
#[derive(Debug, Default)]
struct CourseDeviation {
    source: CourseDeviationSource,
    source_id: Option<String>,
    selected_course: Option<f32>,
    lateral: Option<f32>,
    full_scale_nm: Option<f32>,
    glideslope: Option<f32>,
    to_from: NavToFrom,
}

/// Works out what the course deviation indicator should show.
///
/// The navigation radio takes precedence over the flight plan whenever it is
/// receiving a station: that is what a pilot flying an approach expects, and it
/// is how a real CDI behaves once the source is switched to the radio.
///
/// **Sign conventions are an assumption to verify against the simulator.**
/// Positive is taken to mean the aircraft is right of course and above the
/// glidepath, matching `cross_track_error_nm`. If a needle turns out to be
/// mirrored, it is one negation here, and the tests pin the convention so the
/// change is a deliberate one.
fn resolve_deviation(source: &MsfsTelemetry, has_flight_plan: bool) -> CourseDeviation {
    if is_set(source.nav_has_signal) {
        let is_localizer = is_set(source.nav_has_localizer);

        return CourseDeviation {
            source: if is_localizer {
                CourseDeviationSource::Localizer
            } else {
                CourseDeviationSource::Vor
            },
            source_id: trimmed(&source.nav_ident),
            selected_course: Some(normalize(source.nav_obs) as f32),
            lateral: Some((source.nav_cdi / NAV_CDI_FULL_SCALE).clamp(-1.0, 1.0) as f32),
            // A radio course narrows as the station is approached, so there is
            // no fixed distance that full scale corresponds to.
            full_scale_nm: None,
            glideslope: is_set(source.nav_has_glide_slope)
                .then(|| (source.nav_gsi / NAV_GSI_FULL_SCALE).clamp(-1.0, 1.0) as f32),
            // A localizer has no radial, so the flag has nothing to report.
            to_from: if is_localizer {
                NavToFrom::Off
            } else {
                to_nav_to_from(source.nav_to_from)
            },
        };
    }

    if has_flight_plan {
        let full_scale_nm = if is_set(source.approach_active) {
            GPS_APPROACH_FULL_SCALE_NM
        } else {
            GPS_EN_ROUTE_FULL_SCALE_NM
        };

        return CourseDeviation {
            source: CourseDeviationSource::Gps,
            source_id: trimmed(&source.next_waypoint_id),
            // The flight plan's desired track is not subscribed to, so the
            // course is left for the navigation panel's leg information.
            selected_course: None,
            lateral: Some(
                (to_nautical_miles(source.cross_track_meters) / full_scale_nm).clamp(-1.0, 1.0)
                    as f32,
            ),
            full_scale_nm: Some(full_scale_nm as f32),
            glideslope: None,
            to_from: NavToFrom::Off,
        };
    }

    CourseDeviation {
        source: CourseDeviationSource::None,
        to_from: NavToFrom::Off,
        ..Default::default()
    }
}

pub fn convert(source: &MsfsTelemetry) -> DisplayUpdate {
    let engine_type = to_engine_type(source.engine_type);
    let engine_count = source.engine_count as i32;
    let has_engine = engine_type != EngineType::None
        && engine_type != EngineType::Unsupported
        && engine_count > 0;
    let is_piston = engine_type == EngineType::Piston;
    let has_flight_plan = is_set(source.flight_plan_active);
    let deviation = resolve_deviation(source, has_flight_plan);

    DisplayUpdate::Flight(FlightData {
        indicated_airspeed: source.indicated_airspeed as f32,
        true_airspeed: source.true_airspeed as f32,
        ground_speed: source.ground_speed as f32,
        mach_number: source.mach as f32,
        stall_warning: is_set(source.stall_warning),
        overspeed_warning: is_set(source.overspeed_warning),

        // The sim reports both angles in radians despite the "DEGREES" in their
        // names, and signs them the opposite way round from how an instrument
        // reads: positive is nose down and wing down on the left.
        pitch_degrees: to_degrees(-source.pitch_radians) as f32,
        bank_degrees: to_degrees(-source.bank_radians) as f32,
        slip_ball: (source.turn_coordinator_ball / 127.0).clamp(-1.0, 1.0) as f32,
        turn_rate: to_degrees(source.turn_rate_radians_per_second) as f32,

        indicated_altitude: source.indicated_altitude as f32,
        altitude_above_ground: source.altitude_above_ground as f32,
        vertical_speed: source.vertical_speed as f32,
        altimeter_setting_hpa: source.altimeter_setting_mb as f32,
        on_ground: is_set(source.on_ground),

        heading_magnetic: normalize(source.heading_magnetic) as f32,
        heading_true: normalize(source.heading_true) as f32,
        ground_track: normalize(source.ground_track) as f32,
        wind_direction: normalize(source.wind_direction) as f32,
        wind_speed: source.wind_velocity as f32,

        autopilot_master: is_set(source.autopilot_master),
        heading_hold: is_set(source.autopilot_heading_lock),
        heading_bug: normalize(source.autopilot_heading_bug) as f32,
        altitude_hold: is_set(source.autopilot_altitude_lock),
        altitude_target: source.autopilot_altitude_target as f32,
        speed_hold: is_set(source.autopilot_airspeed_hold),
        speed_target: source.autopilot_airspeed_target as f32,
        vertical_speed_target: source.autopilot_vertical_speed_target as f32,
        nav_hold: is_set(source.autopilot_nav_lock),
        approach_hold: is_set(source.autopilot_approach_hold),

        // Without a flight plan the sim leaves these at zero, which would read
        // as "next waypoint 0 NM away" rather than "nothing planned".
        has_active_flight_plan: has_flight_plan,
        next_waypoint_id: has_flight_plan
            .then(|| trimmed(&source.next_waypoint_id))
            .flatten(),
        distance_to_waypoint_nm: has_flight_plan
            .then(|| to_nautical_miles(source.waypoint_distance_meters) as f32),
        waypoint_ete_seconds: has_flight_plan
            .then(|| to_seconds(source.waypoint_ete_seconds))
            .flatten(),
        destination_id: has_flight_plan
            .then(|| trimmed(&source.destination_id))
            .flatten(),
        distance_to_destination_nm: has_flight_plan
            .then(|| destination_distance_nm(source))
            .flatten(),
        destination_ete_seconds: has_flight_plan
            .then(|| to_seconds(source.destination_ete_seconds))
            .flatten(),
        destination_eta_utc_seconds: has_flight_plan
            .then(|| to_time_of_day(source.destination_eta_seconds))
            .flatten(),
        cross_track_error_nm: has_flight_plan
            .then(|| to_nautical_miles(source.cross_track_meters) as f32),

        deviation_source: deviation.source,
        deviation_source_id: deviation.source_id,
        selected_course: deviation.selected_course,
        lateral_deviation: deviation.lateral,
        lateral_full_scale_nm: deviation.full_scale_nm,
        glideslope_deviation: deviation.glideslope,
        to_from: deviation.to_from,

        engine_count,
        engine_type,
        engine_primary_pct: has_engine.then(|| {
            if is_piston {
                source.piston_pct_max_rpm as f32
            } else {
                source.turbine_n1_pct as f32
            }
        }),
        // A jet has no propeller or crankshaft speed a pilot reads; everything else does.
        engine_rpm: (has_engine && engine_type != EngineType::Jet)
            .then(|| source.engine_rpm.round() as i32),
        fuel_flow_pph: has_engine.then(|| source.fuel_flow_pph as f32),
        oil_temperature: has_engine.then(|| source.oil_temperature as f32),
        oil_pressure: has_engine.then(|| source.oil_pressure as f32),
        manifold_pressure: is_piston.then(|| source.manifold_pressure as f32),

        fuel_quantity_lbs: source.fuel_quantity_lbs as f32,
        fuel_capacity_lbs: (source.fuel_capacity_gallons * source.fuel_weight_per_gallon) as f32,
        fuel_endurance_seconds: endurance_seconds(source),

        flaps_handle_index: source.flaps_handle_index as i32,
        flaps_handle_positions: source.flaps_handle_positions as i32,
        gear_percent_extended: source.gear_percent_extended.clamp(0.0, 100.0) as f32,
        gear_handle_down: is_set(source.gear_handle_down),
        spoilers_pct: source.spoilers_handle_pct.clamp(0.0, 100.0) as f32,
        spoilers_armed: is_set(source.spoilers_armed),
        parking_brake_on: is_set(source.parking_brake_on),
        elevator_trim_pct: source.elevator_trim_pct as f32,

        landing_lights_on: is_set(source.light_landing),
        taxi_lights_on: is_set(source.light_taxi),
        strobe_lights_on: is_set(source.light_strobe),
        nav_lights_on: is_set(source.light_nav),
        beacon_on: is_set(source.light_beacon),

        aircraft_title: trimmed(&source.aircraft_title).unwrap_or_default(),
        sim_time_utc_seconds: to_time_of_day(source.zulu_time_seconds).unwrap_or(0),
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    use simhub_model::DisplayType;

    fn flight(source: &MsfsTelemetry) -> FlightData {
        match convert(source) {
            DisplayUpdate::Flight(data) => data,
            other => panic!("expected a flight dashboard, got {other:?}"),
        }
    }

    /// An aircraft with a running piston engine, so the engine block is populated.
    fn piston() -> MsfsTelemetry {
        MsfsTelemetry {
            engine_count: 1.0,
            engine_type: 0.0,
            ..Default::default()
        }
    }

    #[test]
    fn convert_returns_a_flight_dashboard() {
        assert_eq!(
            convert(&MsfsTelemetry::default()).display_type(),
            DisplayType::FlightDashboard
        );
    }

    #[test]
    fn any_non_zero_flag_counts_as_set() {
        assert!(!is_set(0.0));
        assert!(is_set(1.0));
        // The C# original compared against the smallest denormal, so even a
        // vanishingly small reading was "set". A machine-epsilon comparison
        // would have changed that.
        assert!(is_set(1e-300));
        assert!(is_set(-1.0));
    }

    #[test]
    fn attitude_is_converted_to_degrees_and_resigned() {
        let data = flight(&MsfsTelemetry {
            pitch_radians: std::f64::consts::FRAC_PI_6,
            bank_radians: -std::f64::consts::FRAC_PI_4,
            ..Default::default()
        });
        assert!((data.pitch_degrees - -30.0).abs() < 0.001);
        assert!((data.bank_degrees - 45.0).abs() < 0.001);
    }

    #[test]
    fn bearings_are_wrapped_into_a_full_circle() {
        assert!((normalize(370.0) - 10.0).abs() < 0.001);
        assert!((normalize(-10.0) - 350.0).abs() < 0.001);
        assert!((normalize(0.0)).abs() < 0.001);
    }

    #[test]
    fn the_slip_ball_is_clamped_to_its_travel() {
        let hard_left = flight(&MsfsTelemetry {
            turn_coordinator_ball: -500.0,
            ..Default::default()
        });
        assert_eq!(hard_left.slip_ball, -1.0);

        let centred = flight(&MsfsTelemetry::default());
        assert_eq!(centred.slip_ball, 0.0);
    }

    #[test]
    fn flight_plan_fields_are_absent_without_a_plan() {
        let data = flight(&MsfsTelemetry {
            // The sim leaves stale values here when no plan is loaded.
            waypoint_distance_meters: 9_260.0,
            next_waypoint_id: "KSEA".to_string(),
            ..Default::default()
        });
        assert!(!data.has_active_flight_plan);
        assert_eq!(data.next_waypoint_id, None);
        assert_eq!(data.distance_to_waypoint_nm, None);
        assert_eq!(data.cross_track_error_nm, None);
    }

    #[test]
    fn flight_plan_distances_are_nautical_miles() {
        let data = flight(&MsfsTelemetry {
            flight_plan_active: 1.0,
            waypoint_distance_meters: 18_520.0,
            next_waypoint_id: "  KSEA  ".to_string(),
            ..Default::default()
        });
        assert!(data.has_active_flight_plan);
        assert_eq!(data.distance_to_waypoint_nm, Some(10.0));
        // Identifiers arrive space-padded from the fixed-width simvar.
        assert_eq!(data.next_waypoint_id.as_deref(), Some("KSEA"));
    }

    #[test]
    fn the_radio_takes_precedence_over_the_flight_plan() {
        let data = flight(&MsfsTelemetry {
            flight_plan_active: 1.0,
            nav_has_signal: 1.0,
            nav_ident: "SEA".to_string(),
            nav_obs: 160.0,
            nav_cdi: 127.0,
            ..Default::default()
        });
        assert_eq!(data.deviation_source, CourseDeviationSource::Vor);
        assert_eq!(data.deviation_source_id.as_deref(), Some("SEA"));
        assert_eq!(data.selected_course, Some(160.0));
        assert_eq!(data.lateral_deviation, Some(1.0));
        // A radio course narrows towards the station, so full scale has no distance.
        assert_eq!(data.lateral_full_scale_nm, None);
    }

    #[test]
    fn a_localizer_reports_no_radial_and_may_carry_a_glideslope() {
        let data = flight(&MsfsTelemetry {
            nav_has_signal: 1.0,
            nav_has_localizer: 1.0,
            nav_has_glide_slope: 1.0,
            nav_gsi: 59.5,
            nav_to_from: 1.0,
            ..Default::default()
        });
        assert_eq!(data.deviation_source, CourseDeviationSource::Localizer);
        assert_eq!(data.glideslope_deviation, Some(0.5));
        // The to/from flag is meaningless on a localizer.
        assert_eq!(data.to_from, NavToFrom::Off);
    }

    #[test]
    fn a_vor_reports_its_to_from_flag() {
        let data = flight(&MsfsTelemetry {
            nav_has_signal: 1.0,
            nav_to_from: 2.0,
            ..Default::default()
        });
        assert_eq!(data.to_from, NavToFrom::From);
        // No glideslope on a plain VOR.
        assert_eq!(data.glideslope_deviation, None);
    }

    #[test]
    fn the_flight_plan_scale_tightens_on_an_approach() {
        let en_route = flight(&MsfsTelemetry {
            flight_plan_active: 1.0,
            cross_track_meters: 1852.0,
            ..Default::default()
        });
        assert_eq!(en_route.lateral_full_scale_nm, Some(2.0));
        assert_eq!(en_route.lateral_deviation, Some(0.5));

        let approach = flight(&MsfsTelemetry {
            flight_plan_active: 1.0,
            approach_active: 1.0,
            cross_track_meters: 1852.0,
            ..Default::default()
        });
        assert_eq!(approach.lateral_full_scale_nm, Some(0.3));
        // The same error now pegs the needle.
        assert_eq!(approach.lateral_deviation, Some(1.0));
    }

    #[test]
    fn no_signal_and_no_plan_leaves_the_needle_blank() {
        let data = flight(&MsfsTelemetry::default());
        assert_eq!(data.deviation_source, CourseDeviationSource::None);
        assert_eq!(data.lateral_deviation, None);
        assert_eq!(data.to_from, NavToFrom::Off);
    }

    #[test]
    fn engine_types_mirror_the_simvar_numbering() {
        assert_eq!(to_engine_type(0.0), EngineType::Piston);
        assert_eq!(to_engine_type(1.0), EngineType::Jet);
        assert_eq!(to_engine_type(2.0), EngineType::None);
        assert_eq!(to_engine_type(3.0), EngineType::HeloBellTurbine);
        assert_eq!(to_engine_type(5.0), EngineType::Turboprop);
        // 4 is the SDK's own "unsupported", and so is anything out of range.
        assert_eq!(to_engine_type(4.0), EngineType::Unsupported);
        assert_eq!(to_engine_type(99.0), EngineType::Unsupported);
    }

    #[test]
    fn a_jet_reports_no_crankshaft_speed() {
        let data = flight(&MsfsTelemetry {
            engine_count: 2.0,
            engine_type: 1.0,
            engine_rpm: 2400.0,
            turbine_n1_pct: 88.5,
            ..Default::default()
        });
        assert_eq!(data.engine_rpm, None);
        assert_eq!(data.engine_primary_pct, Some(88.5));
        // Manifold pressure is a piston instrument.
        assert_eq!(data.manifold_pressure, None);
    }

    #[test]
    fn a_piston_reports_rpm_and_manifold_pressure() {
        let data = flight(&MsfsTelemetry {
            engine_rpm: 2399.6,
            piston_pct_max_rpm: 75.0,
            manifold_pressure: 24.5,
            ..piston()
        });
        assert_eq!(data.engine_rpm, Some(2400));
        assert_eq!(data.engine_primary_pct, Some(75.0));
        assert_eq!(data.manifold_pressure, Some(24.5));
    }

    #[test]
    fn an_aircraft_without_engines_reports_no_engine_readings() {
        let data = flight(&MsfsTelemetry {
            engine_count: 0.0,
            engine_type: 2.0,
            fuel_flow_pph: 30.0,
            oil_pressure: 55.0,
            ..Default::default()
        });
        assert_eq!(data.engine_primary_pct, None);
        assert_eq!(data.engine_rpm, None);
        assert_eq!(data.fuel_flow_pph, None);
        assert_eq!(data.oil_pressure, None);
    }

    #[test]
    fn fuel_capacity_is_weighed_from_gallons() {
        let data = flight(&MsfsTelemetry {
            fuel_capacity_gallons: 50.0,
            fuel_weight_per_gallon: 6.0,
            ..Default::default()
        });
        assert_eq!(data.fuel_capacity_lbs, 300.0);
    }

    #[test]
    fn endurance_needs_a_fuel_flow_to_divide_by() {
        assert_eq!(endurance_seconds(&MsfsTelemetry::default()), None);

        let data = flight(&MsfsTelemetry {
            fuel_quantity_lbs: 60.0,
            fuel_flow_pph: 30.0,
            ..Default::default()
        });
        // Two hours of fuel.
        assert_eq!(data.fuel_endurance_seconds, Some(7200));
    }

    #[test]
    fn distance_to_destination_is_derived_from_time_and_ground_speed() {
        assert_eq!(destination_distance_nm(&MsfsTelemetry::default()), None);

        let data = flight(&MsfsTelemetry {
            flight_plan_active: 1.0,
            ground_speed: 120.0,
            destination_ete_seconds: 1800.0,
            ..Default::default()
        });
        // 120 knots for half an hour.
        assert_eq!(data.distance_to_destination_nm, Some(60.0));
    }

    #[test]
    fn times_fold_into_a_day() {
        assert_eq!(to_time_of_day(0.0), None);
        assert_eq!(to_time_of_day(-5.0), None);
        assert_eq!(to_time_of_day(3661.4), Some(3661));
        // Past midnight it wraps rather than growing without bound.
        assert_eq!(to_time_of_day(90_000.0), Some(3600));
    }

    #[test]
    fn control_surfaces_are_clamped_to_their_travel() {
        let data = flight(&MsfsTelemetry {
            gear_percent_extended: 140.0,
            spoilers_handle_pct: -20.0,
            ..Default::default()
        });
        assert_eq!(data.gear_percent_extended, 100.0);
        assert_eq!(data.spoilers_pct, 0.0);
    }

    #[test]
    fn a_blank_aircraft_title_becomes_an_empty_string() {
        let data = flight(&MsfsTelemetry {
            aircraft_title: "   ".to_string(),
            ..Default::default()
        });
        assert_eq!(data.aircraft_title, "");
    }
}

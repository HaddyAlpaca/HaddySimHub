//! Lives in `tests/` rather than in `src/lib.rs` because Windows refuses to
//! start an unelevated exe whose name contains "update" (installer detection,
//! error 740), and the unit test exe would be named after this crate.

use simhub_update::*;
use std::ffi::OsString;
use std::path::Path;
fn asset(name: &str) -> ReleaseAsset {
    ReleaseAsset::new(name, format!("https://example.invalid/{name}"))
}

fn release(version: &str, assets: &[&str]) -> Release {
    Release::builder()
        .version(version)
        .assets(assets.iter().map(|name| asset(name)))
        .build()
        .unwrap()
}

#[test]
fn a_release_tag_loses_its_leading_v_for_the_comparison() {
    assert_eq!(normalise_version("v0.1.42"), "0.1.42");
    assert_eq!(normalise_version("V0.1.42"), "0.1.42");
}

#[test]
fn a_bare_version_and_surrounding_whitespace_are_accepted() {
    assert_eq!(normalise_version("0.1.42"), "0.1.42");
    assert_eq!(normalise_version(" v0.1.42\n"), "0.1.42");
}

#[test]
fn the_tag_to_install_gets_back_its_leading_v() {
    assert_eq!(release_tag("0.1.42"), "v0.1.42");
    assert_eq!(release_tag("v0.1.42"), "v0.1.42");
}

#[test]
fn the_current_version_is_semver_once_normalised() {
    let version = normalise_version(current_version());
    let parts: Vec<&str> = version.split('.').collect();
    assert_eq!(parts.len(), 3, "{version}");
    assert!(
        parts.iter().all(|part| part.parse::<u64>().is_ok()),
        "{version}"
    );
}

#[test]
fn the_zip_is_picked_by_its_exact_name() {
    let assets = [asset("notes.txt"), asset(ASSET_NAME), asset("other.zip")];
    assert_eq!(select_asset(&assets).unwrap().name(), ASSET_NAME);
}

#[test]
fn a_similarly_named_asset_is_not_mistaken_for_the_zip() {
    let assets = [asset("haddy-simhub.zip.sha256"), asset("HADDY-SIMHUB.ZIP")];
    assert!(select_asset(&assets).is_none());
}

#[test]
fn a_newer_release_with_the_zip_is_installable() {
    let found = installable(Some(release("0.1.43", &[ASSET_NAME])));
    assert_eq!(found.unwrap().version(), "0.1.43");
}

#[test]
fn a_newer_release_without_the_zip_is_skipped() {
    assert!(installable(Some(release("0.1.43", &["source.tar.gz"]))).is_none());
}

#[test]
fn no_newer_release_means_no_update() {
    assert!(installable(None).is_none());
}

#[test]
fn the_restart_keeps_the_arguments_and_skips_the_next_check() {
    let args = restart_args(["race".into()]);
    assert_eq!(
        args,
        vec![OsString::from("race"), OsString::from(NO_UPDATE_ARG)]
    );
}

#[test]
fn the_restart_does_not_repeat_the_skip_flag() {
    let args = restart_args([NO_UPDATE_ARG.into(), "race".into()]);
    assert_eq!(
        args,
        vec![OsString::from(NO_UPDATE_ARG), OsString::from("race")]
    );
}

#[test]
fn another_process_of_the_same_exe_is_another_instance() {
    let exe = Path::new(r"C:\Apps\HaddySimHub\HaddySimHub.exe");
    let other = Path::new(r"c:\apps\haddysimhub\HADDYSIMHUB.EXE");
    assert!(is_other_instance(
        Pid::from(2),
        Some(other),
        Pid::from(1),
        exe
    ));
}

#[test]
fn this_process_is_not_another_instance() {
    let exe = Path::new(r"C:\Apps\HaddySimHub\HaddySimHub.exe");
    assert!(!is_other_instance(
        Pid::from(1),
        Some(exe),
        Pid::from(1),
        exe
    ));
}

#[test]
fn a_copy_installed_elsewhere_is_left_alone() {
    let exe = Path::new(r"C:\Apps\HaddySimHub\HaddySimHub.exe");
    let elsewhere = Path::new(r"D:\Old\HaddySimHub.exe");
    assert!(!is_other_instance(
        Pid::from(2),
        Some(elsewhere),
        Pid::from(1),
        exe
    ));
}

#[test]
fn a_process_whose_path_cannot_be_read_is_left_alone() {
    let exe = Path::new(r"C:\Apps\HaddySimHub\HaddySimHub.exe");
    assert!(!is_other_instance(Pid::from(2), None, Pid::from(1), exe));
}

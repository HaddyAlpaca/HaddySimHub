//! Keeps HaddySimHub up to date from its GitHub releases, and makes sure only
//! one copy of it runs.
//!
//! The C# app needed a separate `HaddySimHubUpdater.exe` because a running
//! .NET app cannot overwrite its own files. A Rust build is a single exe, and
//! [`self_update`] can swap a running exe in place, so the check, download,
//! unzip, replace and restart all happen inside the app itself.
//!
//! What the app calls at startup:
//!
//! ```no_run
//! simhub_update::stop_other_instances();
//! let skip = std::env::args().any(|arg| arg == simhub_update::NO_UPDATE_ARG);
//! if !skip {
//!     // Logs and swallows every failure; only returns when there is
//!     // nothing to install or the install failed.
//!     simhub_update::update_at_startup();
//! }
//! ```
//!
//! The pieces it is made of, for callers that want to drive it themselves:
//!
//! - [`current_version`] is the version baked in at build time.
//! - [`check_for_update`]`(current) -> Result<Option<Release>>` asks GitHub
//!   for a newer release that carries the `haddy-simhub.zip` asset.
//! - [`apply_update`]`(&release) -> Result<()>` downloads that release and
//!   replaces the running exe with the `HaddySimHub.exe` inside it.
//! - [`restart`]`() -> Result<Infallible>` starts the new exe and exits.
//! - [`stop_other_instances`]`()` stops other copies of this exe.
//!
//! # The release contract
//!
//! CD publishes each build as a GitHub release tagged `v0.1.<run number>`
//! with a single `haddy-simhub.zip` asset that holds `HaddySimHub.exe` at its
//! root. The C# app recorded the installed tag in `version.txt`; this one gets
//! it from `HADDYSIMHUB_VERSION` at compile time instead, so CD must build
//! with that variable set to the release tag.

use std::convert::Infallible;
use std::ffi::OsString;
use std::path::Path;
use std::time::{Duration, Instant};

use self_update::backends::github;
use sysinfo::{ProcessRefreshKind, ProcessesToUpdate, System, UpdateKind};

pub use self_update::errors::Error;
pub use self_update::{Release, ReleaseAsset};
pub use sysinfo::Pid;

pub type Result<T> = std::result::Result<T, Error>;

pub const REPO_OWNER: &str = "HaddyAlpaca";
pub const REPO_NAME: &str = "HaddySimHub";

/// The release asset CD uploads. Matched by its full name, as the C# updater
/// did, rather than by self_update's target-triple heuristic, which this name
/// does not follow.
pub const ASSET_NAME: &str = "haddy-simhub.zip";

/// The exe inside [`ASSET_NAME`] that replaces the running one. self_update
/// appends `.exe` on Windows.
pub const BIN_NAME: &str = "HaddySimHub";

/// Command-line flag that skips the startup update check. [`restart`] passes
/// it to the new exe so that a release built without `HADDYSIMHUB_VERSION`,
/// which would always look out of date, cannot update itself in a loop.
pub const NO_UPDATE_ARG: &str = "--no-update";

/// How long to wait for a stopped instance to disappear. The C# guard waited
/// up to 3s for a graceful close plus 2s after killing.
const STOP_TIMEOUT: Duration = Duration::from_secs(5);

/// The version this exe was built as: `HADDYSIMHUB_VERSION` when CD set it to
/// the release tag, otherwise the crate version for local builds.
pub fn current_version() -> &'static str {
    option_env!("HADDYSIMHUB_VERSION").unwrap_or(env!("CARGO_PKG_VERSION"))
}

/// Turns a release tag such as `v0.1.42` into the bare semver `0.1.42` that
/// self_update compares. Accepts either form, so CD may bake in the tag.
pub fn normalise_version(version: &str) -> &str {
    let trimmed = version.trim();
    trimmed
        .strip_prefix('v')
        .or_else(|| trimmed.strip_prefix('V'))
        .unwrap_or(trimmed)
}

/// The GitHub tag of a release, from the bare version self_update reports.
/// Every release CD creates is tagged with a leading `v`.
pub fn release_tag(version: &str) -> String {
    format!("v{}", normalise_version(version))
}

/// Picks the asset to download. `None` means the release cannot be installed.
pub fn select_asset(assets: &[ReleaseAsset]) -> Option<ReleaseAsset> {
    assets
        .iter()
        .find(|asset| asset.name() == ASSET_NAME)
        .cloned()
}

/// Whether a newer release is worth acting on. A release without our asset
/// (a half-finished CD run, or one made by hand) is skipped, like the C#
/// updater gave up with "No assets found in the latest release."
pub fn installable(newer: Option<Release>) -> Option<Release> {
    newer.filter(|release| select_asset(release.assets()).is_some())
}

/// Asks GitHub whether a release newer than `current` exists and can be
/// installed.
///
/// The C# app updated whenever the latest tag differed from the installed one.
/// This only moves forward, so a local build newer than the latest release is
/// left alone. A `current` that is not semver (after dropping a leading `v`) is
/// an error rather than an unconditional update.
pub fn check_for_update(current: &str) -> Result<Option<Release>> {
    let newer = updater(current, None)?.is_update_available()?;
    Ok(installable(newer))
}

/// Downloads `release` and replaces the running exe with the one inside it.
///
/// Only the exe is taken from the zip. That is the whole app now; the C#
/// release also carried the .NET runtime config and the `Updater` folder,
/// which a Rust build has no use for.
pub fn apply_update(release: &Release) -> Result<()> {
    updater(current_version(), Some(&release_tag(release.version())))?.update()?;
    Ok(())
}

/// Starts the freshly installed exe with this process's arguments plus
/// [`NO_UPDATE_ARG`], then exits. Only returns if the new exe could not be
/// started.
pub fn restart() -> Result<Infallible> {
    self_update::restart::restart_with(restart_args(std::env::args_os().skip(1)))
}

/// The arguments to restart with: the original ones, plus [`NO_UPDATE_ARG`]
/// unless it is already there.
pub fn restart_args(args: impl IntoIterator<Item = OsString>) -> Vec<OsString> {
    let mut args: Vec<OsString> = args.into_iter().collect();
    if !args.iter().any(|arg| arg == NO_UPDATE_ARG) {
        args.push(NO_UPDATE_ARG.into());
    }
    args
}

/// The startup update, as the C# `UpdateChecker.CheckAsync` ran it: check,
/// install, restart, and never let a failure stop the app from starting.
pub fn update_at_startup() {
    let current = current_version();
    log::info!("Current version: {current}");
    let result = check_for_update(current).and_then(|release| match release {
        None => {
            log::info!("No update available");
            Ok(())
        }
        Some(release) => {
            log::info!("New version available: {}", release_tag(release.version()));
            apply_update(&release)?;
            log::info!("Update installed. Restarting...");
            restart().map(|never| match never {})
        }
    });
    if let Err(error) = result {
        log::error!("Error checking for updates: {error}");
    }
}

fn updater(current: &str, tag: Option<&str>) -> Result<github::Update> {
    let mut builder = github::Update::configure();
    builder
        .repo_owner(REPO_OWNER)
        .repo_name(REPO_NAME)
        .bin_name(BIN_NAME)
        .current_version(normalise_version(current))
        .asset_matcher(select_asset)
        // Releases are `v0.1.<run>`; the default "compatible" strategy would
        // refuse to cross from 0.1 to 0.2, which the C# updater never did.
        .update_strategy(self_update::UpdateStrategy::Latest)
        // No console to prompt on: the app is a GUI.
        .unattended();
    if let Some(tag) = tag {
        builder.release_tag(tag);
    }
    builder.build()
}

/// Stops every other running copy of this exe, so a second launch replaces the
/// first instead of fighting it for the telemetry sources and the window.
///
/// Matches on the full exe path, case-insensitively, like the C# guard: a copy
/// installed elsewhere, or another program that happens to share the name, is
/// left alone.
pub fn stop_other_instances() {
    let Ok(current_exe) = std::env::current_exe() else {
        log::debug!("Could not read the path of this exe; not stopping other instances");
        return;
    };
    let Ok(current_pid) = sysinfo::get_current_pid() else {
        return;
    };

    let refresh = ProcessRefreshKind::nothing().with_exe(UpdateKind::OnlyIfNotSet);
    let mut system = System::new();
    system.refresh_processes_specifics(ProcessesToUpdate::All, true, refresh);

    let others: Vec<Pid> = system
        .processes()
        .iter()
        .filter(|(pid, process)| is_other_instance(**pid, process.exe(), current_pid, &current_exe))
        .map(|(pid, _)| *pid)
        .collect();

    for pid in others {
        // sysinfo cannot ask a Windows process to close its window, which the
        // C# guard tried first, so this goes straight to killing it.
        log::info!("Stopping existing process (ID: {pid})...");
        let killed = system.process(pid).is_some_and(|process| process.kill());
        if !killed {
            log::info!("Process {pid} already exited.");
            continue;
        }
        if !wait_for_exit(&mut system, pid, refresh) {
            log::info!("Process {pid} did not exit in time.");
        }
    }
}

/// Whether a process is another copy of the running exe.
pub fn is_other_instance(
    pid: Pid,
    exe: Option<&Path>,
    current_pid: Pid,
    current_exe: &Path,
) -> bool {
    pid != current_pid && exe.is_some_and(|exe| same_path(exe, current_exe))
}

fn same_path(a: &Path, b: &Path) -> bool {
    a.to_string_lossy().to_lowercase() == b.to_string_lossy().to_lowercase()
}

fn wait_for_exit(system: &mut System, pid: Pid, refresh: ProcessRefreshKind) -> bool {
    let deadline = Instant::now() + STOP_TIMEOUT;
    loop {
        system.refresh_processes_specifics(ProcessesToUpdate::Some(&[pid]), true, refresh);
        if system.process(pid).is_none() {
            return true;
        }
        if Instant::now() >= deadline {
            return false;
        }
        std::thread::sleep(Duration::from_millis(100));
    }
}

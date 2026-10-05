//! HaddySimHub: detects the running simulator and shows its dashboard.

mod logging;

use clap::{Parser, ValueEnum};
use simhub_core::DashboardKind;
use simhub_core::lifecycle::{DisplaysRunner, RunningProcesses, Selection};
use simhub_core::live::LiveDashboard;
use simhub_games::{GAMES, GameDisplay, Sink};
use simhub_ui::{DashboardHandle, DashboardUi};
use std::error::Error;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, Mutex};
use std::time::Duration;
use sysinfo::{ProcessesToUpdate, System};

/// How often the running processes are checked, as the C# runner did.
const POLL_INTERVAL: Duration = Duration::from_secs(2);

/// Command-line options. Parsed before anything else runs, so `--help` and a
/// mistyped option never stop another instance or reach the network.
#[derive(Parser)]
#[command(name = "HaddySimHub", version = simhub_update::current_version(), about)]
struct Options {
    /// Skip the startup update check.
    #[arg(long)]
    no_update: bool,

    /// Show a dashboard filled with sample data instead of reading a game.
    #[arg(long, value_name = "DASHBOARD")]
    demo: Option<Demo>,
}

#[derive(Clone, Copy, ValueEnum)]
enum Demo {
    Race,
    Rally,
    Truck,
    Flight,
}

impl From<Demo> for DashboardKind {
    fn from(demo: Demo) -> Self {
        match demo {
            Demo::Race => Self::Race,
            Demo::Rally => Self::Rally,
            Demo::Truck => Self::Truck,
            Demo::Flight => Self::Flight,
        }
    }
}

fn main() -> Result<(), Box<dyn Error>> {
    let options = Options::parse();

    if let Some(demo) = options.demo {
        return simhub_ui::run(simhub_core::demo_dashboard(demo.into()));
    }

    let _logger = logging::setup()?;
    log::info!("HaddySimHub {}", simhub_update::current_version());

    simhub_update::stop_other_instances();
    if !options.no_update {
        simhub_update::update_at_startup();
    }

    let ui = DashboardUi::new()?;
    let handle = ui.handle();

    let stop = Arc::new(AtomicBool::new(false));
    let runner = {
        let stop = Arc::clone(&stop);
        let handle = handle.clone();
        std::thread::Builder::new()
            .name("displays-runner".into())
            .spawn(move || run_displays(&handle, &stop))?
    };

    ui.run()?;

    log::info!("Window closed. Exiting application...");
    stop.store(true, Ordering::Relaxed);
    if runner.join().is_err() {
        log::error!("The displays runner panicked");
    }
    Ok(())
}

/// Keeps exactly one game feeding the dashboard as games come and go.
fn run_displays(handle: &DashboardHandle, stop: &AtomicBool) {
    let dashboard = Arc::new(Mutex::new(LiveDashboard::new()));
    let displays = GAMES
        .iter()
        .map(|game| GameDisplay::new(game, sink(handle, &dashboard, game.description)))
        .collect();
    let mut runner = DisplaysRunner::new(displays);
    let mut system = System::new();

    while !stop.load(Ordering::Relaxed) {
        system.refresh_processes(ProcessesToUpdate::All, true);
        let running = RunningProcesses::new(
            system
                .processes()
                .values()
                .map(|process| process.name().to_string_lossy().into_owned()),
        );

        if runner.tick(&running) == Selection::Idle {
            lock(&dashboard).snapshot(&simhub_model::DisplayUpdate::None);
            handle.show(None, "");
        }

        let mut waited = Duration::ZERO;
        while waited < POLL_INTERVAL && !stop.load(Ordering::Relaxed) {
            std::thread::sleep(Duration::from_millis(100));
            waited += Duration::from_millis(100);
        }
    }

    runner.shutdown();
}

/// Where one game's updates go: through the shared dashboard state, which keeps
/// the race trace between frames, to the window.
fn sink(handle: &DashboardHandle, dashboard: &Arc<Mutex<LiveDashboard>>, status: &str) -> Sink {
    let handle = handle.clone();
    let dashboard = Arc::clone(dashboard);
    let status = status.to_owned();
    Arc::new(move |update| {
        if log::log_enabled!(log::Level::Trace) {
            log::trace!("{update:?}");
        }
        let snapshot = lock(&dashboard).snapshot(&update);
        handle.show(snapshot, &status);
    })
}

fn lock(dashboard: &Mutex<LiveDashboard>) -> std::sync::MutexGuard<'_, LiveDashboard> {
    dashboard
        .lock()
        .unwrap_or_else(|poisoned| poisoned.into_inner())
}

#[cfg(test)]
mod tests {
    use super::*;
    use clap::CommandFactory;

    #[test]
    fn the_command_line_is_well_formed() {
        Options::command().debug_assert();
    }

    #[test]
    fn an_unknown_option_is_rejected_rather_than_ignored() {
        assert!(Options::try_parse_from(["HaddySimHub", "--capture", "x"]).is_err());
    }

    #[test]
    fn options_parse() {
        let options = Options::try_parse_from(["HaddySimHub", "--no-update"]).unwrap();
        assert!(options.no_update);
        assert!(options.demo.is_none());

        let demo = Options::try_parse_from(["HaddySimHub", "--demo", "truck"]).unwrap();
        assert!(matches!(demo.demo, Some(Demo::Truck)));
    }
}

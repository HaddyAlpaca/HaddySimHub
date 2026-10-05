//! The supported games, and the feed that runs while one of them is selected.
//!
//! Replaces `DisplayDefinitions`, `SimpleGameDisplay` and the per-game
//! providers of the C# app. Each game pairs a process name — how the runner
//! knows it is up — with a feed: a loop on its own thread that reads the
//! game's telemetry, converts it and hands every update to the dashboard until
//! it is told to stop.

mod feeds;

use simhub_core::lifecycle::{Display, RunningProcesses};
use simhub_model::DisplayUpdate;
use std::sync::Arc;
use std::sync::atomic::{AtomicBool, Ordering};
use std::thread::JoinHandle;
use std::time::Duration;

/// Receives every update a feed produces.
pub type Sink = Arc<dyn Fn(DisplayUpdate) + Send + Sync>;

/// One supported game.
pub struct Game {
    /// Short lowercase name, for logs.
    pub slug: &'static str,
    pub description: &'static str,
    /// The process the game runs under, without `.exe`.
    ///
    /// The Assetto Corsa titles all publish under the same shared memory names,
    /// so the process is what tells them apart; see `HaddySimHub/Displays/README.md`
    /// in the history of this repository.
    pub process_name: &'static str,
    feed: fn(&Feed),
}

/// The registered games, in the order the C# composition root listed them.
/// When two are running, the earlier one wins the first selection.
pub static GAMES: [Game; 8] = [
    Game {
        slug: "dirt2",
        description: "Dirt Rally 2",
        process_name: "dirtrally2",
        feed: feeds::dirt2,
    },
    Game {
        slug: "ets2",
        description: "Euro Truck Simulator 2",
        process_name: "eurotrucks2",
        feed: feeds::ets,
    },
    Game {
        slug: "iracing",
        description: "IRacing",
        process_name: "iracingui",
        feed: feeds::iracing,
    },
    Game {
        slug: "ac",
        description: "Assetto Corsa",
        process_name: "acs",
        feed: feeds::ac,
    },
    Game {
        slug: "acc",
        description: "Assetto Corsa Competizione",
        process_name: "AC2-Win64-Shipping",
        feed: feeds::acc,
    },
    Game {
        slug: "acrally",
        description: "Assetto Corsa Rally",
        process_name: "acr",
        feed: feeds::acrally,
    },
    Game {
        slug: "msfs",
        description: "Microsoft Flight Simulator 2020",
        process_name: "FlightSimulator",
        feed: feeds::msfs,
    },
    Game {
        slug: "forza",
        description: "Forza Horizon 5",
        process_name: "ForzaHorizon5",
        feed: feeds::forza,
    },
];

/// What a running feed sees: where to send updates and whether to stop.
pub struct Feed {
    game: &'static Game,
    stop: Arc<AtomicBool>,
    sink: Sink,
    received_first: AtomicBool,
}

impl Feed {
    pub fn game(&self) -> &'static Game {
        self.game
    }

    pub fn stopped(&self) -> bool {
        self.stop.load(Ordering::Relaxed)
    }

    pub fn send(&self, update: DisplayUpdate) {
        if !self.received_first.swap(true, Ordering::Relaxed) {
            log::info!("First telemetry received from {}", self.game.description);
        }
        (self.sink)(update);
    }

    /// Sleeps, but wakes early when asked to stop, so a retry delay never holds
    /// up switching to another game.
    pub fn sleep(&self, duration: Duration) {
        const STEP: Duration = Duration::from_millis(50);
        let mut remaining = duration;
        while !self.stopped() && !remaining.is_zero() {
            let step = remaining.min(STEP);
            std::thread::sleep(step);
            remaining -= step;
        }
    }
}

/// A [`Game`] as the runner sees it.
pub struct GameDisplay {
    game: &'static Game,
    sink: Sink,
    worker: Option<(Arc<AtomicBool>, JoinHandle<()>)>,
}

impl GameDisplay {
    pub fn new(game: &'static Game, sink: Sink) -> Self {
        Self {
            game,
            sink,
            worker: None,
        }
    }

    pub fn game(&self) -> &'static Game {
        self.game
    }

    pub fn all(sink: Sink) -> Vec<Self> {
        GAMES
            .iter()
            .map(|game| Self::new(game, Arc::clone(&sink)))
            .collect()
    }
}

impl Display for GameDisplay {
    fn description(&self) -> &str {
        self.game.description
    }

    fn is_active(&mut self, running: &RunningProcesses) -> Result<bool, String> {
        Ok(running.contains(self.game.process_name))
    }

    fn start(&mut self) -> Result<(), String> {
        if self.worker.is_some() {
            return Ok(());
        }

        let stop = Arc::new(AtomicBool::new(false));
        let feed = Feed {
            game: self.game,
            stop: Arc::clone(&stop),
            sink: Arc::clone(&self.sink),
            received_first: AtomicBool::new(false),
        };
        let run = self.game.feed;
        let handle = std::thread::Builder::new()
            .name(format!("feed-{}", self.game.slug))
            .spawn(move || run(&feed))
            .map_err(|error| error.to_string())?;

        self.worker = Some((stop, handle));
        Ok(())
    }

    fn stop(&mut self) -> Result<(), String> {
        let Some((stop, handle)) = self.worker.take() else {
            return Ok(());
        };
        stop.store(true, Ordering::Relaxed);
        handle
            .join()
            .map_err(|_| format!("the {} feed panicked", self.game.description))
    }
}

impl Drop for GameDisplay {
    fn drop(&mut self) {
        let _ = Display::stop(self);
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::HashSet;

    #[test]
    fn every_game_has_its_own_process_and_slug() {
        let processes: HashSet<_> = GAMES
            .iter()
            .map(|game| game.process_name.to_ascii_lowercase())
            .collect();
        let slugs: HashSet<_> = GAMES.iter().map(|game| game.slug).collect();

        assert_eq!(processes.len(), GAMES.len());
        assert_eq!(slugs.len(), GAMES.len());
    }

    #[test]
    fn the_assetto_titles_are_told_apart_by_their_process() {
        let running = RunningProcesses::new(["AC2-Win64-Shipping.exe"]);
        let sink: Sink = Arc::new(|_| {});
        let mut active: Vec<_> = GameDisplay::all(sink)
            .into_iter()
            .filter_map(|mut display| {
                display
                    .is_active(&running)
                    .unwrap()
                    .then_some(display.game().slug)
            })
            .collect();
        active.sort_unstable();

        assert_eq!(active, ["acc"]);
    }

    #[test]
    fn a_feed_sleep_ends_as_soon_as_it_is_stopped() {
        let feed = Feed {
            game: &GAMES[0],
            stop: Arc::new(AtomicBool::new(true)),
            sink: Arc::new(|_| {}),
            received_first: AtomicBool::new(false),
        };
        let started = std::time::Instant::now();
        feed.sleep(Duration::from_secs(10));
        assert!(started.elapsed() < Duration::from_secs(1));
    }
}

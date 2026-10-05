//! Which game feeds the dashboard, ported from `HaddySimHub/DisplaysRunner.cs`.
//!
//! Every display writes to the same screen, so only one of them may run at a
//! time: two open games would otherwise interleave frames of different
//! dashboard types. The display already running keeps its turn for as long as
//! its game is up, which stops the choice flip-flopping while two games are open.
//!
//! The runner is driven by [`DisplaysRunner::tick`] rather than owning a timer,
//! so the selection rules are tested without sleeping.

use std::collections::HashSet;

/// One game the dashboard can show.
pub trait Display {
    fn description(&self) -> &str;

    /// Whether this display's game is running. An error is logged and counts as
    /// not running, so one misbehaving display cannot stop the others.
    fn is_active(&mut self, running: &RunningProcesses) -> Result<bool, String>;

    fn start(&mut self) -> Result<(), String>;

    fn stop(&mut self) -> Result<(), String>;
}

/// The process names running at one moment, compared the way Windows'
/// `Process.GetProcessesByName` did: case-insensitive, without the `.exe`.
#[derive(Clone, Debug, Default)]
pub struct RunningProcesses {
    names: HashSet<String>,
}

impl RunningProcesses {
    pub fn new<I, S>(names: I) -> Self
    where
        I: IntoIterator<Item = S>,
        S: AsRef<str>,
    {
        Self {
            names: names
                .into_iter()
                .map(|name| normalise(name.as_ref()))
                .filter(|name| !name.is_empty())
                .collect(),
        }
    }

    pub fn contains(&self, process_name: &str) -> bool {
        self.names.contains(&normalise(process_name))
    }

    /// Sorted names, for the debug log that explains why no game was detected.
    pub fn sorted_names(&self) -> Vec<&str> {
        let mut names: Vec<&str> = self.names.iter().map(String::as_str).collect();
        names.sort_unstable();
        names
    }
}

fn normalise(name: &str) -> String {
    let lower = name.trim().to_ascii_lowercase();
    match lower.strip_suffix(".exe") {
        Some(stem) => stem.to_owned(),
        None => lower,
    }
}

/// What a tick decided.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Selection {
    /// No game is running; the dashboard shows its idle screen.
    Idle,
    /// The display at this index feeds the dashboard.
    Active(usize),
}

pub struct DisplaysRunner<D: Display> {
    displays: Vec<D>,
    current: Option<usize>,
    reported_idle: bool,
}

impl<D: Display> DisplaysRunner<D> {
    pub fn new(displays: Vec<D>) -> Self {
        Self {
            displays,
            current: None,
            reported_idle: false,
        }
    }

    pub fn current(&self) -> Option<&D> {
        self.current.map(|index| &self.displays[index])
    }

    pub fn displays(&self) -> &[D] {
        &self.displays
    }

    /// Re-evaluates which display should run, switching if the answer changed.
    pub fn tick(&mut self, running: &RunningProcesses) -> Selection {
        let selected = self.select(running);

        match selected {
            None => {
                if !self.reported_idle {
                    log::info!("No active displays found");
                    log::debug!(
                        "No game process detected. Running processes: {}",
                        running.sorted_names().join(", ")
                    );
                    self.reported_idle = true;
                }
            }
            Some(_) => self.reported_idle = false,
        }

        if selected != self.current {
            self.switch(selected);
        }

        match selected {
            Some(index) => Selection::Active(index),
            None => Selection::Idle,
        }
    }

    /// Stops the running display. Called once on shutdown.
    pub fn shutdown(&mut self) {
        self.switch(None);
    }

    fn select(&mut self, running: &RunningProcesses) -> Option<usize> {
        if let Some(index) = self.current
            && is_active(&mut self.displays[index], running)
        {
            return Some(index);
        }

        (0..self.displays.len()).find(|&index| is_active(&mut self.displays[index], running))
    }

    fn switch(&mut self, to: Option<usize>) {
        if let Some(from) = self.current.take() {
            let display = &mut self.displays[from];
            log::info!("Stop receiving data from {}", display.description());
            if let Err(error) = display.stop() {
                log::error!(
                    "Error stopping datafeed of game {}: {error}",
                    display.description()
                );
            }
        }

        if let Some(index) = to {
            let display = &mut self.displays[index];
            log::info!("Start receiving data from {}", display.description());
            if let Err(error) = display.start() {
                log::error!(
                    "Error starting datafeed of game {}: {error}",
                    display.description()
                );
            }
        }

        self.current = to;
    }
}

fn is_active<D: Display>(display: &mut D, running: &RunningProcesses) -> bool {
    display.is_active(running).unwrap_or_else(|error| {
        log::error!(
            "Error checking whether {} is active: {error}",
            display.description()
        );
        false
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    struct FakeDisplay {
        name: &'static str,
        process: &'static str,
        fails: bool,
        starts: usize,
        stops: usize,
    }

    impl FakeDisplay {
        fn new(name: &'static str, process: &'static str) -> Self {
            Self {
                name,
                process,
                fails: false,
                starts: 0,
                stops: 0,
            }
        }
    }

    impl Display for FakeDisplay {
        fn description(&self) -> &str {
            self.name
        }

        fn is_active(&mut self, running: &RunningProcesses) -> Result<bool, String> {
            if self.fails {
                return Err("boom".into());
            }
            Ok(running.contains(self.process))
        }

        fn start(&mut self) -> Result<(), String> {
            self.starts += 1;
            Ok(())
        }

        fn stop(&mut self) -> Result<(), String> {
            self.stops += 1;
            Ok(())
        }
    }

    fn running(names: &[&str]) -> RunningProcesses {
        RunningProcesses::new(names.iter().copied())
    }

    #[test]
    fn only_one_display_is_fed_when_two_games_are_running() {
        let mut runner = DisplaysRunner::new(vec![
            FakeDisplay::new("First", "first"),
            FakeDisplay::new("Second", "second"),
        ]);

        let selection = runner.tick(&running(&["first", "second"]));

        assert_eq!(selection, Selection::Active(0));
        assert_eq!(runner.displays()[0].starts, 1);
        assert_eq!(runner.displays()[1].starts, 0);
    }

    #[test]
    fn the_running_display_keeps_its_turn_while_its_game_is_up() {
        let mut runner = DisplaysRunner::new(vec![
            FakeDisplay::new("First", "first"),
            FakeDisplay::new("Second", "second"),
        ]);

        assert_eq!(runner.tick(&running(&["second"])), Selection::Active(1));
        // The earlier display's game starts too; the running one keeps its turn.
        assert_eq!(
            runner.tick(&running(&["first", "second"])),
            Selection::Active(1)
        );

        assert_eq!(runner.displays()[0].starts, 0);
        assert_eq!(runner.displays()[1].starts, 1);
    }

    #[test]
    fn the_runner_switches_when_the_running_displays_game_quits() {
        let mut runner = DisplaysRunner::new(vec![
            FakeDisplay::new("First", "first"),
            FakeDisplay::new("Second", "second"),
        ]);

        runner.tick(&running(&["first", "second"]));
        let selection = runner.tick(&running(&["second"]));

        assert_eq!(selection, Selection::Active(1));
        assert_eq!(runner.displays()[0].stops, 1);
        assert_eq!(runner.displays()[1].starts, 1);
    }

    #[test]
    fn the_dashboard_goes_idle_when_the_last_game_quits() {
        let mut runner = DisplaysRunner::new(vec![FakeDisplay::new("Only", "only")]);

        runner.tick(&running(&["only"]));
        let selection = runner.tick(&running(&[]));

        assert_eq!(selection, Selection::Idle);
        assert_eq!(runner.displays()[0].stops, 1);
        assert!(runner.current().is_none());
    }

    #[test]
    fn shutdown_stops_the_running_display() {
        let mut runner = DisplaysRunner::new(vec![FakeDisplay::new("Only", "only")]);

        runner.tick(&running(&["only"]));
        runner.shutdown();

        assert_eq!(runner.displays()[0].stops, 1);
        assert!(runner.current().is_none());
    }

    #[test]
    fn a_display_that_fails_to_report_its_state_does_not_stop_the_others() {
        let mut faulty = FakeDisplay::new("Faulty", "faulty");
        faulty.fails = true;
        let mut runner = DisplaysRunner::new(vec![faulty, FakeDisplay::new("Healthy", "healthy")]);

        let selection = runner.tick(&running(&["faulty", "healthy"]));

        assert_eq!(selection, Selection::Active(1));
    }

    #[test]
    fn a_display_is_started_once_while_it_stays_selected() {
        let mut runner = DisplaysRunner::new(vec![FakeDisplay::new("Only", "only")]);

        for _ in 0..5 {
            runner.tick(&running(&["only"]));
        }

        assert_eq!(runner.displays()[0].starts, 1);
        assert_eq!(runner.displays()[0].stops, 0);
    }

    #[test]
    fn process_names_match_like_windows_does() {
        let processes = running(&["AC2-Win64-Shipping.exe", "acs.EXE", "explorer.exe"]);

        assert!(processes.contains("AC2-Win64-Shipping"));
        assert!(processes.contains("ac2-win64-shipping"));
        assert!(processes.contains("acs"));
        // Only the 64-bit Assetto Corsa build is detected.
        assert!(!running(&["acs_x86.exe"]).contains("acs"));
    }
}

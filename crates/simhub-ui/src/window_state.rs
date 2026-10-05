//! Remembers where the dashboard window was, so it reopens on the same monitor
//! of a multi-monitor rig, at the same size and maximized state.
//!
//! Positions and sizes are physical pixels, which is what Windows stores and
//! what stays stable when monitors have different scale factors.

use serde::{Deserialize, Serialize};
use slint::{PhysicalPosition, PhysicalSize};
use std::io;
use std::path::{Path, PathBuf};

#[derive(Clone, Copy, Debug, PartialEq, Eq, Serialize, Deserialize)]
pub struct WindowState {
    pub x: i32,
    pub y: i32,
    pub width: u32,
    pub height: u32,
    pub maximized: bool,
}

impl WindowState {
    /// What to save for a window at `position` and `size`.
    ///
    /// A maximized window reports the monitor's corner and size. Its position is
    /// kept, because that is what puts it back on the right monitor, but the size
    /// from before it was maximized is kept too, so un-maximizing after a restart
    /// returns to it rather than to a monitor-sized normal window.
    pub fn capture(
        position: PhysicalPosition,
        size: PhysicalSize,
        maximized: bool,
        previous: Option<WindowState>,
    ) -> Self {
        let (width, height) = match (maximized, previous) {
            (true, Some(previous)) => (previous.width, previous.height),
            _ => (size.width, size.height),
        };
        Self {
            x: position.x,
            y: position.y,
            width,
            height,
            maximized,
        }
    }

    /// The point used to decide whether the window would still be visible: its
    /// centre, so a window hanging partly off a monitor edge still counts.
    pub fn centre(&self) -> (i32, i32) {
        (
            self.x.saturating_add((self.width / 2) as i32),
            self.y.saturating_add((self.height / 2) as i32),
        )
    }
}

/// `%APPDATA%\HaddySimHub\window.json`.
pub fn default_path() -> Option<PathBuf> {
    directories::ProjectDirs::from("", "", "HaddySimHub")
        .map(|dirs| dirs.config_dir().join("window.json"))
}

/// The saved state, or `None` when there is none or it cannot be read. A
/// damaged file is not worth failing over: the window just opens at its default.
pub fn load(path: &Path) -> Option<WindowState> {
    let text = std::fs::read_to_string(path).ok()?;
    match serde_json::from_str(&text) {
        Ok(state) => Some(state),
        Err(error) => {
            log::warn!("Ignoring saved window state in {}: {error}", path.display());
            None
        }
    }
}

pub fn save(path: &Path, state: &WindowState) -> io::Result<()> {
    if let Some(dir) = path.parent() {
        std::fs::create_dir_all(dir)?;
    }
    let text = serde_json::to_string_pretty(state).map_err(io::Error::other)?;
    std::fs::write(path, text)
}

/// Whether the point lies on a connected monitor. A monitor unplugged since the
/// last run would otherwise leave the window somewhere no one can see it.
pub fn on_a_monitor((x, y): (i32, i32)) -> bool {
    use windows::Win32::Foundation::POINT;
    use windows::Win32::Graphics::Gdi::{MONITOR_DEFAULTTONULL, MonitorFromPoint};

    let monitor = unsafe { MonitorFromPoint(POINT { x, y }, MONITOR_DEFAULTTONULL) };
    !monitor.is_invalid()
}

#[cfg(test)]
mod tests {
    use super::*;

    fn state(x: i32, y: i32, width: u32, height: u32, maximized: bool) -> WindowState {
        WindowState {
            x,
            y,
            width,
            height,
            maximized,
        }
    }

    #[test]
    fn a_normal_window_is_saved_as_it_is() {
        let saved = WindowState::capture(
            PhysicalPosition::new(-1920, 100),
            PhysicalSize::new(1600, 1000),
            false,
            Some(state(0, 0, 1440, 900, false)),
        );
        assert_eq!(saved, state(-1920, 100, 1600, 1000, false));
    }

    #[test]
    fn a_maximized_window_keeps_its_monitor_and_its_previous_size() {
        let saved = WindowState::capture(
            PhysicalPosition::new(2552, -8),
            PhysicalSize::new(2576, 1416),
            true,
            Some(state(2700, 100, 1500, 950, false)),
        );
        assert_eq!(saved, state(2552, -8, 1500, 950, true));
    }

    #[test]
    fn a_window_maximized_on_first_run_uses_its_own_size() {
        let saved = WindowState::capture(
            PhysicalPosition::new(-8, -8),
            PhysicalSize::new(1936, 1056),
            true,
            None,
        );
        assert_eq!(saved, state(-8, -8, 1936, 1056, true));
    }

    #[test]
    fn the_state_survives_a_save_and_load() {
        let dir = std::env::temp_dir().join(format!("hsh-window-{}", std::process::id()));
        let path = dir.join("window.json");
        let original = state(-1920, 40, 1440, 900, true);

        save(&path, &original).unwrap();
        let loaded = load(&path);
        let _ = std::fs::remove_dir_all(&dir);

        assert_eq!(loaded, Some(original));
    }

    #[test]
    fn a_damaged_file_is_ignored() {
        let dir = std::env::temp_dir().join(format!("hsh-window-bad-{}", std::process::id()));
        std::fs::create_dir_all(&dir).unwrap();
        let path = dir.join("window.json");
        std::fs::write(&path, "{ not json").unwrap();

        let loaded = load(&path);
        let _ = std::fs::remove_dir_all(&dir);

        assert_eq!(loaded, None);
    }

    #[test]
    fn a_point_far_outside_every_monitor_is_not_on_one() {
        assert!(!on_a_monitor((i32::MAX / 2, i32::MAX / 2)));
    }
}

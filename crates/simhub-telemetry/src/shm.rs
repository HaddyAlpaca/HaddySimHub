//! Read-only access to a named shared memory page that a game publishes.
//!
//! `simetry` covers the Assetto Corsa and Competizione pages, but keeps its own
//! mapping type private. Assetto Corsa Rally is not in `simetry` at all, and its
//! SCS reader spins without yielding while the game is paused, so those two read
//! their pages here and decode them with the tested byte decoders in this crate.

use std::io;
use windows::Win32::Foundation::{CloseHandle, HANDLE};
use windows::Win32::System::Memory::{
    FILE_MAP_READ, MEMORY_MAPPED_VIEW_ADDRESS, MapViewOfFile, OpenFileMappingW, UnmapViewOfFile,
};
use windows::core::HSTRING;

/// One mapped page, unmapped when dropped.
pub struct SharedMemoryPage {
    handle: HANDLE,
    view: MEMORY_MAPPED_VIEW_ADDRESS,
    len: usize,
}

// The view is only ever read through `copy_into`, which copies out of it, and
// the handle is owned by this value alone.
unsafe impl Send for SharedMemoryPage {}

impl SharedMemoryPage {
    /// Opens a page the game has already created. Fails while the game has not
    /// published it yet, which is how a caller tells "running" from "ready".
    pub fn open(name: &str, len: usize) -> io::Result<Self> {
        let handle = unsafe { OpenFileMappingW(FILE_MAP_READ.0, false, &HSTRING::from(name)) }
            .map_err(io::Error::other)?;

        let view = unsafe { MapViewOfFile(handle, FILE_MAP_READ, 0, 0, len) };
        if view.Value.is_null() {
            let error = io::Error::last_os_error();
            unsafe {
                let _ = CloseHandle(handle);
            }
            return Err(error);
        }

        Ok(Self { handle, view, len })
    }

    pub fn len(&self) -> usize {
        self.len
    }

    pub fn is_empty(&self) -> bool {
        self.len == 0
    }

    /// Copies the whole page into `buffer`, resizing it to the page length.
    ///
    /// The game writes while we read, so a single copy can be torn. Callers
    /// that care compare two copies or a packet id, as the decoders document.
    pub fn copy_into(&self, buffer: &mut Vec<u8>) {
        buffer.resize(self.len, 0);
        unsafe {
            std::ptr::copy_nonoverlapping(
                self.view.Value as *const u8,
                buffer.as_mut_ptr(),
                self.len,
            );
        }
    }
}

impl Drop for SharedMemoryPage {
    fn drop(&mut self) {
        unsafe {
            let _ = UnmapViewOfFile(self.view);
            let _ = CloseHandle(self.handle);
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn a_page_no_game_has_published_cannot_be_opened() {
        assert!(SharedMemoryPage::open(r"Local\HaddySimHubPageThatDoesNotExist", 16).is_err());
    }
}

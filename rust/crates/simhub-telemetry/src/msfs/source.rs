//! Holds one SimConnect session and drains it.
//!
//! Ported from `HaddySimHub/Displays/Msfs/SimConnectClient.cs`.
//!
//! Dispatch is polled with `SimConnect_GetNextDispatch` rather than by passing
//! a window handle to `SimConnect_Open`, so there is no Win32 message loop: the
//! app drives this from the same kind of polling thread as every other game.
//! Retrying, back-off and logging belong to that caller; this type tries once
//! and reports what went wrong.

use super::dispatch::{self, Dispatch};
use super::library::{Handle, SimConnectLibrary};
use super::{BLOCK_SIZE, DEFINITIONS, decode};
use simhub_model::telemetry::MsfsTelemetry;
use std::ffi::{CString, c_void};
use std::io;
use std::ptr;

const CLIENT_NAME: &str = "HaddySimHub";
const DEFINITION_ID: u32 = 1;
const REQUEST_ID: u32 = 1;

/// `SIMCONNECT_OBJECT_ID_USER`: the aircraft the user is flying.
const OBJECT_ID_USER: u32 = 0;

/// `SIMCONNECT_PERIOD_SIM_FRAME`.
const PERIOD_SIM_FRAME: u32 = 3;

/// `SIMCONNECT_UNUSED`, which lets the sim number the datum itself.
const UNUSED: u32 = u32::MAX;

/// `E_FAIL`, which `SimConnect_GetNextDispatch` returns when the queue is
/// simply empty. Any other failure means the connection itself is broken.
const E_FAIL: i32 = 0x8000_4005_u32 as i32;

/// SimConnect reports failure the COM way, with the sign bit set.
fn failed(hresult: i32) -> bool {
    hresult < 0
}

fn hresult_error(kind: io::ErrorKind, what: &str, hresult: i32) -> io::Error {
    io::Error::new(kind, format!("{what} failed (0x{:08X})", hresult as u32))
}

/// The last request the sim reported as malformed.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct SimConnectException {
    /// `SIMCONNECT_EXCEPTION` value.
    pub code: u32,
    /// The packet id of the offending call.
    pub send_id: u32,
    /// Which parameter of that call was at fault.
    pub index: u32,
}

/// An open SimConnect session subscribed to the user aircraft's telemetry.
pub struct MsfsSource {
    library: SimConnectLibrary,
    handle: Handle,
    exception_count: u64,
    last_exception: Option<SimConnectException>,
}

// SAFETY: the SimConnect handle is not tied to the thread that opened it, and
// `&mut self` on every call means it is never used from two threads at once.
unsafe impl Send for MsfsSource {}

impl MsfsSource {
    /// Tries to open a SimConnect session once. Err when the DLL or the sim is
    /// not available.
    ///
    /// `NotFound` means the DLL is absent, which retrying will not fix;
    /// `ConnectionRefused` means the sim is not up yet.
    pub fn connect() -> io::Result<Self> {
        let library = SimConnectLibrary::load()?;
        let name = CString::new(CLIENT_NAME).expect("no interior NUL");

        let mut handle: Handle = ptr::null_mut();
        // SAFETY: valid out pointer and NUL-terminated name; the window, user
        // event and event handle stay zero because dispatch is polled.
        let result = unsafe {
            (library.open)(
                &mut handle,
                name.as_ptr(),
                ptr::null_mut(),
                0,
                ptr::null_mut(),
                0,
            )
        };
        if failed(result) || handle.is_null() {
            return Err(hresult_error(
                io::ErrorKind::ConnectionRefused,
                "SimConnect_Open",
                result,
            ));
        }

        // Constructed before subscribing so a failure below closes the handle.
        let source = Self {
            library,
            handle,
            exception_count: 0,
            last_exception: None,
        };
        source.subscribe()?;
        Ok(source)
    }

    /// Builds the data definition and asks for it every simulation frame.
    ///
    /// Any failure here means the block would not match [`DEFINITIONS`] — a
    /// rejected simvar is left out, shifting everything after it — so the
    /// session is abandoned rather than read misaligned. Every rejection is
    /// collected first, so a bad simvar list is diagnosed in one run.
    fn subscribe(&self) -> io::Result<()> {
        let mut rejected = Vec::new();

        for definition in DEFINITIONS {
            let name = CString::new(definition.name).expect("no interior NUL");
            let unit = definition
                .unit
                .map(|unit| CString::new(unit).expect("no interior NUL"));
            // SAFETY: open handle; both strings outlive the call, and a null
            // unit is what the SDK expects for the string types.
            let result = unsafe {
                (self.library.add_to_data_definition)(
                    self.handle,
                    DEFINITION_ID,
                    name.as_ptr(),
                    unit.as_ref().map_or(ptr::null(), |unit| unit.as_ptr()),
                    definition.data_type as u32,
                    0.0,
                    UNUSED,
                )
            };
            if failed(result) {
                rejected.push(format!("{} (0x{:08X})", definition.name, result as u32));
            }
        }

        if !rejected.is_empty() {
            return Err(io::Error::new(
                io::ErrorKind::InvalidData,
                format!(
                    "the simulator rejected {} of {} simulation variables: {}",
                    rejected.len(),
                    DEFINITIONS.len(),
                    rejected.join(", ")
                ),
            ));
        }

        // SAFETY: open handle, plain integer arguments.
        let result = unsafe {
            (self.library.request_data_on_sim_object)(
                self.handle,
                REQUEST_ID,
                DEFINITION_ID,
                OBJECT_ID_USER,
                PERIOD_SIM_FRAME,
                0,
                0,
                0,
                0,
            )
        };
        if failed(result) {
            return Err(hresult_error(
                io::ErrorKind::Other,
                "SimConnect_RequestDataOnSimObject",
                result,
            ));
        }
        Ok(())
    }

    /// Drains pending dispatches without blocking. Returns the newest
    /// telemetry frame, if any.
    ///
    /// The whole queue is drained and only the newest block kept: the sim
    /// dispatches every simulation frame, faster than this is polled, and a
    /// dashboard only ever wants the latest state.
    ///
    /// Err when the sim has quit or the connection has broken; the session is
    /// closed by then, and the caller should drop this and reconnect.
    pub fn poll(&mut self) -> io::Result<Option<MsfsTelemetry>> {
        if self.handle.is_null() {
            return Err(io::Error::new(
                io::ErrorKind::NotConnected,
                "SimConnect session is closed",
            ));
        }

        let mut newest = None;

        loop {
            let mut data: *mut c_void = ptr::null_mut();
            let mut size: u32 = 0;
            // SAFETY: open handle and valid out pointers.
            let result =
                unsafe { (self.library.get_next_dispatch)(self.handle, &mut data, &mut size) };

            if result == E_FAIL || (!failed(result) && data.is_null()) {
                // The queue is empty: the normal idle case.
                return Ok(newest);
            }
            if failed(result) {
                self.close();
                return Err(hresult_error(
                    io::ErrorKind::ConnectionAborted,
                    "SimConnect_GetNextDispatch",
                    result,
                ));
            }

            // SAFETY: SimConnect owns `size` bytes at `data` until the next
            // dispatch call; they are only read within this iteration.
            let message = unsafe { std::slice::from_raw_parts(data as *const u8, size as usize) };

            match dispatch::parse(message) {
                Dispatch::Data { request_id, block } if request_id == REQUEST_ID => {
                    // A truncated block is skipped rather than half-read.
                    if block.len() >= BLOCK_SIZE {
                        newest = decode(block).or(newest);
                    }
                }
                Dispatch::Quit => {
                    self.close();
                    return Err(io::Error::new(
                        io::ErrorKind::ConnectionAborted,
                        "the simulator closed the SimConnect connection",
                    ));
                }
                Dispatch::Exception {
                    code,
                    send_id,
                    index,
                } => {
                    self.exception_count += 1;
                    self.last_exception = Some(SimConnectException {
                        code,
                        send_id,
                        index,
                    });
                }
                _ => {}
            }
        }
    }

    /// How many exceptions the sim has reported on this session.
    ///
    /// They are not fatal — a malformed request leaves the rest working — so
    /// they are counted for the caller to log rather than returned as errors.
    pub fn exception_count(&self) -> u64 {
        self.exception_count
    }

    /// The most recent exception the sim reported, if any.
    pub fn last_exception(&self) -> Option<SimConnectException> {
        self.last_exception
    }

    fn close(&mut self) {
        if self.handle.is_null() {
            return;
        }
        // SAFETY: the handle is open and is nulled so it is closed only once.
        unsafe {
            (self.library.close)(self.handle);
        }
        self.handle = ptr::null_mut();
    }
}

impl Drop for MsfsSource {
    fn drop(&mut self) {
        // Runs before the library field is dropped, so `close` is still mapped.
        self.close();
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn e_fail_is_the_com_failure_code() {
        assert_eq!(E_FAIL as u32, 0x8000_4005);
        assert!(failed(E_FAIL));
        assert!(!failed(0));
    }

    #[test]
    fn hresults_are_reported_in_hex() {
        let error = hresult_error(io::ErrorKind::Other, "SimConnect_Open", E_FAIL);
        assert_eq!(error.to_string(), "SimConnect_Open failed (0x80004005)");
    }

    #[test]
    fn the_source_can_move_to_a_polling_thread() {
        fn assert_send<T: Send>() {}
        assert_send::<MsfsSource>();
    }
}

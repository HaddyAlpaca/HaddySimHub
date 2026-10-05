//! Finds and binds the native `SimConnect.dll` at runtime.
//!
//! Ported from `Interop/SimConnectLibraryResolver.cs` and
//! `Interop/NativeMethods.cs`.
//!
//! The library ships with the MSFS SDK rather than with this application and
//! is not on the default DLL search path, so it is loaded dynamically from the
//! known locations. A machine without it gets an error from
//! [`MsfsSource::connect`](super::MsfsSource::connect) — the flight simulator
//! display stays inactive — instead of failing to start.

use libloading::Library;
use std::ffi::{c_char, c_void};
use std::io;
use std::path::{Path, PathBuf};

pub const LIBRARY_NAME: &str = "SimConnect.dll";

/// The locations searched, in order.
///
/// The application directory comes first so a copy shipped alongside the
/// executable always wins over an SDK install. `MSFS_SDK` is the variable the
/// SDK installer sets; `C:\MSFS SDK` is its default install location.
pub fn candidate_paths(exe_dir: Option<&Path>, sdk_root: Option<&str>) -> Vec<PathBuf> {
    let in_sdk = |root: &Path| root.join("SimConnect SDK").join("lib").join(LIBRARY_NAME);

    let mut paths = Vec::new();
    if let Some(dir) = exe_dir {
        paths.push(dir.join(LIBRARY_NAME));
    }
    if let Some(root) = sdk_root.filter(|root| !root.trim().is_empty()) {
        paths.push(in_sdk(Path::new(root)));
    }
    paths.push(in_sdk(Path::new(r"C:\MSFS SDK")));
    paths
}

fn default_candidates() -> Vec<PathBuf> {
    let exe = std::env::current_exe().ok();
    let exe_dir = exe.as_deref().and_then(Path::parent);
    let sdk_root = std::env::var("MSFS_SDK").ok();
    candidate_paths(exe_dir, sdk_root.as_deref())
}

pub type Handle = *mut c_void;

type OpenFn = unsafe extern "system" fn(
    handle: *mut Handle,
    name: *const c_char,
    window: *mut c_void,
    user_event_win32: u32,
    event_handle: *mut c_void,
    config_index: u32,
) -> i32;
type CloseFn = unsafe extern "system" fn(handle: Handle) -> i32;
type AddToDataDefinitionFn = unsafe extern "system" fn(
    handle: Handle,
    define_id: u32,
    datum_name: *const c_char,
    units_name: *const c_char,
    datum_type: u32,
    epsilon: f32,
    datum_id: u32,
) -> i32;
type RequestDataOnSimObjectFn = unsafe extern "system" fn(
    handle: Handle,
    request_id: u32,
    define_id: u32,
    object_id: u32,
    period: u32,
    flags: u32,
    origin: u32,
    interval: u32,
    limit: u32,
) -> i32;
type GetNextDispatchFn =
    unsafe extern "system" fn(handle: Handle, data: *mut *mut c_void, size: *mut u32) -> i32;

/// The five SimConnect entry points needed to read telemetry.
///
/// The function pointers are copied out of the library, which is kept alive
/// beside them; they are valid for as long as this value is.
pub struct SimConnectLibrary {
    pub open: OpenFn,
    pub close: CloseFn,
    pub add_to_data_definition: AddToDataDefinitionFn,
    pub request_data_on_sim_object: RequestDataOnSimObjectFn,
    pub get_next_dispatch: GetNextDispatchFn,
    _library: Library,
}

impl SimConnectLibrary {
    /// Loads from the known locations, then from the default search path,
    /// which still finds the library if it happens to be on `PATH`.
    pub fn load() -> io::Result<Self> {
        let candidates = default_candidates();
        Self::load_from(&candidates, true).map_err(|error| {
            let searched: Vec<String> = candidates
                .iter()
                .map(|path| path.display().to_string())
                .collect();
            io::Error::new(
                error.kind(),
                format!("{error}; searched {} and PATH", searched.join(", ")),
            )
        })
    }

    fn load_from(candidates: &[PathBuf], fall_back_to_search_path: bool) -> io::Result<Self> {
        let mut last_error = None;
        for candidate in candidates.iter().filter(|path| path.is_file()) {
            // SAFETY: loading SimConnect runs its DllMain, which has no
            // preconditions on us.
            match unsafe { Library::new(candidate) } {
                Ok(library) => return Self::bind(library),
                // Most often a 32-bit copy; keep looking for a usable one.
                Err(error) => last_error = Some(error),
            }
        }

        if fall_back_to_search_path {
            // SAFETY: as above.
            if let Ok(library) = unsafe { Library::new(LIBRARY_NAME) } {
                return Self::bind(library);
            }
        }

        Err(match last_error {
            Some(error) => io::Error::new(
                io::ErrorKind::InvalidData,
                format!("{LIBRARY_NAME} could not be loaded ({error}); a 64-bit build is required"),
            ),
            None => io::Error::new(
                io::ErrorKind::NotFound,
                format!("{LIBRARY_NAME} was not found"),
            ),
        })
    }

    fn bind(library: Library) -> io::Result<Self> {
        fn missing(error: libloading::Error) -> io::Error {
            io::Error::new(
                io::ErrorKind::InvalidData,
                format!("{LIBRARY_NAME} lacks an entry point: {error}"),
            )
        }

        // SAFETY: the signatures match SimConnect.h for x64, where `__stdcall`
        // and `extern "system"` are both the one Windows x64 convention.
        unsafe {
            Ok(Self {
                open: *library.get(b"SimConnect_Open\0").map_err(missing)?,
                close: *library.get(b"SimConnect_Close\0").map_err(missing)?,
                add_to_data_definition: *library
                    .get(b"SimConnect_AddToDataDefinition\0")
                    .map_err(missing)?,
                request_data_on_sim_object: *library
                    .get(b"SimConnect_RequestDataOnSimObject\0")
                    .map_err(missing)?,
                get_next_dispatch: *library
                    .get(b"SimConnect_GetNextDispatch\0")
                    .map_err(missing)?,
                _library: library,
            })
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn the_application_directory_is_searched_first() {
        let paths = candidate_paths(Some(Path::new(r"C:\app")), Some(r"D:\sdk"));
        assert_eq!(
            paths,
            vec![
                PathBuf::from(r"C:\app\SimConnect.dll"),
                PathBuf::from(r"D:\sdk\SimConnect SDK\lib\SimConnect.dll"),
                PathBuf::from(r"C:\MSFS SDK\SimConnect SDK\lib\SimConnect.dll"),
            ]
        );
    }

    #[test]
    fn a_blank_sdk_variable_is_ignored() {
        let paths = candidate_paths(None, Some("  "));
        assert_eq!(
            paths,
            vec![PathBuf::from(
                r"C:\MSFS SDK\SimConnect SDK\lib\SimConnect.dll"
            )]
        );
    }

    #[test]
    fn a_missing_library_is_reported_as_not_found() {
        let nowhere = [PathBuf::from(r"Z:\does\not\exist\SimConnect.dll")];
        let error = SimConnectLibrary::load_from(&nowhere, false)
            .err()
            .expect("nothing to load");
        assert_eq!(error.kind(), io::ErrorKind::NotFound);
    }

    #[test]
    fn a_file_that_is_not_a_library_is_reported_as_unloadable() {
        let dir = std::env::temp_dir().join(format!("simhub-msfs-{}", std::process::id()));
        std::fs::create_dir_all(&dir).expect("creates a temp dir");
        let fake = dir.join(LIBRARY_NAME);
        std::fs::write(&fake, b"not a PE image").expect("writes");

        let result = SimConnectLibrary::load_from(std::slice::from_ref(&fake), false);
        std::fs::remove_dir_all(&dir).ok();

        let error = result.err().expect("cannot load");
        assert_eq!(error.kind(), io::ErrorKind::InvalidData);
    }
}

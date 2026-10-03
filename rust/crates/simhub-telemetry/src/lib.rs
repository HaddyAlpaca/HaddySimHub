//! Readers that turn what a simulator publishes into decoded telemetry.
//!
//! Each source splits in two. Decoding — bytes to a telemetry struct — is pure
//! and lives here, so the wire layouts are pinned by tests that run anywhere.
//! Acquisition — opening a shared memory page, binding a socket, talking to
//! SimConnect — is thin, and the parts that need Windows are gated as such.
//!
//! Keeping the split means a layout mistake is caught by a unit test rather
//! than by squinting at a dashboard.

pub mod ac;
pub mod acc;
pub mod acrally;
pub mod dirt2;
pub mod ets;
pub mod forza;
pub mod iracing;
pub mod udp;

/// Reads a little-endian value out of a packet at a fixed offset.
///
/// Callers check the length once, up front, so these index directly.
mod read {
    pub fn f32_at(bytes: &[u8], offset: usize) -> f32 {
        f32::from_le_bytes(
            bytes[offset..offset + 4]
                .try_into()
                .expect("checked length"),
        )
    }

    pub fn i32_at(bytes: &[u8], offset: usize) -> i32 {
        i32::from_le_bytes(
            bytes[offset..offset + 4]
                .try_into()
                .expect("checked length"),
        )
    }

    pub fn u16_at(bytes: &[u8], offset: usize) -> u16 {
        u16::from_le_bytes(
            bytes[offset..offset + 2]
                .try_into()
                .expect("checked length"),
        )
    }

    /// Reads a fixed-width UTF-16 string, stopping at the first NUL.
    ///
    /// The Assetto static page declares these as `ByValTStr` with a Unicode
    /// charset, so the byte length is twice the character count and the tail is
    /// padding that must not be read as text.
    pub fn utf16_at(bytes: &[u8], offset: usize, byte_len: usize) -> String {
        let units: Vec<u16> = bytes[offset..offset + byte_len]
            .chunks_exact(2)
            .map(|pair| u16::from_le_bytes([pair[0], pair[1]]))
            .take_while(|unit| *unit != 0)
            .collect();
        String::from_utf16_lossy(&units)
    }
}

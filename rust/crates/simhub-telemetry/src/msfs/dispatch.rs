//! Reads the messages `SimConnect_GetNextDispatch` hands back.
//!
//! Ported from `HaddySimHub/Displays/Msfs/Interop/SimConnectMessages.cs`.
//!
//! SimConnect multiplexes every kind of notification onto one queue, each
//! message starting with a `SIMCONNECT_RECV` header. Reading the headers from a
//! byte slice rather than casting the DLL's pointer to a `#[repr(C)]` struct
//! keeps this pure, so the offsets are pinned by tests that need no simulator.

use crate::read::u32_at;

/// `SIMCONNECT_RECV`: size, version, id.
pub const RECV_HEADER_SIZE: usize = 12;

/// `SIMCONNECT_RECV_SIMOBJECT_DATA` up to, not including, its `dwData` payload.
///
/// The header plus request, object and define ids, flags, entry number, out-of
/// and define count: ten `u32`s. The data block starts straight after.
pub const SIMOBJECT_DATA_HEADER_SIZE: usize = 40;

/// `SIMCONNECT_RECV_EXCEPTION`: the header plus exception, send id and index.
pub const EXCEPTION_SIZE: usize = 24;

/// The subset of `SIMCONNECT_RECV_ID` this client acts on.
const RECV_ID_EXCEPTION: u32 = 1;
const RECV_ID_OPEN: u32 = 2;
const RECV_ID_QUIT: u32 = 3;
const RECV_ID_SIMOBJECT_DATA: u32 = 8;

const ID: usize = 8;
const REQUEST_ID: usize = 12;
const EXCEPTION_CODE: usize = 12;
const EXCEPTION_SEND_ID: usize = 16;
const EXCEPTION_INDEX: usize = 20;

/// One message off the dispatch queue.
#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub enum Dispatch<'a> {
    /// Object data for `request_id`, with `block` being the data block alone.
    Data { request_id: u32, block: &'a [u8] },
    /// The sim reported a request we sent as malformed — most often an unknown
    /// simvar name or a wrong unit.
    Exception { code: u32, send_id: u32, index: u32 },
    /// Acknowledges `SimConnect_Open`.
    Open,
    /// The sim is shutting down; the handle is dead from here on.
    Quit,
    /// Anything else, or a message too short for the type its id claims.
    /// Nothing else is subscribed to, so these are skipped.
    Other,
}

/// Classifies one dispatch message.
pub fn parse(message: &[u8]) -> Dispatch<'_> {
    if message.len() < RECV_HEADER_SIZE {
        return Dispatch::Other;
    }

    match u32_at(message, ID) {
        RECV_ID_SIMOBJECT_DATA if message.len() >= SIMOBJECT_DATA_HEADER_SIZE => Dispatch::Data {
            request_id: u32_at(message, REQUEST_ID),
            block: &message[SIMOBJECT_DATA_HEADER_SIZE..],
        },
        RECV_ID_EXCEPTION if message.len() >= EXCEPTION_SIZE => Dispatch::Exception {
            code: u32_at(message, EXCEPTION_CODE),
            send_id: u32_at(message, EXCEPTION_SEND_ID),
            index: u32_at(message, EXCEPTION_INDEX),
        },
        RECV_ID_OPEN => Dispatch::Open,
        RECV_ID_QUIT => Dispatch::Quit,
        _ => Dispatch::Other,
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn message(words: &[u32]) -> Vec<u8> {
        words.iter().flat_map(|word| word.to_le_bytes()).collect()
    }

    #[test]
    fn header_sizes_match_the_sdk() {
        assert_eq!(RECV_HEADER_SIZE, 3 * 4);
        assert_eq!(SIMOBJECT_DATA_HEADER_SIZE, 10 * 4);
        assert_eq!(EXCEPTION_SIZE, 6 * 4);
    }

    #[test]
    fn object_data_yields_the_block_after_the_forty_byte_header() {
        // size, version, id, request, object, define, flags, entry, out of, count
        let mut bytes = message(&[48, 4, 8, 7, 1, 1, 0, 1, 1, 1]);
        bytes.extend_from_slice(&[0xAA; 8]);

        assert_eq!(
            parse(&bytes),
            Dispatch::Data {
                request_id: 7,
                block: &[0xAA; 8],
            }
        );
    }

    #[test]
    fn an_exception_carries_its_code_send_id_and_index() {
        let bytes = message(&[24, 4, 1, 7, 3, 12]);
        assert_eq!(
            parse(&bytes),
            Dispatch::Exception {
                code: 7,
                send_id: 3,
                index: 12,
            }
        );
    }

    #[test]
    fn open_and_quit_are_recognised() {
        assert_eq!(parse(&message(&[12, 4, 2])), Dispatch::Open);
        assert_eq!(parse(&message(&[12, 4, 3])), Dispatch::Quit);
    }

    #[test]
    fn unsubscribed_message_types_are_skipped() {
        // 4 is SIMCONNECT_RECV_ID_EVENT, which nothing here subscribes to.
        assert_eq!(parse(&message(&[12, 4, 4])), Dispatch::Other);
    }

    #[test]
    fn truncated_messages_are_skipped_rather_than_misread() {
        assert_eq!(parse(&[]), Dispatch::Other);
        assert_eq!(parse(&message(&[12, 4])), Dispatch::Other);
        // Claims to be object data but stops inside the header.
        assert_eq!(parse(&message(&[20, 4, 8, 1, 1])), Dispatch::Other);
        assert_eq!(parse(&message(&[20, 4, 1, 1, 1])), Dispatch::Other);
    }
}

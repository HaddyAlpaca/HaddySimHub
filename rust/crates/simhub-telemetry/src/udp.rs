//! Receiving telemetry datagrams.
//!
//! Dirt Rally 2.0 and Forza Horizon 5 both broadcast over UDP, so this is the
//! one acquisition path that needs nothing from Windows and can be exercised
//! against a real socket in a test.

use std::io;
use std::net::{SocketAddr, UdpSocket};
use std::time::Duration;

/// Large enough for any telemetry datagram either game sends, with room to
/// notice one that is unexpectedly longer.
const BUFFER_SIZE: usize = 2048;

/// A bound socket that hands out whole datagrams.
#[derive(Debug)]
pub struct UdpSource {
    socket: UdpSocket,
    buffer: Box<[u8]>,
}

impl UdpSource {
    /// Binds to every interface on `port`, which is how the games are configured
    /// to send.
    pub fn bind(port: u16) -> io::Result<Self> {
        Self::bind_to(&SocketAddr::from(([0, 0, 0, 0], port)))
    }

    pub fn bind_to(address: &SocketAddr) -> io::Result<Self> {
        let socket = UdpSocket::bind(address)?;
        Ok(Self {
            socket,
            buffer: vec![0u8; BUFFER_SIZE].into_boxed_slice(),
        })
    }

    pub fn local_addr(&self) -> io::Result<SocketAddr> {
        self.socket.local_addr()
    }

    /// Bounds how long [`recv`](Self::recv) blocks, so a shutdown is not stuck
    /// waiting on a game that has already closed.
    pub fn set_read_timeout(&self, timeout: Option<Duration>) -> io::Result<()> {
        self.socket.set_read_timeout(timeout)
    }

    /// Waits for one datagram and returns exactly its bytes.
    pub fn recv(&mut self) -> io::Result<&[u8]> {
        let (len, _) = self.socket.recv_from(&mut self.buffer)?;
        Ok(&self.buffer[..len])
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Binds a receiver on an arbitrary free port and a sender aimed at it.
    fn loopback_pair() -> (UdpSource, UdpSocket, SocketAddr) {
        let receiver =
            UdpSource::bind_to(&SocketAddr::from(([127, 0, 0, 1], 0))).expect("binds a free port");
        let address = receiver.local_addr().expect("has an address");
        let sender = UdpSocket::bind(SocketAddr::from(([127, 0, 0, 1], 0))).expect("binds");
        (receiver, sender, address)
    }

    #[test]
    fn a_datagram_arrives_with_its_exact_length() {
        let (mut receiver, sender, address) = loopback_pair();
        sender.send_to(&[1, 2, 3, 4, 5], address).expect("sends");

        let received = receiver.recv().expect("receives");
        assert_eq!(received, &[1, 2, 3, 4, 5]);
    }

    #[test]
    fn datagram_boundaries_are_preserved() {
        let (mut receiver, sender, address) = loopback_pair();
        sender.send_to(&[1, 2, 3], address).expect("sends");
        sender.send_to(&[4, 5], address).expect("sends");

        // Two sends stay two reads; they are not run together as a stream would.
        assert_eq!(receiver.recv().expect("receives"), &[1, 2, 3]);
        assert_eq!(receiver.recv().expect("receives"), &[4, 5]);
    }

    #[test]
    fn a_read_timeout_gives_up_rather_than_blocking_forever() {
        let (mut receiver, _sender, _address) = loopback_pair();
        receiver
            .set_read_timeout(Some(Duration::from_millis(50)))
            .expect("sets a timeout");

        let error = receiver.recv().expect_err("nothing was sent");
        assert!(
            matches!(
                error.kind(),
                io::ErrorKind::WouldBlock | io::ErrorKind::TimedOut
            ),
            "unexpected error kind: {:?}",
            error.kind()
        );
    }
}

//! One loop per telemetry source. Each runs until its feed is stopped,
//! reconnecting on its own when the game is not ready yet or goes away.

use crate::Feed;
use simhub_convert as convert;
use simhub_model::DisplayUpdate;
use simhub_telemetry::shm::SharedMemoryPage;
use simhub_telemetry::{ac, acc, acrally, dirt2, ets, forza, iracing, msfs};
use std::io;
use std::time::Duration;

/// How often a shared memory page is read. The C# readers polled at 10 ms.
const SHARED_MEMORY_POLL: Duration = Duration::from_millis(10);

/// How long a UDP receive waits before checking whether to stop.
const UDP_TIMEOUT: Duration = Duration::from_millis(250);

/// Pause between attempts to reach a game that is running but not ready.
const RETRY_DELAY: Duration = Duration::from_secs(1);

/// Logs the first failed attempt and every hundredth after it, like the C#
/// providers, so a game sitting in its menus does not flood the log.
struct Misses {
    count: u32,
}

impl Misses {
    fn new() -> Self {
        Self { count: 0 }
    }

    fn missed(&mut self, feed: &Feed, what: &str, error: &dyn std::fmt::Display) {
        self.count += 1;
        let name = feed.game().description;
        if self.count == 1 {
            log::warn!("[{name}] {what} not available yet - retrying while the game runs: {error}");
        } else if self.count.is_multiple_of(100) {
            log::debug!("[{name}] still no {what} after {} attempts", self.count);
        }
    }

    fn connected(&mut self, feed: &Feed, what: &str) {
        let name = feed.game().description;
        if self.count > 0 {
            log::info!(
                "[{name}] Connected to {what} after {} missed attempts",
                self.count
            );
        } else {
            log::info!("[{name}] Connected to {what}");
        }
        self.count = 0;
    }
}

// ------------------------------------------------------------ shared memory

/// Reads a set of pages every [`SHARED_MEMORY_POLL`] and converts a frame
/// whenever any byte changed. Comparing bytes covers both the packet id the
/// Assetto pages bump each frame and the SCS map, which has none.
fn shared_memory(
    feed: &Feed,
    pages: &[(&str, usize)],
    mut convert: impl FnMut(&[Vec<u8>]) -> Option<DisplayUpdate>,
) {
    let mut misses = Misses::new();

    while !feed.stopped() {
        let opened: io::Result<Vec<SharedMemoryPage>> = pages
            .iter()
            .map(|(name, len)| SharedMemoryPage::open(name, *len))
            .collect();
        let opened = match opened {
            Ok(opened) => opened,
            Err(error) => {
                misses.missed(feed, "shared memory", &error);
                feed.sleep(RETRY_DELAY);
                continue;
            }
        };
        misses.connected(feed, "shared memory");

        let mut current: Vec<Vec<u8>> = vec![Vec::new(); opened.len()];
        let mut last: Vec<Vec<u8>> = Vec::new();
        while !feed.stopped() {
            for (page, buffer) in opened.iter().zip(current.iter_mut()) {
                page.copy_into(buffer);
            }
            if current != last {
                if let Some(update) = convert(&current) {
                    feed.send(update);
                }
                last.clone_from(&current);
            }
            std::thread::sleep(SHARED_MEMORY_POLL);
        }
    }
}

pub fn ets(feed: &Feed) {
    let mut converter = convert::ets::EtsConverter::new();
    shared_memory(feed, &[(ets::MAP_NAME, ets::MAP_SIZE)], |pages| {
        ets::decode(&pages[0]).map(|telemetry| converter.convert(&telemetry))
    });
}

pub fn acrally(feed: &Feed) {
    let mut converter = convert::acrally::AcRallyConverter::new();
    shared_memory(
        feed,
        &[
            (acrally::PHYSICS_PAGE, acrally::PHYSICS_SIZE),
            (acrally::GRAPHICS_PAGE, acrally::GRAPHICS_SIZE),
            (acrally::STATIC_PAGE, acrally::STATIC_SIZE),
        ],
        |pages| {
            acrally::decode(&pages[0], &pages[1], &pages[2])
                .map(|telemetry| converter.convert(&telemetry))
        },
    );
}

// ------------------------------------------------------------ UDP

/// Receives datagrams until stopped. The socket is bound once per selection;
/// a port already in use is retried, since another app may release it.
fn udp<S>(
    feed: &Feed,
    bind: impl Fn() -> io::Result<S>,
    set_timeout: impl Fn(&S) -> io::Result<()>,
    mut receive: impl FnMut(&mut S) -> io::Result<Option<DisplayUpdate>>,
) {
    let mut misses = Misses::new();

    while !feed.stopped() {
        let mut source = match bind().and_then(|source| set_timeout(&source).map(|()| source)) {
            Ok(source) => source,
            Err(error) => {
                misses.missed(feed, "UDP port", &error);
                feed.sleep(RETRY_DELAY);
                continue;
            }
        };
        misses.connected(feed, "UDP port");

        while !feed.stopped() {
            match receive(&mut source) {
                Ok(Some(update)) => feed.send(update),
                Ok(None) => {}
                Err(error)
                    if matches!(
                        error.kind(),
                        io::ErrorKind::WouldBlock | io::ErrorKind::TimedOut
                    ) => {}
                Err(error) => {
                    log::warn!("[{}] UDP receive failed: {error}", feed.game().description);
                    break;
                }
            }
        }
    }
}

pub fn dirt2(feed: &Feed) {
    udp(
        feed,
        dirt2::Dirt2Source::bind,
        |source| source.set_read_timeout(Some(UDP_TIMEOUT)),
        |source| {
            Ok(source
                .recv()?
                .map(|telemetry| convert::dirt2::convert(&telemetry)))
        },
    );
}

pub fn forza(feed: &Feed) {
    udp(
        feed,
        forza::ForzaSource::bind,
        |source| source.set_read_timeout(Some(UDP_TIMEOUT)),
        |source| {
            Ok(source
                .recv()?
                .map(|telemetry| convert::forza::convert(&telemetry)))
        },
    );
}

// ------------------------------------------------------------ SimConnect

/// SimConnect is drained without blocking, about once per screen frame.
const SIMCONNECT_POLL: Duration = Duration::from_millis(16);

pub fn msfs(feed: &Feed) {
    let mut misses = Misses::new();

    while !feed.stopped() {
        let mut source = match msfs::MsfsSource::connect() {
            Ok(source) => source,
            Err(error) => {
                misses.missed(feed, "SimConnect", &error);
                feed.sleep(RETRY_DELAY);
                continue;
            }
        };
        misses.connected(feed, "SimConnect");

        let mut exceptions = 0;
        while !feed.stopped() {
            match source.poll() {
                Ok(Some(telemetry)) => feed.send(convert::msfs::convert(&telemetry)),
                Ok(None) => std::thread::sleep(SIMCONNECT_POLL),
                Err(error) => {
                    log::warn!(
                        "[{}] SimConnect connection lost: {error}",
                        feed.game().description
                    );
                    break;
                }
            }
            if source.exception_count() != exceptions {
                exceptions = source.exception_count();
                log::warn!(
                    "[{}] SimConnect exception: {:?}",
                    feed.game().description,
                    source.last_exception()
                );
            }
        }
    }
}

// ------------------------------------------------------------ simetry

/// Runs an async feed on a runtime of this thread, ending it when the feed is
/// stopped. simetry's clients await between reads, so dropping the future at
/// that point is a clean stop.
fn run_async<F: Future<Output = ()>>(feed: &Feed, work: F) {
    let runtime = match tokio::runtime::Builder::new_current_thread()
        .enable_time()
        .build()
    {
        Ok(runtime) => runtime,
        Err(error) => {
            log::error!(
                "[{}] Cannot start the feed: {error}",
                feed.game().description
            );
            return;
        }
    };

    runtime.block_on(async {
        let stopped = async {
            while !feed.stopped() {
                tokio::time::sleep(Duration::from_millis(50)).await;
            }
        };
        tokio::select! {
            () = stopped => {}
            () = work => {}
        }
    });
}

pub fn ac(feed: &Feed) {
    use simetry::assetto_corsa::Client;
    run_async(feed, async {
        loop {
            let mut client = Client::connect(RETRY_DELAY).await;
            log::info!("[{}] Connected to shared memory", feed.game().description);
            while let Some(state) = client.next_sim_state().await {
                feed.send(convert::ac::convert(&ac::simetry_source::telemetry_from(
                    &state,
                )));
            }
            log::info!("[{}] Shared memory disconnected", feed.game().description);
        }
    });
}

pub fn acc(feed: &Feed) {
    use simetry::assetto_corsa_competizione::Client;
    run_async(feed, async {
        loop {
            let mut client = Client::connect(RETRY_DELAY).await;
            log::info!("[{}] Connected to shared memory", feed.game().description);
            while let Some(state) = client.next_sim_state().await {
                feed.send(convert::acc::convert(&acc::simetry_source::telemetry_from(
                    &state,
                )));
            }
            log::info!("[{}] Shared memory disconnected", feed.game().description);
        }
    });
}

pub fn iracing(feed: &Feed) {
    run_async(feed, async {
        let mut converter = convert::iracing::IRacingConverter::new();
        loop {
            let mut client = iracing::simetry_source::Client::connect(RETRY_DELAY).await;
            log::info!("[{}] Connected to the iRacing SDK", feed.game().description);
            while let Some(state) = client.next_sim_state().await {
                let sample = iracing::simetry_source::sample_from(&state);
                feed.send(converter.convert(&sample));
            }
            log::info!("[{}] iRacing SDK disconnected", feed.game().description);
        }
    });
}

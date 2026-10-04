# Checks that need a running game

Every decoder is unit-tested against its byte layout, and the whole path from
source to dashboard was exercised with stand-in processes (DiRT Rally 2 datagrams
and an ETS2 shared memory map). What tests cannot show is whether a game really
publishes what the layout says. Each item below is a concrete doubt found while
porting; each is settled by a few minutes in that game.

| Game | Question | What a wrong answer looks like |
| --- | --- | --- |
| Forza Horizon 5 | Is the datagram 323 or 324 bytes? | If the trailing byte belongs to a field, every offset after it is wrong and the dashboard still looks plausible |
| ACC | Does the lap counter show the session total in a lap-limited race? | simetry does not expose `numberOfLaps`, so the lap row may show only the current lap |
| ACC | Is session time remaining a thousand times too large? | The ACC reader scales the page value by 1000 where the Assetto Corsa reader divides the same field at the same offset by 1000 |
| DiRT Rally 2 | Does stage progress sit at 100% immediately? | The field at offset 12 is read as a 0-1 fraction, but the published layout calls it total distance driven |
| ETS2 | What does a 14-speed gearbox show in second gear? | The crawler labels were off by one; now `C1`, `C2`, then 1-12 |
| ETS2 | Does the dashboard update while the game is paused, and stop cleanly? | The map is polled every 10 ms and compared byte for byte |
| AC Rally | Do the values look sane at all? | Its page layout was copied from ACC, never validated, and has no published specification |
| MSFS 2020 | Does the flight dashboard connect and stay connected? | An empty SimConnect queue is assumed to return `E_FAIL`; if it returns something else, the feed reconnects in a loop (visible in the log) |
| MSFS 2020 | Do the deviation needles point the right way? | Positive is assumed to mean right of course and above the glidepath; mirrored needles are one negation |
| All | Does the first update from GitHub install and restart? | Only the check was tested against the live API; download, replace and restart run for the first time after the first Rust release |

Run with `HADDYSIMHUB_DEBUG=1` to get debug logging, or `RUST_LOG=trace` to log
every display update as well.

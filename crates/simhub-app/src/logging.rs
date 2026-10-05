//! Coloured console output plus a daily log file, as the C# app kept.
//!
//! `HADDYSIMHUB_DEBUG=1` lowers the level to debug, and `RUST_LOG` overrides it
//! entirely (`RUST_LOG=trace` also logs every display update).

use flexi_logger::{
    Age, Cleanup, Criterion, Duplicate, FileSpec, Logger, LoggerHandle, Naming,
    colored_default_format, detailed_format,
};

/// Days of log files kept, matching the C# `MaxArchiveFiles`.
const KEEP_DAYS: usize = 14;

pub fn setup() -> Result<LoggerHandle, flexi_logger::FlexiLoggerError> {
    let level = if std::env::var("HADDYSIMHUB_DEBUG").as_deref() == Ok("1") {
        "debug"
    } else {
        "info"
    };

    Logger::try_with_env_or_str(level)?
        .log_to_file(FileSpec::default().directory("log").basename("haddysimhub"))
        .rotate(
            Criterion::Age(Age::Day),
            Naming::TimestampsDirect,
            Cleanup::KeepLogFiles(KEEP_DAYS),
        )
        .format_for_files(detailed_format)
        .duplicate_to_stdout(Duplicate::All)
        .format_for_stdout(colored_default_format)
        .start()
}

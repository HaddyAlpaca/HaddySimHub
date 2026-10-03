use simhub_core::DashboardKind;
use std::error::Error;

fn main() -> Result<(), Box<dyn Error>> {
    let requested_kind = std::env::args().nth(1);
    let kind = match requested_kind.as_deref() {
        None => DashboardKind::Race,
        Some(value) => DashboardKind::from_arg(value).ok_or_else(|| {
            format!("Unknown dashboard '{value}'. Choose race, rally, truck, or flight.")
        })?,
    };

    let snapshot = simhub_core::demo_dashboard(kind);
    simhub_ui::run(snapshot)?;
    Ok(())
}

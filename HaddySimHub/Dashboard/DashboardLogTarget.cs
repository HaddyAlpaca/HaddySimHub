using NLog;
using NLog.Targets;

namespace HaddySimHub.Dashboard;

/// <summary>
/// NLog target that forwards rendered log events into the <see cref="DashboardLogStore"/>
/// so they can be displayed inside the live console dashboard instead of being
/// written directly to the console.
/// </summary>
public sealed class DashboardLogTarget : TargetWithLayout
{
    private readonly DashboardLogStore _store;

    public DashboardLogTarget(DashboardLogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    protected override void Write(LogEventInfo logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        var message = RenderLogEvent(Layout, logEvent);
        _store.Add(new LogEntry(logEvent.TimeStamp, logEvent.Level.Name, message));
    }
}

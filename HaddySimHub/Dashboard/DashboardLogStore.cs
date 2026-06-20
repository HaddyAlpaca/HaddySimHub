namespace HaddySimHub.Dashboard;

/// <summary>
/// Thread-safe ring buffer that keeps the most recent log entries so the
/// console dashboard can render them. A single shared instance is used because
/// the NLog target is created before the dependency injection container.
/// </summary>
public sealed class DashboardLogStore
{
    public static DashboardLogStore Instance { get; } = new();

    private const int Capacity = 500;
    private readonly object _gate = new();
    private readonly Queue<LogEntry> _entries = new(Capacity);

    public void Add(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_gate)
        {
            if (_entries.Count >= Capacity)
            {
                _entries.Dequeue();
            }

            _entries.Enqueue(entry);
        }
    }

    public IReadOnlyList<LogEntry> Snapshot(int maxEntries)
    {
        lock (_gate)
        {
            if (maxEntries <= 0 || _entries.Count <= maxEntries)
            {
                return _entries.ToArray();
            }

            return _entries.Skip(_entries.Count - maxEntries).ToArray();
        }
    }
}

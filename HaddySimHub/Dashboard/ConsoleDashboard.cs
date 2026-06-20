using HaddySimHub.Displays;
using HaddySimHub.Shared;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace HaddySimHub.Dashboard;

/// <summary>
/// Renders a live, btop-style console dashboard showing the web server status,
/// the available games and their connection state, and a colour-coded log feed.
/// </summary>
public sealed class ConsoleDashboard
{
    private readonly IReadOnlyList<IDisplay> _displays;
    private readonly DisplaysRunner _displaysRunner;
    private readonly DashboardLogStore _logStore;
    private readonly int _port;
    private readonly DateTime _startedUtc = DateTime.UtcNow;

    public ConsoleDashboard(
        IEnumerable<IDisplay> displays,
        DisplaysRunner displaysRunner,
        DashboardLogStore logStore,
        int port)
    {
        _displays = (displays ?? throw new ArgumentNullException(nameof(displays))).ToList();
        _displaysRunner = displaysRunner ?? throw new ArgumentNullException(nameof(displaysRunner));
        _logStore = logStore ?? throw new ArgumentNullException(nameof(logStore));
        _port = port;
    }

    /// <summary>
    /// Gets a value indicating whether the live dashboard can be rendered. It is
    /// disabled when output/input is redirected (for example in CI or when piping
    /// to a file) or when explicitly turned off via the
    /// <c>HADDYSIMHUB_NO_DASHBOARD</c> environment variable.
    /// </summary>
    public static bool IsSupported =>
        !Console.IsOutputRedirected
        && !Console.IsInputRedirected
        && Environment.GetEnvironmentVariable("HADDYSIMHUB_NO_DASHBOARD") != "1";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Clear any startup output so the dashboard renders from the top of
            // a clean screen instead of being pushed below earlier log lines.
            AnsiConsole.Clear();

            await AnsiConsole.Live(BuildLayout())
                .AutoClear(true)
                .Overflow(VerticalOverflow.Crop)
                .StartAsync(async ctx =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        ctx.UpdateTarget(BuildLayout());
                        ctx.Refresh();

                        try
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                });
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown.
        }
        catch (Exception ex)
        {
            // The dashboard is non-essential: a rendering failure must never
            // crash the host. Fall back to console logging so output stays visible.
            Logger.Error($"Console dashboard stopped unexpectedly: {ex.Message}\n\n{ex.StackTrace}");
            Logger.ActivateConsoleFallback();
        }
    }

    private IRenderable BuildLayout() => BuildLayout(GetWindowHeight());

    /// <summary>
    /// Builds the full-screen dashboard layout for a terminal of the given
    /// height. A Spectre <see cref="Layout"/> is used (rather than stacked
    /// <c>Rows</c>) so the log panel expands to fill the complete vertical
    /// space, btop-style, instead of leaving the terminal partially blank.
    /// </summary>
    internal IRenderable BuildLayout(int windowHeight)
    {
        // Status panel always shows three rows; the games panel shows one row
        // per registered game (minimum one). The taller of the two, plus the
        // rounded panel border, determines the body height.
        var bodyContentRows = Math.Max(3, Math.Max(1, _displays.Count));
        var bodyHeight = bodyContentRows + 2;

        // header rule (1) + footer (1) + log panel border (2).
        const int chrome = 4;
        var logCapacity = Math.Max(5, windowHeight - bodyHeight - chrome);

        return new Layout("root").SplitRows(
            new Layout("header").Size(1).Update(BuildHeader()),
            new Layout("body").Size(bodyHeight)
                .Update(new Columns(BuildStatusPanel(), BuildGamesPanel()).Expand()),
            new Layout("log").Ratio(1).Update(BuildLogPanel(logCapacity)),
            new Layout("footer").Size(1).Update(BuildFooter()));
    }

    private IRenderable BuildHeader()
    {
        var version = GetCurrentVersion();
        var uptime = DateTime.UtcNow - _startedUtc;
        var rule = new Rule($"[bold deepskyblue1]HaddySimHub[/]  [grey]v{Markup.Escape(version)} · uptime {uptime:hh\\:mm\\:ss}[/]")
        {
            Justification = Justify.Left,
            Style = Style.Parse("deepskyblue1"),
        };

        return rule;
    }

    private IRenderable BuildStatusPanel()
    {
        var current = _displaysRunner.CurrentDisplay;
        var testId = Program.TestId;

        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap());
        grid.AddColumn();

        grid.AddRow("[grey]Web server[/]", $"[green]●[/] listening on [white]:{_port}[/]");
        grid.AddRow(
            "[grey]Active game[/]",
            current is null
                ? "[yellow]○ idle[/]"
                : $"[green]●[/] [white]{Markup.Escape(current.Description)}[/]");
        grid.AddRow(
            "[grey]Test mode[/]",
            string.IsNullOrEmpty(testId)
                ? "[grey]disabled[/]"
                : $"[orange1]{Markup.Escape(testId)}[/]");

        return new Panel(grid)
        {
            Header = new PanelHeader("[bold]Status[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("grey"),
            Expand = true,
        };
    }

    private IRenderable BuildGamesPanel()
    {
        var grid = new Grid();
        grid.AddColumn();

        foreach (var display in _displays)
        {
            var active = SafeIsActive(display);
            var marker = active ? "[green]●[/]" : "[grey37]○[/]";
            var name = active
                ? $"[white]{Markup.Escape(display.Description)}[/]"
                : $"[grey]{Markup.Escape(display.Description)}[/]";
            grid.AddRow($"{marker} {name}");
        }

        if (_displays.Count == 0)
        {
            grid.AddRow("[grey]No games registered[/]");
        }

        return new Panel(grid)
        {
            Header = new PanelHeader("[bold]Games[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("grey"),
            Expand = true,
        };
    }

    private IRenderable BuildLogPanel(int maxLines)
    {
        var entries = _logStore.Snapshot(maxLines);

        var lines = new List<IRenderable>();
        foreach (var entry in entries)
        {
            foreach (var rawLine in entry.Message.Split('\n'))
            {
                var text = rawLine.TrimEnd('\r');
                if (text.Length == 0)
                {
                    continue;
                }

                var colour = LevelColour(entry.Level);
                var line = $"[grey]{entry.Timestamp:HH:mm:ss}[/] [{colour}]{entry.Level.ToUpperInvariant(),-5}[/] [{colour}]{Markup.Escape(text)}[/]";
                lines.Add(new Markup(line));
            }
        }

        if (lines.Count == 0)
        {
            lines.Add(new Markup("[grey]Waiting for activity...[/]"));
        }

        return new Panel(new Rows(lines))
        {
            Header = new PanelHeader("[bold]Log[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("grey"),
            Expand = true,
        };
    }

    private static IRenderable BuildFooter()
    {
        return new Markup("[grey]Press[/] [white]Ctrl+T[/] [grey]to cycle test mode  ·[/] [white]Ctrl+C[/] [grey]to quit[/]");
    }

    private static string LevelColour(string level) => level.ToUpperInvariant() switch
    {
        "FATAL" => "red1",
        "ERROR" => "red",
        "WARN" => "yellow",
        "INFO" => "green",
        "DEBUG" => "grey",
        "TRACE" => "grey37",
        _ => "white",
    };

    private static bool SafeIsActive(IDisplay display)
    {
        try
        {
            return display.IsActive;
        }
        catch
        {
            return false;
        }
    }

    private static int GetWindowHeight()
    {
        try
        {
            return Console.WindowHeight;
        }
        catch
        {
            return 30;
        }
    }

    private static string GetCurrentVersion()
    {
        try
        {
            return File.Exists(UpdateConstants.VersionFile)
                ? File.ReadAllText(UpdateConstants.VersionFile).Trim()
                : "dev";
        }
        catch
        {
            return "dev";
        }
    }
}

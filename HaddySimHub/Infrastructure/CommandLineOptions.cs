namespace HaddySimHub.Infrastructure;

/// <summary>
/// The options the application accepts on its command line.
/// </summary>
/// <remarks>
/// Parsed before anything else runs, so that <c>--help</c> and a mistyped option
/// do not stop other instances or reach the network.
/// </remarks>
public sealed record CommandLineOptions(bool E2eMode, bool NoUpdate, string? CaptureDirectory, bool HelpRequested)
{
    public const string Usage = """
        HaddySimHub

        Usage: HaddySimHub [options]

          --e2e            Bind to localhost only, expose the /__e2e data
                           injection routes, and stop polling the games. Used by
                           the Playwright suite.
          --no-update      Skip the startup update check.
          --capture <dir>  Record raw source bytes for ACC, ETS2 and iRacing to
                           <dir>/<game>.jsonl. Off unless this option is given.
                           Other game sources are not hooked up yet.
          --help, -h       Show this help.

        """;

    public static bool TryParse(string[] args, out CommandLineOptions options, out string? error)
    {
        options = new CommandLineOptions(E2eMode: false, NoUpdate: false, CaptureDirectory: null, HelpRequested: false);
        error = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--help" or "-h":
                    options = options with { HelpRequested = true };
                    break;

                case "--e2e":
                    options = options with { E2eMode = true };
                    break;

                case "--no-update":
                    options = options with { NoUpdate = true };
                    break;

                case "--capture":
                    if (i + 1 >= args.Length)
                    {
                        error = "--capture needs a directory to write to.";
                        return false;
                    }

                    options = options with { CaptureDirectory = args[++i] };
                    break;

                default:
                    error = $"Unknown option '{args[i]}'.";
                    return false;
            }
        }

        return true;
    }
}

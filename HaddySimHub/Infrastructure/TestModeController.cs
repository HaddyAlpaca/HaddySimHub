using HaddySimHub.Displays;

namespace HaddySimHub.Infrastructure;

public static class TestModeController
{
    private static readonly string[] ValidIds =
    [
        DisplayDefinitions.TestIds.Race,
        DisplayDefinitions.TestIds.Rally,
        DisplayDefinitions.TestIds.Truck,
        DisplayDefinitions.TestIds.Flight,
    ];

    public static string Current { get; private set; } = string.Empty;

    public static void ApplyArgument(string[] args)
    {
        var index = Array.FindIndex(args, a => a == "--test" || a == "--test-mode");
        string? value = null;

        if (index >= 0 && index + 1 < args.Length)
        {
            value = args[index + 1];
        }
        else
        {
            var inline = args.FirstOrDefault(a => a.StartsWith("--test=", StringComparison.Ordinal));
            if (inline is not null)
            {
                value = inline["--test=".Length..];
            }
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (Array.IndexOf(ValidIds, normalized) < 0)
        {
            Logger.Warn($"Unknown test mode '{value}'. Valid values: {string.Join(", ", ValidIds)}.");
            return;
        }

        Set(normalized);
        Logger.Info($"Test mode:'{Current}' (set via command-line argument).");
    }

    public static void Cycle()
    {
        var nextIndex = Array.IndexOf(ValidIds, Current) + 1;
        Set(nextIndex < ValidIds.Length ? ValidIds[nextIndex] : string.Empty);
        Logger.Info(string.IsNullOrEmpty(Current) ? "Test mode disabled." : $"Test mode:'{Current}'.");
    }

    public static void Set(string value)
    {
        Current = value;
    }
}

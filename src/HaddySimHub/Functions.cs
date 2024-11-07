using System.Diagnostics;

namespace HaddySimHub;

internal static class Functions
{
    public static bool IsProcessRunning(string processName) => Process.GetProcessesByName(processName).Length != 0;
}
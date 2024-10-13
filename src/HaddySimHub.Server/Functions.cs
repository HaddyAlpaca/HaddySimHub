using System.Diagnostics;

namespace HaddySimHub.Server;

internal static class Functions
{
    public static bool IsProcessRunning(string processName) => Process.GetProcessesByName(processName).Length != 0;
}
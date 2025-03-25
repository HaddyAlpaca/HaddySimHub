using System.Diagnostics;

namespace HaddySimHub.Shared
{
    public class ProcessHelper
    {
        public static bool IsProcessRunning(string processName) => Process.GetProcessesByName(processName).Length != 0;

    }
}

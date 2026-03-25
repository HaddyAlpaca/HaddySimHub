using System.Diagnostics;

namespace HaddySimHub.Shared
{
    public class ProcessHelper
    {
        public static bool IsProcessRunning(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            return processes.Length != 0;
        }
    }
}

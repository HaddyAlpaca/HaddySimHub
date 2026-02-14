using System.Diagnostics;

namespace HaddySimHub.Shared
{
    public class ProcessHelper
    {
        public static bool IsProcessRunning(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            var isRunning = processes.Length != 0;
            
            // Log process detection for debugging
            if (isRunning)
            {
                var pids = string.Join(", ", processes.Select(p => p.Id));
                System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Process '{processName}' is running. PIDs: {pids}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Process '{processName}' is NOT running");
            }
            
            return isRunning;
        }
    }
}

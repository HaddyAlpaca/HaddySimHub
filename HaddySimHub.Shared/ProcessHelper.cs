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

        /// <summary>
        /// Returns the distinct, sorted names of all currently running processes.
        /// Useful for diagnosing why a game is not detected (i.e. the configured
        /// process name does not match the actual executable name).
        /// </summary>
        public static IReadOnlyList<string> GetRunningProcessNames()
        {
            try
            {
                return Process.GetProcesses()
                    .Select(p =>
                    {
                        try
                        {
                            return p.ProcessName;
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    })
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return [];
            }
        }
    }
}

using System.Diagnostics;

namespace HaddySimHub.Shared
{
    public class ProcessHelper
    {
        // Map of process names to game descriptions
        private static readonly Dictionary<string, string> GameProcessMap = new()
        {
            { "ac2", "Assetto Corsa Competizione" },
            { "ac", "Assetto Corsa" },
            { "acr", "Assetto Corsa Rally" },
            { "eurotrucks2", "Euro Truck Simulator 2" },
            { "americantrucks", "American Truck Simulator" },
            { "iracingui", "iRacing" },
            { "dirtrally2", "Dirt Rally 2.0" },
            { "rf2", "rFactor 2" },
        };

        public static bool IsProcessRunning(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            var isRunning = processes.Length != 0;
            
            // If not found by process name, try searching by description
            if (!isRunning)
            {
                processes = FindProcessByDescription(processName);
                isRunning = processes.Length > 0;
                
                if (isRunning)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Process '{processName}' not found by name, but found by description");
                }
            }
            
            // Log process detection for debugging
            if (isRunning)
            {
                var pids = string.Join(", ", processes.Select(p => p.Id));
                System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Process '{processName}' is running. PIDs: {pids}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Process '{processName}' is NOT running");
                LogProcessSuggestions(processName);
            }
            
            return isRunning;
        }

        /// <summary>
        /// Finds processes by their description/product name
        /// </summary>
        private static Process[] FindProcessByDescription(string searchTerm)
        {
            try
            {
                var allProcesses = Process.GetProcesses();
                var matchingProcesses = new List<Process>();

                foreach (var process in allProcesses)
                {
                    try
                    {
                        var description = process.MainModule?.FileVersionInfo.ProductName ?? "";
                        var fileName = Path.GetFileNameWithoutExtension(process.MainModule?.FileName ?? "");
                        
                        // Check if description or filename contains the search term (case-insensitive)
                        if (description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            fileName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingProcesses.Add(process);
                            System.Diagnostics.Debug.WriteLine(
                                $"[ProcessHelper] Found process by description: {process.ProcessName} - {description}");
                        }
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                }

                return matchingProcesses.ToArray();
            }
            catch
            {
                return Array.Empty<Process>();
            }
        }

        /// <summary>
        /// Logs helpful suggestions when a process is not found
        /// </summary>
        private static void LogProcessSuggestions(string processName)
        {
            System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Troubleshooting for '{processName}':");
            
            // Show expected game name if available
            if (GameProcessMap.TryGetValue(processName, out var gameName))
            {
                System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Expected game: {gameName}");
                System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Please make sure {gameName} is running.");
            }

            // Show available running game processes
            var runningGameProcesses = GetRunningGameProcesses();
            if (runningGameProcesses.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Currently running games:");
                foreach (var (procName, gameName) in runningGameProcesses)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProcessHelper]   - {gameName} ({procName})");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProcessHelper] No supported game processes detected.");
                System.Diagnostics.Debug.WriteLine($"[ProcessHelper] Supported games: {string.Join(", ", GameProcessMap.Values)}");
            }
        }

        /// <summary>
        /// Gets a list of currently running game processes
        /// </summary>
        public static List<(string ProcessName, string GameName)> GetRunningGameProcesses()
        {
            var runningGames = new List<(string ProcessName, string GameName)>();

            foreach (var (processName, gameName) in GameProcessMap)
            {
                if (IsProcessRunningQuiet(processName))
                {
                    runningGames.Add((processName, gameName));
                }
            }

            return runningGames;
        }

        /// <summary>
        /// Checks if a process is running without logging
        /// </summary>
        private static bool IsProcessRunningQuiet(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    return true;
                }

                // Also check by description if not found by name
                processes = FindProcessByDescription(processName);
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

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
            
            Console.WriteLine($"[ProcessHelper] Checking for process '{processName}': found {processes.Length} instances");
            if (processes.Length > 0)
            {
                foreach (var p in processes)
                {
                    Console.WriteLine($"[ProcessHelper] - {processName} (PID: {p.Id})");
                }
            }
            
            return isRunning;
        }

        /// <summary>
        /// Finds processes by their description/product name
        /// </summary>
        public static Process[] FindProcessByDescription(string searchTerm)
        {
            try
            {
                Console.WriteLine($"[ProcessHelper] Searching for processes with description containing '{searchTerm}'");
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
                            Console.WriteLine($"[ProcessHelper] Found matching process: {process.ProcessName} (PID: {process.Id}) - Description: '{description}', Filename: '{fileName}'");
                        }
                    }
                    catch
                    {
                        // Skip processes we can't access
                        Console.WriteLine($"[ProcessHelper] Could not access process {process.ProcessName} (PID: {process.Id})");
                    }
                }

                Console.WriteLine($"[ProcessHelper] Search completed: found {matchingProcesses.Count} processes matching '{searchTerm}'");
                return [.. matchingProcesses];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProcessHelper] Error searching for processes: {ex.Message}");
                return [];
            }
        }
    }
}

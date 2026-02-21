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
            
            return isRunning;
        }

        /// <summary>
        /// Finds processes by their description/product name
        /// </summary>
        public static Process[] FindProcessByDescription(string searchTerm)
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

                return [.. matchingProcesses];
            }
            catch
            {
                return [];
            }
        }
    }
}

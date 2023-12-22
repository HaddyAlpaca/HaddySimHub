using System.Diagnostics;

namespace HaddySimHub.GameData;

public class ProcessMonitor : IProcessMonitor
{
    /// <summary>
    /// Checks whether game process is running.
    /// </summary>
    /// <returns>True if game is running, false otherwise.</returns>
    public bool IsRunning(string processName) => Process.GetProcessesByName(processName).Length != 0;
}

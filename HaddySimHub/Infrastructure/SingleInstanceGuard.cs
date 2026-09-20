namespace HaddySimHub.Infrastructure;

public static class SingleInstanceGuard
{
    public static void StopOtherInstances()
    {
        var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        var processes = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);
        var currentPath = GetExecutablePath(currentProcess);

        foreach (var process in processes)
        {
            if (process.Id == currentProcess.Id || !IsSameExecutable(process, currentPath))
            {
                continue;
            }

            try
            {
                Logger.Info($"Sending close signal to existing process {process.ProcessName} (ID: {process.Id})...");
                process.CloseMainWindow();
                if (!process.WaitForExit(3000))
                {
                    Logger.Info("Process did not close gracefully, killing...");
                    process.Kill();
                    process.WaitForExit(2000);
                }
            }
            catch (InvalidOperationException)
            {
                Logger.Info($"Process {process.Id} already exited.");
            }
        }
    }

    private static bool IsSameExecutable(System.Diagnostics.Process process, string? currentPath)
    {
        if (currentPath is null)
        {
            return false;
        }

        var path = GetExecutablePath(process);
        return path is not null && string.Equals(path, currentPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetExecutablePath(System.Diagnostics.Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch (Exception ex)
        {
            Logger.Debug($"Could not read the executable path of process {process.Id}: {ex.GetType().Name}");
            return null;
        }
    }
}

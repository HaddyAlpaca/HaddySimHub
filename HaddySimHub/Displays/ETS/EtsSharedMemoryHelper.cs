using System.IO.MemoryMappedFiles;
using HaddySimHub;

namespace HaddySimHub.Displays.ETS;

/// <summary>
/// Helper for checking ETS/ATS shared memory availability
/// </summary>
public static class EtsSharedMemoryHelper
{
    private const string SharedMemoryName = "Local\\SCSTelemetry";

    /// <summary>
    /// Check if ETS/ATS shared memory is available without fully connecting.
    /// This is faster and more reliable than process detection.
    /// </summary>
    public static bool IsSharedMemoryAvailable()
    {
        try
        {
#pragma warning disable CA1416
            using var testFile = MemoryMappedFile.OpenExisting(SharedMemoryName);
#pragma warning restore CA1416
            return true;
        }
        catch (FileNotFoundException)
        {
            // Expected when game is not running
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            // Expected when insufficient permissions
            Logger.Debug("[ETS] Insufficient permissions to access shared memory");
            return false;
        }
        catch (Exception ex)
        {
            // Unexpected error - log it for debugging
            Logger.Error($"[ETS] Unexpected error checking shared memory: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }
}

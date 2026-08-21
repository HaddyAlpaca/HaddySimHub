using System.IO.MemoryMappedFiles;

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
        catch
        {
            return false;
        }
    }
}

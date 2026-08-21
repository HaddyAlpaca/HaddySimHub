using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Reads Assetto Corsa Rally telemetry from shared memory
/// </summary>
public class ACRallySharedMemoryReader : IDisposable
{
    private const string SharedMemoryName = "Local\\assettocorsa_rally";
    private MemoryMappedFile? _memoryMappedFile;
    private MemoryMappedViewAccessor? _viewAccessor;

    public bool IsConnected { get; private set; }

    /// <summary>
    /// Check if AC Rally shared memory is available without fully connecting.
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
            Logger.Debug("[ACRally] Insufficient permissions to access shared memory");
            return false;
        }
        catch (Exception ex)
        {
            // Unexpected error - log it for debugging
            Logger.Error($"[ACRally] Unexpected error checking shared memory: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    public void Connect()
    {
        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            _memoryMappedFile = MemoryMappedFile.OpenExisting(SharedMemoryName);
#pragma warning restore CA1416 // Validate platform compatibility
            _viewAccessor = _memoryMappedFile.CreateViewAccessor(0, Marshal.SizeOf<ACRallyTelemetry>());
            IsConnected = true;
        }
        catch (Exception ex)
        {
            Logger.Debug($"[ACRally] Failed to connect to shared memory: {ex.Message}");
            IsConnected = false;
        }
    }

    public bool TryReadTelemetry(out ACRallyTelemetry telemetry)
    {
        telemetry = default;

        if (_viewAccessor == null || !IsConnected)
        {
            return false;
        }

        try
        {
            _viewAccessor.Read(0, out telemetry);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Debug($"[ACRally] Failed to read telemetry: {ex.Message}");
            return false;
        }
    }

    public void Disconnect()
    {
        IsConnected = false;
        _viewAccessor?.Dispose();
        _memoryMappedFile?.Dispose();
    }

    public void Dispose()
    {
        Disconnect();
    }
}

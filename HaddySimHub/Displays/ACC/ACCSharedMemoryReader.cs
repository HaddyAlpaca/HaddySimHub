using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Diagnostics;
using HaddySimHub;

namespace HaddySimHub.Displays.ACC;

/// <summary>
/// Reads Assetto Corsa Competizione telemetry from shared memory
/// </summary>
public class ACCSharedMemoryReader : IDisposable
{
    private const string SharedMemoryName = "Local\\assettocorsa_physical";
    private MemoryMappedFile? _memoryMappedFile;
    private MemoryMappedViewAccessor? _viewAccessor;

    public bool IsConnected { get; private set; }

    public void Connect()
    {
        Logger.Info($"ACCSharedMemoryReader: attempting to open shared memory '{SharedMemoryName}'.");
        try
        {
            Logger.Debug($"[ACC] Attempting to connect to shared memory: {SharedMemoryName}");
#pragma warning disable CA1416 // Validate platform compatibility
            _memoryMappedFile = MemoryMappedFile.OpenExisting(SharedMemoryName);
#pragma warning restore CA1416 // Validate platform compatibility
            _viewAccessor = _memoryMappedFile.CreateViewAccessor(0, Marshal.SizeOf<ACCTelemetry>());
            IsConnected = true;
            Logger.Info($"[ACC] Successfully connected to shared memory");
        }
        catch (Exception ex)
        {
            IsConnected = false;
            Logger.Debug($"[ACC] Failed to connect to shared memory: {ex.GetType().Name} - {ex.Message}");
        }
    }

    public bool TryReadTelemetry(out ACCTelemetry telemetry)
    {
        telemetry = default;

        if (_viewAccessor == null || !IsConnected)
        {
            Logger.Debug("ACCSharedMemoryReader: TryReadTelemetry called but not connected or view accessor is null.");
            return false;
        }

        try
        {
            _viewAccessor.Read(0, out telemetry);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error("ACCSharedMemoryReader: error reading telemetry: " + ex.Message);
            Logger.Debug(ex.ToString());
            return false;
        }
    }

    public void Disconnect()
    {
        _viewAccessor?.Dispose();
        _memoryMappedFile?.Dispose();
        IsConnected = false;
    }

    public void Dispose()
    {
        Disconnect();
    }
}

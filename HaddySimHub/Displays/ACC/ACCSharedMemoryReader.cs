using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

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
        try
        {
            Logger.Debug($"[ACC] Attempting to connect to shared memory: {SharedMemoryName}");
            _memoryMappedFile = MemoryMappedFile.OpenExisting(SharedMemoryName);
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
            return false;
        }

        try
        {
            _viewAccessor.Read(0, out telemetry);
            return true;
        }
        catch
        {
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

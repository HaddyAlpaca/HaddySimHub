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

    public void Connect()
    {
        try
        {
            _memoryMappedFile = MemoryMappedFile.OpenExisting(SharedMemoryName);
            _viewAccessor = _memoryMappedFile.CreateViewAccessor(0, Marshal.SizeOf<ACRallyTelemetry>());
            IsConnected = true;
        }
        catch
        {
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
        catch
        {
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

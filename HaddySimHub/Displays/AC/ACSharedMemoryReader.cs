using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.AC;

/// <summary>
/// Reads Assetto Corsa telemetry from shared memory
/// </summary>
public class ACSharedMemoryReader : IDisposable
{
    private const string SharedMemoryName = "Local\\assettocorsa";
    private MemoryMappedFile? _memoryMappedFile;
    private MemoryMappedViewAccessor? _viewAccessor;

    public bool IsConnected { get; private set; }

    public void Connect()
    {
        try
        {
            _memoryMappedFile = MemoryMappedFile.OpenExisting(SharedMemoryName);
            _viewAccessor = _memoryMappedFile.CreateViewAccessor(0, Marshal.SizeOf<ACTelemetry>());
            IsConnected = true;
        }
        catch
        {
            IsConnected = false;
        }
    }

    public bool TryReadTelemetry(out ACTelemetry telemetry)
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

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
            _memoryMappedFile = MemoryMappedFile.OpenExisting(SharedMemoryName);
            _viewAccessor = _memoryMappedFile.CreateViewAccessor(0, Marshal.SizeOf<ACCTelemetry>());
            IsConnected = true;
            Logger.Info("ACCSharedMemoryReader: connected to shared memory.");
        }
        catch (Exception ex)
        {
            IsConnected = false;
            Logger.Error($"ACCSharedMemoryReader: failed to open shared memory '{SharedMemoryName}': {ex.Message}");
            Logger.Debug(ex.ToString());

            try
            {
                var procs = Process.GetProcesses()
                    .Where(p => p.ProcessName.ToLower().Contains("assetto") || p.ProcessName.ToLower().Contains("acc") || p.ProcessName.ToLower().Contains("ac"))
                    .Select(p => $"{p.ProcessName} (Id={p.Id})")
                    .Take(50);

                Logger.Info("ACCSharedMemoryReader: matching processes: " + (procs.Any() ? string.Join(", ", procs) : "(none found)"));
            }
            catch (Exception px)
            {
                Logger.Debug("ACCSharedMemoryReader: failed to enumerate processes: " + px.ToString());
            }
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

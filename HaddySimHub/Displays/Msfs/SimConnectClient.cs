using System.Runtime.InteropServices;
using HaddySimHub.Displays.Msfs.Interop;

namespace HaddySimHub.Displays.Msfs;

/// <summary>
/// Reads the user aircraft's telemetry over SimConnect.
/// </summary>
public sealed class SimConnectClient : ISimConnectClient
{
    private const string ClientName = "HaddySimHub";
    private const uint DefinitionId = 1;
    private const uint RequestId = 1;

    /// <summary>Opening a connection while the sim is not up fails, and the provider polls far faster than a game starts.</summary>
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);

    /// <summary>Stops one bad request from filling the log at the dispatch rate.</summary>
    private const int ExceptionLogInterval = 100;

    private static readonly int PayloadOffset = Marshal.SizeOf<SimConnectRecvSimObjectData>();

    private readonly Lock _sync = new();

    private nint _handle;
    private bool _disposed;
    private bool _libraryMissing;
    private DateTime _lastAttemptUtc = DateTime.MinValue;
    private int _exceptionCount;

    public bool IsConnected
    {
        get
        {
            lock (_sync)
            {
                return _handle != nint.Zero;
            }
        }
    }

    public void Connect()
    {
        lock (_sync)
        {
            if (_disposed || _handle != nint.Zero || _libraryMissing)
            {
                return;
            }

            if (DateTime.UtcNow - _lastAttemptUtc < RetryInterval)
            {
                return;
            }

            _lastAttemptUtc = DateTime.UtcNow;

            try
            {
                if (NativeMethods.Failed(NativeMethods.SimConnect_Open(out var handle, ClientName, nint.Zero, 0, nint.Zero, 0)))
                {
                    return;
                }

                _handle = handle;
            }
            catch (DllNotFoundException)
            {
                // Without the SDK's redistributable there is nothing to retry, so stop
                // trying and say once where it is expected to be.
                _libraryMissing = true;
                Logger.Warn(
                    $"[Msfs] {NativeMethods.LibraryName} was not found, so Microsoft Flight Simulator telemetry is unavailable. " +
                    $"Searched: {string.Join(", ", SimConnectLibraryResolver.CandidatePaths())}");
                return;
            }
            catch (BadImageFormatException ex)
            {
                _libraryMissing = true;
                Logger.Warn($"[Msfs] {NativeMethods.LibraryName} could not be loaded ({ex.Message}). A 64-bit build is required.");
                return;
            }

            if (!TrySubscribe())
            {
                Close();
            }
        }
    }

    public void Disconnect()
    {
        lock (_sync)
        {
            Close();
        }
    }

    public bool TryReadTelemetry(out MsfsTelemetry telemetry)
    {
        telemetry = default;

        lock (_sync)
        {
            if (_handle == nint.Zero)
            {
                return false;
            }

            var received = false;

            // Drain the whole queue and keep the newest block: the sim dispatches on
            // every simulation frame, which is faster than this is polled, and a
            // dashboard only ever wants the latest state.
            while (!NativeMethods.Failed(NativeMethods.SimConnect_GetNextDispatch(_handle, out var data, out _)) && data != nint.Zero)
            {
                var header = Marshal.PtrToStructure<SimConnectRecv>(data);

                switch ((SimConnectRecvId)header.Id)
                {
                    case SimConnectRecvId.SimObjectData:
                        var message = Marshal.PtrToStructure<SimConnectRecvSimObjectData>(data);
                        if (message.RequestId == RequestId)
                        {
                            telemetry = Marshal.PtrToStructure<MsfsTelemetry>(data + PayloadOffset);
                            received = true;
                        }

                        break;

                    case SimConnectRecvId.Quit:
                        // The sim is shutting down; the handle is dead from here on.
                        Logger.Info("[Msfs] Simulator closed the SimConnect connection");
                        Close();
                        return received;

                    case SimConnectRecvId.Exception:
                        LogException(data);
                        break;

                    case SimConnectRecvId.Open:
                        Logger.Info("[Msfs] SimConnect connection opened");
                        break;

                    default:
                        // Every other notification type shares this queue; none are subscribed to.
                        break;
                }
            }

            return received;
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Close();
        }
    }

    /// <summary>
    /// Builds the data definition and asks for it every simulation frame. Any failure
    /// here means the block would not match <see cref="MsfsTelemetry"/>, so the caller
    /// closes the connection rather than reading misaligned data.
    /// </summary>
    private bool TrySubscribe()
    {
        // Collect every rejection before giving up rather than stopping at the first.
        // A rejected simvar is fatal either way -- it is left out of the block, so
        // everything after it would be read from the wrong offset -- but naming all of
        // them at once turns diagnosing a bad simvar into one run instead of several.
        var rejected = new List<string>();

        foreach (var definition in SimVarDefinitions.All)
        {
            var result = NativeMethods.SimConnect_AddToDataDefinition(
                _handle,
                DefinitionId,
                definition.Name,
                definition.Unit,
                (uint)definition.DataType,
                0f,
                uint.MaxValue);

            if (NativeMethods.Failed(result))
            {
                rejected.Add($"{definition.Name} (0x{result:X8})");
            }
        }

        if (rejected.Count > 0)
        {
            Logger.Warn(
                $"[Msfs] The simulator rejected {rejected.Count} of {SimVarDefinitions.All.Count} simulation variables, " +
                $"so telemetry cannot be read: {string.Join(", ", rejected)}");
            return false;
        }

        var request = NativeMethods.SimConnect_RequestDataOnSimObject(
            _handle,
            RequestId,
            DefinitionId,
            NativeMethods.ObjectIdUser,
            (uint)SimConnectPeriod.SimFrame,
            flags: 0,
            origin: 0,
            interval: 0,
            limit: 0);

        if (NativeMethods.Failed(request))
        {
            Logger.Warn($"[Msfs] Subscribing to telemetry failed (0x{request:X8})");
            return false;
        }

        Logger.Info($"[Msfs] Subscribed to {SimVarDefinitions.All.Count} simulation variables");
        return true;
    }

    private void LogException(nint data)
    {
        var message = Marshal.PtrToStructure<SimConnectRecvException>(data);
        _exceptionCount++;

        if (_exceptionCount == 1 || _exceptionCount % ExceptionLogInterval == 0)
        {
            Logger.Warn($"[Msfs] SimConnect reported exception {message.ExceptionCode} on request {message.SendId}, index {message.Index}");
        }
    }

    private void Close()
    {
        if (_handle == nint.Zero)
        {
            return;
        }

        try
        {
            NativeMethods.SimConnect_Close(_handle);
        }
        catch (DllNotFoundException)
        {
            // The library vanished between opening and closing; nothing left to release.
        }

        _handle = nint.Zero;
        _exceptionCount = 0;
    }
}

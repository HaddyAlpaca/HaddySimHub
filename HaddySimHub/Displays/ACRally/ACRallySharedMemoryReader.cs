using HaddySimHub.Capture;

namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Reads Assetto Corsa Rally telemetry from the three shared memory pages the
/// game publishes.
/// </summary>
/// <remarks>
/// Assetto Corsa Rally reuses the Assetto Corsa page names
/// (<c>Local\acpmf_physics</c>, <c>_graphics</c> and <c>_static</c>), so the
/// presence of these pages says nothing about <em>which</em> title of the
/// Assetto Corsa family is running. Deciding that is the display's job; this
/// class only supplies the data.
/// </remarks>
public sealed class ACRallySharedMemoryReader : ISharedMemoryTelemetryReader<ACRallyTelemetry>
{
    private const string PhysicsMemoryName = "Local\\acpmf_physics";
    private const string GraphicsMemoryName = "Local\\acpmf_graphics";
    private const string StaticMemoryName = "Local\\acpmf_static";

    private readonly SharedMemoryPage<ACRallyPhysics> _physics = new(PhysicsMemoryName, "[ACRally] physics");
    private readonly SharedMemoryPage<ACRallyGraphics> _graphics = new(GraphicsMemoryName, "[ACRally] graphics");
    private readonly SharedMemoryPage<ACRallyStatic> _static = new(StaticMemoryName, "[ACRally] static");
    private readonly TelemetryCapture? _capture;

    private bool _staticLogged;
    private int? _lastPacketId;

    public ACRallySharedMemoryReader(TelemetryCapture? capture = null)
    {
        _capture = capture;
    }

    /// <summary>
    /// True once the pages carrying live telemetry are mapped. The static page is
    /// best-effort: it only supplies fallbacks, so a session still streams without it.
    /// </summary>
    public bool IsConnected => _physics.IsConnected && _graphics.IsConnected;

    public void Connect()
    {
        var wasConnected = IsConnected;

        _physics.TryOpen();
        _graphics.TryOpen();
        _static.TryOpen();

        if (IsConnected && !wasConnected)
        {
            Logger.Info("[ACRally] Connected to shared memory");
        }
    }

    public bool TryReadTelemetry(out ACRallyTelemetry telemetry)
    {
        telemetry = default;

        if (!IsConnected)
        {
            return false;
        }

        try
        {
            var capturing = _capture?.Enabled == true;
            ACRallyPhysics physics;
            ACRallyGraphics graphics;
            byte[] physicsBytes;
            byte[] graphicsBytes;
            if (capturing)
            {
                if (!_physics.TryReadStable(out physics, out physicsBytes, captureFullPage: true) ||
                    !_graphics.TryReadStable(out graphics, out graphicsBytes, captureFullPage: true))
                {
                    _capture!.MarkIncomplete("source-page-unstable:acrally");
                    return false;
                }
            }
            else
            {
                physics = _physics.Read();
                graphics = _graphics.Read();
                physicsBytes = [];
                graphicsBytes = [];
            }

            // The static page is written once per session. Missing it costs only the
            // rev limit and stage length, both of which have physics/graphics fallbacks,
            // so keep trying for it while the live pages already stream.
            if (!_static.IsConnected)
            {
                _static.TryOpen();
            }

            byte[] staticBytes = [];
            var staticInfo = _static.IsConnected
                ? _static.Read(out staticBytes, captureFullPage: capturing)
                : default;
            if (capturing)
            {
                if (!_static.IsConnected)
                {
                    _capture!.MarkIncomplete("source-page-unavailable:acrally:static");
                    return false;
                }

                _capture!.RecordRawFrame(
                    "acrally",
                    "shared-memory",
                    $"{PhysicsMemoryName} + {GraphicsMemoryName} + {StaticMemoryName}",
                    new Dictionary<string, byte[]>
                    {
                        ["physics"] = physicsBytes,
                        ["graphics"] = graphicsBytes,
                        ["static"] = staticBytes,
                    });

                if (physics.PacketId != graphics.PacketId)
                {
                    _capture.MarkIncomplete($"source-pages-mismatched:acrally:{physics.PacketId}-{graphics.PacketId}");
                }
                else if (_lastPacketId is int previousPacketId &&
                         HaddySimHub.Displays.ACC.ACCSharedMemoryReader.HasPacketGap(previousPacketId, physics.PacketId))
                {
                    _capture.MarkIncomplete($"source-gap:acrally:{previousPacketId}-{physics.PacketId}");
                }

                _lastPacketId = physics.PacketId;
            }

            if (_static.IsConnected && !_staticLogged)
            {
                _staticLogged = true;
                Logger.Info(
                    $"[ACRally] Session: car '{staticInfo.CarModel}' on '{staticInfo.Track}' " +
                    $"(shared memory {staticInfo.SmVersion}, game {staticInfo.AcVersion})");
            }

            telemetry = new ACRallyTelemetry
            {
                PacketId = physics.PacketId,
                Gas = physics.Gas,
                Brake = physics.Brake,
                Clutch = physics.Clutch,
                SteerAngle = physics.SteerAngle,
                Gear = physics.Gear,
                Rpms = physics.Rpms,
                CurrentMaxRpm = physics.CurrentMaxRpm,
                SpeedKmh = physics.SpeedKmh,
                Fuel = physics.Fuel,
                TurboBoost = physics.TurboBoost,
                WaterTemperature = physics.WaterTemperature,
                IsEngineRunning = physics.IsEngineRunning,
                PitLimiterOn = physics.PitLimiterOn,
                AbsInAction = physics.AbsInAction,
                TcInAction = physics.TcInAction,

                Status = graphics.Status,
                SessionType = graphics.SessionType,
                CurrentTime = graphics.CurrentTime,
                LastTime = graphics.LastTime,
                BestTime = graphics.BestTime,
                CompletedLap = graphics.CompletedLap,
                NumberOfLaps = graphics.NumberOfLaps,
                Position = graphics.Position,
                NormalizedCarPosition = graphics.NormalizedCarPosition,
                DistanceTraveled = graphics.DistanceTraveled,
                CurrentSectorIndex = graphics.CurrentSectorIndex,
                LastSectorTime = graphics.LastSectorTime,
                IsValidLap = graphics.IsValidLap,
                SessionTimeLeft = graphics.SessionTimeLeft,
                DeltaLapTime = graphics.DeltaLapTime,
                IsDeltaPositive = graphics.IsDeltaPositive,

                MaxRpm = staticInfo.MaxRpm,
                TrackSplineLength = staticInfo.TrackSplineLength,
                SectorCount = staticInfo.SectorCount,
                CarModel = staticInfo.CarModel ?? string.Empty,
                Track = staticInfo.Track ?? string.Empty,
                SmVersion = staticInfo.SmVersion ?? string.Empty,
                AcVersion = staticInfo.AcVersion ?? string.Empty,
            };

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"[ACRally] Error reading telemetry: {ex.GetType().Name}: {ex.Message}");
            Logger.Debug(ex.ToString());
            Disconnect();
            return false;
        }
    }

    public void Disconnect()
    {
        _physics.Close();
        _graphics.Close();
        _static.Close();
        _staticLogged = false;
    }

    public void Dispose()
    {
        Disconnect();
    }
}

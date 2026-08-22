namespace HaddySimHub.Displays.AC;

/// <summary>
/// Reads Assetto Corsa telemetry from the three shared memory pages the game publishes.
/// </summary>
/// <remarks>
/// The page names are shared with Assetto Corsa Competizione and Assetto Corsa Rally,
/// so their presence does not identify the title; the display decides that from the
/// running process. See <c>Displays/README.md</c>.
/// </remarks>
public sealed class ACSharedMemoryReader : IDisposable
{
    private const string PhysicsMemoryName = "Local\\acpmf_physics";
    private const string GraphicsMemoryName = "Local\\acpmf_graphics";
    private const string StaticMemoryName = "Local\\acpmf_static";

    private readonly SharedMemoryPage<ACPhysics> _physics = new(PhysicsMemoryName, "[AC] physics");
    private readonly SharedMemoryPage<ACGraphics> _graphics = new(GraphicsMemoryName, "[AC] graphics");
    private readonly SharedMemoryPage<ACStatic> _static = new(StaticMemoryName, "[AC] static");

    private bool _staticLogged;

    /// <summary>
    /// True once the pages carrying live telemetry are mapped. The static page is
    /// best-effort: it only supplies the rev limit and tank size.
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
            Logger.Info("[AC] Connected to shared memory");
        }
    }

    public bool TryReadTelemetry(out ACTelemetry telemetry)
    {
        telemetry = default;

        if (!IsConnected)
        {
            return false;
        }

        try
        {
            var physics = _physics.Read();
            var graphics = _graphics.Read();

            // The static page is written once per session, so keep trying for it
            // while the live pages already stream.
            if (!_static.IsConnected)
            {
                _static.TryOpen();
            }

            var staticInfo = _static.IsConnected ? _static.Read() : default;

            if (_static.IsConnected && !_staticLogged)
            {
                _staticLogged = true;
                Logger.Info(
                    $"[AC] Session: car '{staticInfo.CarModel}' on '{staticInfo.Track}' " +
                    $"(shared memory {staticInfo.SmVersion}, game {staticInfo.AcVersion})");
            }

            telemetry = new ACTelemetry
            {
                PacketId = physics.PacketId,
                Gas = physics.Gas,
                Brake = physics.Brake,
                Clutch = physics.Clutch,
                SteerAngle = physics.SteerAngle,
                Gear = physics.Gear,
                Rpms = physics.Rpms,
                SpeedKmh = physics.SpeedKmh,
                Fuel = physics.Fuel,
                AirTemp = physics.AirTemp,
                RoadTemp = physics.RoadTemp,
                BrakeBias = physics.BrakeBias,
                PitLimiterOn = physics.PitLimiterOn,

                Status = graphics.Status,
                SessionType = graphics.SessionType,
                CompletedLaps = graphics.CompletedLaps,
                NumberOfLaps = graphics.NumberOfLaps,
                Position = graphics.Position,
                CurrentTime = graphics.CurrentTime,
                LastTime = graphics.LastTime,
                BestTime = graphics.BestTime,
                SessionTimeLeft = graphics.SessionTimeLeft,
                NormalizedCarPosition = graphics.NormalizedCarPosition,
                DistanceTraveled = graphics.DistanceTraveled,

                MaxRpm = staticInfo.MaxRpm,
                MaxFuel = staticInfo.MaxFuel,
                CarModel = staticInfo.CarModel ?? string.Empty,
                Track = staticInfo.Track ?? string.Empty,
                SmVersion = staticInfo.SmVersion ?? string.Empty,
                AcVersion = staticInfo.AcVersion ?? string.Empty,
            };

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"[AC] Error reading telemetry: {ex.GetType().Name}: {ex.Message}");
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

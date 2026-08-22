using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

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
public sealed class ACRallySharedMemoryReader : IDisposable
{
    private const string PhysicsMemoryName = "Local\\acpmf_physics";
    private const string GraphicsMemoryName = "Local\\acpmf_graphics";
    private const string StaticMemoryName = "Local\\acpmf_static";

    private readonly SharedMemoryPage<ACRallyPhysics> _physics = new(PhysicsMemoryName, "physics");
    private readonly SharedMemoryPage<ACRallyGraphics> _graphics = new(GraphicsMemoryName, "graphics");
    private readonly SharedMemoryPage<ACRallyStatic> _static = new(StaticMemoryName, "static");

    private bool _staticLogged;

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
            var physics = _physics.Read();
            var graphics = _graphics.Read();

            // The static page is written once per session. Missing it costs only the
            // rev limit and stage length, both of which have physics/graphics fallbacks,
            // so keep trying for it while the live pages already stream.
            if (!_static.IsConnected)
            {
                _static.TryOpen();
            }

            var staticInfo = _static.IsConnected ? _static.Read() : default;

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

    /// <summary>
    /// One memory mapped page, marshalled into <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// The pages contain fixed-size arrays and UTF-16 strings, so they have to be
    /// marshalled rather than copied with <see cref="MemoryMappedViewAccessor.Read{T}"/>,
    /// which rejects any struct holding references.
    /// </remarks>
    private sealed class SharedMemoryPage<T>(string mapName, string label)
        where T : struct
    {
        private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);

        private readonly int _size = Marshal.SizeOf<T>();
        private MemoryMappedFile? _file;
        private MemoryMappedViewAccessor? _accessor;
        private DateTime _lastAttemptUtc = DateTime.MinValue;

        public bool IsConnected => _accessor is not null;

        public void TryOpen()
        {
            // Opening a page that is not published throws, and the provider polls at
            // 100 Hz, so back off between attempts rather than paying for it per frame.
            if (IsConnected || DateTime.UtcNow - _lastAttemptUtc < RetryInterval)
            {
                return;
            }

            _lastAttemptUtc = DateTime.UtcNow;

            try
            {
#pragma warning disable CA1416 // Validate platform compatibility
                _file = MemoryMappedFile.OpenExisting(mapName, MemoryMappedFileRights.Read);
#pragma warning restore CA1416 // Validate platform compatibility
                _accessor = _file.CreateViewAccessor(0, _size, MemoryMappedFileAccess.Read);
            }
            catch (FileNotFoundException)
            {
                // The game is not running, or has not published this page yet.
                Close();
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Debug($"[ACRally] Access denied to the {label} page ({mapName})");
                Close();
            }
            catch (Exception ex)
            {
                Logger.Debug($"[ACRally] Failed to map the {label} page: {ex.GetType().Name}: {ex.Message}");
                Close();
            }
        }

        public T Read()
        {
            var accessor = _accessor ?? throw new InvalidOperationException($"The {label} page is not mapped.");

            var buffer = new byte[_size];
            accessor.ReadArray(0, buffer, 0, buffer.Length);

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        public void Close()
        {
            _accessor?.Dispose();
            _accessor = null;
            _file?.Dispose();
            _file = null;
        }
    }
}

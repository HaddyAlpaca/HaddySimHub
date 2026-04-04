using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using HaddySimHub;

namespace HaddySimHub.Displays.ACC;

public class ACCSharedMemoryReader : IDisposable
{
    private const string PhysicsMemoryName = "Local\\assettocorsa_competizione";
    private const string GraphicsMemoryName = "Local\\assettocorsa_competizione_gfx";
    
    private MemoryMappedFile? _physicsFile;
    private MemoryMappedViewAccessor? _physicsAccessor;
    private MemoryMappedFile? _graphicsFile;
    private MemoryMappedViewAccessor? _graphicsAccessor;
    
    private int _connectionAttempts;
    private DateTime _lastConnectionAttempt = DateTime.MinValue;
    private static readonly TimeSpan ReconnectCooldown = TimeSpan.FromSeconds(2);
    
    private bool _physicsConnected;
    private bool _graphicsConnected;

    public bool IsConnected => _physicsConnected && _graphicsConnected;
    public bool IsPhysicsConnected => _physicsConnected;
    public bool IsGraphicsConnected => _graphicsConnected;

    public void Connect()
    {
        if (IsConnected) return;
        
        var now = DateTime.Now;
        if ((now - _lastConnectionAttempt) < ReconnectCooldown && _connectionAttempts > 0) return;
        _lastConnectionAttempt = now;
        _connectionAttempts++;
        
        TryConnectPhysics();
        TryConnectGraphics();
        
        if (IsConnected)
        {
            Logger.Info($"[ACC] Connected to shared memory (Physics: {_physicsConnected}, Graphics: {_graphicsConnected})");
        }
        else if (_connectionAttempts <= 3)
        {
            Logger.Debug($"[ACC] Connecting... (Physics: {_physicsConnected}, Graphics: {_graphicsConnected})");
        }
    }

    private void TryConnectPhysics()
    {
        if (_physicsConnected) return;
        
        try
        {
            var size = Marshal.SizeOf<ACCPhysics>();
#pragma warning disable CA1416
            _physicsFile = MemoryMappedFile.OpenExisting(PhysicsMemoryName);
#pragma warning restore CA1416
            _physicsAccessor = _physicsFile.CreateViewAccessor(0, size);
            _physicsConnected = true;
            Logger.Debug($"[ACC] Physics page connected ({size} bytes)");
        }
        catch (FileNotFoundException)
        {
            Logger.Debug($"[ACC] Physics shared memory not found: {PhysicsMemoryName}");
        }
        catch (UnauthorizedAccessException)
        {
            Logger.Error($"[ACC] Access denied to physics shared memory. Run as administrator.");
        }
        catch (Exception ex)
        {
            Logger.Debug($"[ACC] Failed to connect to physics shared memory: {ex.Message}");
        }
    }

    private void TryConnectGraphics()
    {
        if (_graphicsConnected) return;
        
        try
        {
            var size = Marshal.SizeOf<ACCGraphics>();
#pragma warning disable CA1416
            _graphicsFile = MemoryMappedFile.OpenExisting(GraphicsMemoryName);
#pragma warning restore CA1416
            _graphicsAccessor = _graphicsFile.CreateViewAccessor(0, size);
            _graphicsConnected = true;
            Logger.Debug($"[ACC] Graphics page connected ({size} bytes)");
        }
        catch (FileNotFoundException)
        {
            Logger.Debug($"[ACC] Graphics shared memory not found: {GraphicsMemoryName}");
        }
        catch (UnauthorizedAccessException)
        {
            Logger.Error($"[ACC] Access denied to graphics shared memory. Run as administrator.");
        }
        catch (Exception ex)
        {
            Logger.Debug($"[ACC] Failed to connect to graphics shared memory: {ex.Message}");
        }
    }

    public bool TryReadTelemetry(out ACCTelemetry telemetry)
    {
        telemetry = default;

        if (!_physicsConnected || !_graphicsConnected)
        {
            Connect();
            return false;
        }

        try
        {
            ACCPhysics physics = default;
            ACCGraphics graphics = default;
            
            if (_physicsAccessor != null && _physicsConnected)
            {
                _physicsAccessor.Read(0, out physics);
                if (physics.CarDamage == null) physics.CarDamage = new float[5];
                if (physics.RideHeight == null) physics.RideHeight = new float[2];
            }
            
            if (_graphicsAccessor != null && _graphicsConnected)
            {
                _graphicsAccessor.Read(0, out graphics);
                graphics.TyreCompound ??= string.Empty;
            }
            
            telemetry = new ACCTelemetry
            {
                PacketId = physics.PacketId,
                Gas = physics.Gas,
                Brake = physics.Brake,
                Fuel = physics.Fuel,
                Gear = physics.Gear,
                Rpms = physics.Rpms,
                SteerAngle = physics.SteerAngle,
                SpeedKmh = physics.SpeedKmh,
                VelocityX = physics.Velocity.X,
                VelocityY = physics.Velocity.Y,
                VelocityZ = physics.Velocity.Z,
                AccGX = physics.AccG.X,
                AccGY = physics.AccG.Y,
                AccGZ = physics.AccG.Z,
                WheelSlipFl = physics.WheelSlip.FrontLeft,
                WheelSlipFr = physics.WheelSlip.FrontRight,
                WheelSlipRl = physics.WheelSlip.RearLeft,
                WheelSlipRr = physics.WheelSlip.RearRight,
                WheelLoadFl = physics.WheelLoad.FrontLeft,
                WheelLoadFr = physics.WheelLoad.FrontRight,
                WheelLoadRl = physics.WheelLoad.RearLeft,
                WheelLoadRr = physics.WheelLoad.RearRight,
                WheelPressureFl = physics.WheelsPressure.FrontLeft,
                WheelPressureFr = physics.WheelsPressure.FrontRight,
                WheelPressureRl = physics.WheelsPressure.RearLeft,
                WheelPressureRr = physics.WheelsPressure.RearRight,
                WheelAngularSpeedFl = physics.WheelAngularSpeed.FrontLeft,
                WheelAngularSpeedFr = physics.WheelAngularSpeed.FrontRight,
                WheelAngularSpeedRl = physics.WheelAngularSpeed.RearLeft,
                WheelAngularSpeedRr = physics.WheelAngularSpeed.RearRight,
                TyreWearFl = physics.TyreWear.FrontLeft,
                TyreWearFr = physics.TyreWear.FrontRight,
                TyreWearRl = physics.TyreWear.RearLeft,
                TyreWearRr = physics.TyreWear.RearRight,
                TyreCoreTempFl = physics.TyreCoreTemperature.FrontLeft,
                TyreCoreTempFr = physics.TyreCoreTemperature.FrontRight,
                TyreCoreTempRl = physics.TyreCoreTemperature.RearLeft,
                TyreCoreTempRr = physics.TyreCoreTemperature.RearRight,
                BrakeTempFl = physics.BrakeTemp.FrontLeft,
                BrakeTempFr = physics.BrakeTemp.FrontRight,
                BrakeTempRl = physics.BrakeTemp.RearLeft,
                BrakeTempRr = physics.BrakeTemp.RearRight,
                CamberRadFl = physics.CamberRad.FrontLeft,
                CamberRadFr = physics.CamberRad.FrontRight,
                CamberRadRl = physics.CamberRad.RearLeft,
                CamberRadRr = physics.CamberRad.RearRight,
                SuspensionTravelFl = physics.SuspensionTravel.FrontLeft,
                SuspensionTravelFr = physics.SuspensionTravel.FrontRight,
                SuspensionTravelRl = physics.SuspensionTravel.RearLeft,
                SuspensionTravelRr = physics.SuspensionTravel.RearRight,
                Drs = physics.Drs,
                TC = physics.TC,
                Heading = physics.Heading,
                Pitch = physics.Pitch,
                Roll = physics.Roll,
                CgHeight = physics.CgHeight,
                FrontLeftDamage = physics.CarDamage?[0] ?? 0,
                FrontRightDamage = physics.CarDamage?[1] ?? 0,
                RearLeftDamage = physics.CarDamage?[2] ?? 0,
                RearRightDamage = physics.CarDamage?[3] ?? 0,
                CenterDamage = physics.CarDamage?[4] ?? 0,
                NumberOfTyresOut = physics.NumberOfTyresOut,
                PitLimiterOn = physics.PitLimiterOn,
                Abs = physics.Abs,
                KersCharge = physics.KersCharge,
                KersInput = physics.KersInput,
                AutoShifterOn = physics.AutoShifterOn,
                RideHeightFront = physics.RideHeight?[0] ?? 0,
                RideHeightRear = physics.RideHeight?[1] ?? 0,
                TurboBoost = physics.TurboBoost,
                Ballast = physics.Ballast,
                AirDensity = physics.AirDensity,
                AirTemp = physics.AirTemp,
                RoadTemp = physics.RoadTemp,
                LocalAngularVelocityX = physics.LocalAngularVelocity.X,
                LocalAngularVelocityY = physics.LocalAngularVelocity.Y,
                LocalAngularVelocityZ = physics.LocalAngularVelocity.Z,
                FinalFF = physics.FinalFF,
                PerformanceMeter = physics.PerformanceMeter,
                EngineBrake = physics.EngineBrake,
                DrsAvailable = physics.DrsAvailable,
                DrsEnabled = physics.DrsEnabled,
                Clutch = physics.Clutch,
                TyreTempIFl = physics.TyreTempI.FrontLeft,
                TyreTempIFr = physics.TyreTempI.FrontRight,
                TyreTempIRl = physics.TyreTempI.RearLeft,
                TyreTempIRr = physics.TyreTempI.RearRight,
                TyreTempMFl = physics.TyreTempM.FrontLeft,
                TyreTempMFr = physics.TyreTempM.FrontRight,
                TyreTempMRl = physics.TyreTempM.RearLeft,
                TyreTempMRr = physics.TyreTempM.RearRight,
                TyreTempOFl = physics.TyreTempO.FrontLeft,
                TyreTempOFr = physics.TyreTempO.FrontRight,
                TyreTempORl = physics.TyreTempO.RearLeft,
                TyreTempORr = physics.TyreTempO.RearRight,
                IsAIControlled = physics.IsAIControlled,
                BrakeBias = physics.BrakeBias,
                LocalVelocityX = physics.LocalVelocity.X,
                LocalVelocityY = physics.LocalVelocity.Y,
                LocalVelocityZ = physics.LocalVelocity.Z,
                P2PActivation = physics.P2PActivation,
                P2PStatus = physics.P2PStatus,
                MaxRpm = physics.CurrentMaxRpm,
                TcinAction = physics.TcinAction,
                AbsInAction = physics.AbsInAction,
                WaterTemp = physics.WaterTemp,
                FrontBrakeCompound = physics.FrontBrakeCompound,
                RearBrakeCompound = physics.RearBrakeCompound,
                PadLifeFl = physics.PadLife.FrontLeft,
                PadLifeFr = physics.PadLife.FrontRight,
                PadLifeRl = physics.PadLife.RearLeft,
                PadLifeRr = physics.PadLife.RearRight,
                DiscLifeFl = physics.DiscLife.FrontLeft,
                DiscLifeFr = physics.DiscLife.FrontRight,
                DiscLifeRl = physics.DiscLife.RearLeft,
                DiscLifeRr = physics.DiscLife.RearRight,
                IgnitionOn = physics.IgnitionOn,
                StarterEngineOn = physics.StarterEngineOn,
                IsEngineRunning = physics.IsEngineRunning,
                KerbVibration = physics.KerbVibration,
                SlipVibrations = physics.SlipVibrations,
                GVibrations = physics.GVibrations,
                AbsVibrations = physics.AbsVibrations,
                
                Status = (int)graphics.Status,
                SessionType = (int)graphics.SessionType,
                CurrentTimeMs = graphics.CurrentTime,
                LastTimeMs = graphics.LastTime,
                BestTimeMs = graphics.BestTime,
                CurrentLap = graphics.CompletedLap,
                Position = graphics.Position,
                SessionTimeLeftMs = (int)(graphics.SessionTimeLeft * 1000),
                DistanceTraveled = graphics.DistanceTraveled,
                IsInPit = graphics.IsInPit,
                CurrentSectorIndex = graphics.CurrentSectorIndex,
                LastSectorTimeMs = graphics.LastSectorTime,
                NumberOfLaps = graphics.NumberOfLaps,
                TyreCompound = graphics.TyreCompound ?? string.Empty,
                NormalizedCarPosition = graphics.NormalizedCarPosition,
                PenaltyTime = graphics.PenaltyTime,
                Flag = (int)graphics.Flag,
                Penalty = (int)graphics.Penalty,
                IdealLineOn = graphics.IdealLineOn,
                IsInPitLane = graphics.IsInPitLane,
                MandatoryPitDone = graphics.MandatoryPitDone,
                WindSpeed = graphics.WindSpeed,
                WindDirection = graphics.WindDirection,
                TcLevel = graphics.TcLevel,
                TcCutLevel = graphics.TcCutLevel,
                EngineMap = graphics.EngineMap,
                AbsLevel = graphics.AbsLevel,
                FuelPerLap = graphics.FuelPerLap,
                RainLight = graphics.RainLight,
                FlashingLight = graphics.FlashingLight,
                ExhaustTemp = graphics.ExhaustTemp,
                UsedFuel = graphics.UsedFuel,
                DeltaMs = graphics.DeltaLapTime,
                IsDeltaPositive = graphics.IsDeltaPositive,
                IsValidLap = graphics.IsValidLap,
                FuelEstimatedLaps = graphics.FuelEstimatedLaps,
                MissingMandatoryPits = graphics.MissingMandatoryPits,
                CurrentTyreSet = graphics.CurrentTyreSet,
                StrategyTyreSet = graphics.StrategyTyreSet,
                GapAheadMs = graphics.GapAhead,
                GapBehindMs = graphics.GapBehind,
                TrackGripStatus = graphics.TrackGripStatus,
                RainIntensity = graphics.RainIntensity
            };
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"[ACC] Error reading telemetry: {ex.GetType().Name}: {ex.Message}");
            Logger.Debug(ex.ToString());
            _physicsConnected = false;
            _graphicsConnected = false;
            return false;
        }
    }

    public void Disconnect()
    {
        _physicsAccessor?.Dispose();
        _physicsFile?.Dispose();
        _graphicsAccessor?.Dispose();
        _graphicsFile?.Dispose();
        _physicsConnected = false;
        _graphicsConnected = false;
        Logger.Info("[ACC] Disconnected from shared memory");
    }

    public void Dispose()
    {
        Disconnect();
    }
}

namespace HaddySimHub.Displays.ACC;

/// <summary>
/// Reads Assetto Corsa Competizione telemetry from the shared memory pages the
/// game publishes.
/// </summary>
/// <remarks>
/// The page names are shared with Assetto Corsa and Assetto Corsa Rally, so their
/// presence does not identify the title; the display decides that from the running
/// process. See <c>Displays/README.md</c>.
/// </remarks>
public sealed class ACCSharedMemoryReader : ISharedMemoryTelemetryReader<ACCTelemetry>
{
    private const string PhysicsMemoryName = "Local\\acpmf_physics";
    private const string GraphicsMemoryName = "Local\\acpmf_graphics";

    private readonly SharedMemoryPage<ACCPhysics> _physics = new(PhysicsMemoryName, "[ACC] physics");
    private readonly SharedMemoryPage<ACCGraphics> _graphics = new(GraphicsMemoryName, "[ACC] graphics");

    public bool IsConnected => _physics.IsConnected && _graphics.IsConnected;

    public void Connect()
    {
        var wasConnected = IsConnected;

        _physics.TryOpen();
        _graphics.TryOpen();

        if (IsConnected && !wasConnected)
        {
            Logger.Info("[ACC] Connected to shared memory");
        }
    }

    public bool TryReadTelemetry(out ACCTelemetry telemetry)
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
            Disconnect();
            return false;
        }
    }

    public void Disconnect()
    {
        _physics.Close();
        _graphics.Close();
    }

    public void Dispose()
    {
        Disconnect();
    }
}

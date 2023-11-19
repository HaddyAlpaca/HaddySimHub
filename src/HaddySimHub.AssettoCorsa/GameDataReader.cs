using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;

namespace HaddySimHub.AssettoCorsa;

public sealed class GameDataReader(ISharedMemoryReaderFactory sharedMemoryReaderFactory) : IGameDataReader, IDisposable
{
    private readonly ISharedMemoryReader<Physics> physicsFile = sharedMemoryReaderFactory.Create<Physics>("Local\\acpmf_physics");
    private readonly ISharedMemoryReader<Graphics> graphicsFile = sharedMemoryReaderFactory.Create<Graphics>("Local\\acpmf_graphics");
    private readonly ISharedMemoryReader<StaticInfo> staticInfoFile = sharedMemoryReaderFactory.Create<StaticInfo>("Local\\acpmf_static");

    public string ProcessName => "acs";

    public void Dispose()
    {
        this.physicsFile.Dispose();
        this.graphicsFile.Dispose();
        this.staticInfoFile.Dispose();
    }

    public object ReadRawData()
    {
        return new {
            physicsInfo = this.physicsFile.Read(),
            graphicsInfo = this.graphicsFile.Read(),
            staticInfo = this.staticInfoFile.Read()
        };
    }

    public object ReadData()
    {
        //Read all data
        Physics physicsInfo = this.physicsFile.Read();
        Graphics graphicsInfo = this.graphicsFile.Read();
        StaticInfo staticInfo = this.staticInfoFile.Read();

        return new RaceData
        {
            Rpm = physicsInfo.Rpms,
            Fuel = physicsInfo.Fuel,
            TyreTemps = new TyreData<float>
            {
                FrontLeft = physicsInfo.TyreTemp[0],
                FrontRight = physicsInfo.TyreTemp[1],
                RearLeft = physicsInfo.TyreTemp[2],
                RearRight = physicsInfo.TyreTemp[3]
            },
            TyrePressures = new TyreData<float>
            {
                FrontLeft = physicsInfo.WheelsPressure[0],
                FrontRight = physicsInfo.WheelsPressure[1],
                RearLeft = physicsInfo.WheelsPressure[2],
                RearRight = physicsInfo.WheelsPressure[3]
            },
            BrakeTemps = new TyreData<float>
            {
                FrontLeft = physicsInfo.BrakeTemp[0],
                FrontRight = physicsInfo.BrakeTemp[1],
                RearLeft = physicsInfo.BrakeTemp[2],
                RearRight = physicsInfo.BrakeTemp[3]
            },
            BrakeBias = physicsInfo.BrakeBias,
            Speed = (int)physicsInfo.SpeedKmh,
            Gear = physicsInfo.Gear - 1,
            AirTemp = physicsInfo.AirTemp,
            TrackTemp = physicsInfo.RoadTemp,
            TcLevel = graphicsInfo.TC,
            AbsLevel = graphicsInfo.ABS,
            Position = graphicsInfo.Position,
            LastLapTime = graphicsInfo.iLastTime,
            DeltaTime = graphicsInfo.IDeltaLapTime,
            EstimatedLapTime = graphicsInfo.iEstimatedLapTime,
            BestLapTime = graphicsInfo.iBestTime,
            EngineMapping = graphicsInfo.EngineMap,
            GapAhead = graphicsInfo.GapAhead,
            GapBehind = graphicsInfo.GapBehind,
            FuelPerLap = graphicsInfo.FuelXLap,
            CompletedLaps = graphicsInfo.CompletedLaps,
            TotalLaps = graphicsInfo.NumberOfLaps,
            SessionTimeRemaining = graphicsInfo.SessionTimeLeft,
            Flag = graphicsInfo.Flag switch
            {
                AC_FLAG_TYPE.AC_GREEN_FLAG => Flag.Green,
                AC_FLAG_TYPE.AC_BLUE_FLAG => Flag.Blue,
                AC_FLAG_TYPE.AC_YELLOW_FLAG => Flag.Yellow,
                AC_FLAG_TYPE.AC_BLACK_FLAG => Flag.Black,
                AC_FLAG_TYPE.AC_WHITE_FLAG => Flag.White,
                AC_FLAG_TYPE.AC_CHECKERED_FLAG => Flag.Chequered,
                AC_FLAG_TYPE.AC_PENALTY_FLAG or AC_FLAG_TYPE.AC_ORANGE_FLAG => Flag.BlackAndOrange,
                _ => Flag.None,
            },
            GripLevel = graphicsInfo.TrackGripStatus switch
            {
                AC_TRACK_GRIP_STATUS.AC_GREEN => GripLevel.Green,
                AC_TRACK_GRIP_STATUS.AC_FAST => GripLevel.Fast,
                AC_TRACK_GRIP_STATUS.AC_OPTIMUM => GripLevel.Optimum,
                AC_TRACK_GRIP_STATUS.AC_GREASY => GripLevel.Greasy,
                AC_TRACK_GRIP_STATUS.AC_DAMP => GripLevel.Damp,
                AC_TRACK_GRIP_STATUS.AC_WET => GripLevel.Wet,
                AC_TRACK_GRIP_STATUS.AC_FLOODED => GripLevel.Flooded,
                _ => GripLevel.Unknown
            },
            WeatherType = graphicsInfo.RainIntensity switch
            {
                AC_RAIN_INTENSITY.AC_NO_RAIN => WeatherType.Dry,
                AC_RAIN_INTENSITY.AC_DRIZZLE => WeatherType.Drizzle,
                AC_RAIN_INTENSITY.AC_LIGHT_RAIN => WeatherType.LightRain,
                AC_RAIN_INTENSITY.AC_MEDIUM_RAIN => WeatherType.MediumRain,
                AC_RAIN_INTENSITY.AC_HEAVY_RAIN => WeatherType.HeavyRain,
                AC_RAIN_INTENSITY.AC_THUNDERSTORM => WeatherType.Thunderstorm,
                _ => WeatherType.Unknown
            },
            RpmMax = staticInfo.MaxRpm,
            NumberOfCars = staticInfo.NumCars,
            IsTimedSession = staticInfo.IsTimedRace == 1
        };
    }
}

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
            FuelLaps = physicsInfo.Fuel / graphicsInfo.FuelXLap,
            BrakeBias = physicsInfo.BrakeBias,
            Speed = (int)physicsInfo.SpeedKmh,
            Gear = physicsInfo.Gear - 1,
            AirTemp = physicsInfo.AirTemp,
            TrackTemp = physicsInfo.RoadTemp,
            Position = graphicsInfo.Position,
            LastLapTime = graphicsInfo.iLastTime,
            DeltaTime = graphicsInfo.IDeltaLapTime,
            BestLapTime = graphicsInfo.iBestTime,
            GapAhead = graphicsInfo.GapAhead,
            GapBehind = graphicsInfo.GapBehind,
            CompletedLaps = graphicsInfo.CompletedLaps,
            TotalLaps = graphicsInfo.NumberOfLaps,
            SessionTimeRemaining = graphicsInfo.SessionTimeLeft,
            NumberOfCars = staticInfo.NumCars,
            IsTimedSession = staticInfo.IsTimedRace == 1
        };
    }
}

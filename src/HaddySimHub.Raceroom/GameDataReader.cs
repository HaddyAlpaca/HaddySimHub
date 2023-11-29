﻿using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using HaddySimHub.Raceroom.Data;

namespace HaddySimHub.Raceroom;

public sealed class GameDateReader(ISharedMemoryReaderFactory sharedMemoryReaderFactory) : IGameDataReader, IDisposable
{
    private readonly ISharedMemoryReader<Shared> mmf = sharedMemoryReaderFactory.Create<Shared>("$R3E");
    public string ProcessName => "rrre";

    public object ReadRawData() => this.mmf.Read();

    public object ReadData()
    {
        Shared rawData = this.mmf.Read();

        //Set session type
        string sessionType;
        switch(rawData.SessionType)
        {
            case Enums.Session.Practice:
                sessionType = "Practice";
                break;
            
            case Enums.Session.Qualify:
                sessionType = "Qualifying";
                break;

            case Enums.Session.Warmup:
                sessionType = "Warmup";
                break;

            case Enums.Session.Race:
                sessionType = "Race";
                break;
            
            default:
                sessionType = string.Empty;
                break; 
        }

        return new RaceData
        {
            Speed = (int)MpsToKph(rawData.CarSpeed),
            Gear = rawData.Gear,
            Rpm = (int)RpsToRpm(rawData.EngineRps),
            BrakeBias = rawData.BrakeBias,
            SessionTimeRemaining = rawData.SessionTimeRemaining,
            CompletedLaps = rawData.CompletedLaps,
            TotalLaps = rawData.NumberOfLaps,
            GapBehind = rawData.TimeDeltaBehind,
            ThrottlePct = Convert.ToInt32(rawData.Throttle * 100),
            BrakePct = Convert.ToInt32(rawData.Brake * 100),
            SessionType = sessionType
        };
    }

    public void Dispose() => this.mmf.Dispose();

    private static float RpsToRpm(float rps) => rps * (60 / (2 * (float)Math.PI));

    private static float MpsToKph(float mps) => mps * 3.6f;
}

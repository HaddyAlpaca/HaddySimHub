using System.Text;
using HaddySimHub.Models;
using HaddySimHub.Shared;
using iRacingSDK;

namespace HaddySimHub.Displays.IRacing;

internal sealed class Display() : DisplayBase<DataSample>()
{
    private int[]? _lastLaps;
    private int? _sessionNum;
    protected override int PageCount => 2;

    public override void Start()
    {
        iRacing.NewData += async (data) => 
        {
            try
            {
                await this.SendUpdate(data);
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}\n\n{ex.StackTrace}");
            }
        };
        iRacing.StartListening();
    }

    public override void Stop()
    {
        if (iRacing.IsConnected) {
            iRacing.StopListening();
        }
    }

    public override string Description => "IRacing";
    public override bool IsActive => ProcessHelper.IsProcessRunning("iracingui");

    protected override DisplayUpdate ConvertToDisplayUpdate(DataSample data)
    {
        var telemetry = data.Telemetry;

        var sessionData = data.SessionData;
        var session = sessionData.SessionInfo.Sessions.First(s => s.SessionNum == telemetry.SessionNum);

        _lastLaps ??= (int[])telemetry.CarIdxLap.Clone();

        if (_sessionNum != telemetry.SessionNum)
        {
            _sessionNum = telemetry.SessionNum;
            _lastLaps = (int[])telemetry.CarIdxLap.Clone();
        }

        foreach (var driver in sessionData.DriverInfo.CompetingDrivers)
        {
            var carIdx = (int)driver.CarIdx;

            if (!telemetry.HasData(carIdx))
            {
                Logger.Debug($"Car {carIdx} does not have telemetry data.");
                continue;
            }

            var carIdxLap = telemetry.CarIdxLap[carIdx];
            var carIdxLapDistPct = telemetry.CarIdxLapDistPct[carIdx];
            if (carIdxLap > _lastLaps[carIdx] && carIdxLapDistPct > 0.80f)
            {
                // The car has passed the start/finish line and the percentage is in the 80% range.
                // Set the percentage to 0% to avoid the bug in iRacing data stream.
                carIdxLapDistPct = 0;
            }

            _lastLaps[carIdx] = carIdxLap;

            // Build a log message
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"*** Player {carIdx} telemetry data ***");
            logMessage.AppendLine($"License color {driver.LicColor}");
            logMessage.AppendLine($"License string {driver.LicString}");
            logMessage.AppendLine($"IRating: {driver.IRating}");
            logMessage.AppendLine($"In pit: {telemetry.CarIdxOnPitRoad[carIdx]}");

            foreach (var car in telemetry.Cars)
            {
                logMessage.AppendLine($"Car {car.CarIdx} - {car.Details.UserName} - Est.Time: {telemetry.CarIdxEstTime[car.CarIdx]}");
            }

            // Overwrite the console output
            Console.SetCursorPosition(0, 0);
            Console.Write(logMessage.ToString().PadRight(Console.WindowWidth * Console.WindowHeight));
        }

        var displayUpdate = new RaceData
        {
            SessionType = session.SessionType,
            IsLimitedTime = session.IsLimitedTime,
            IsLimitedSessionLaps = session.IsLimitedSessionLaps,
            CurrentLap = telemetry.Lap,
            TotalLaps = session._SessionLaps,
            Incidents = Math.Max(telemetry.PlayerCarDriverIncidentCount, 0),
            MaxIncidents = Math.Max(Math.Min(sessionData!.WeekendInfo.WeekendOptions._IncidentLimit, 999), 0),
            SessionTimeRemaining = (float)telemetry.SessionTimeRemain,
            Position = telemetry.PlayerCarPosition,
            StrengthOfField = telemetry.RaceCars.Count() > 1 ? (int)Math.Round(telemetry.RaceCars.Average(r => r.Details.Driver.IRating)) : 0,
            CurrentLapTime = telemetry.LapCurrentLapTime,
            LastLapTime = Math.Max(telemetry.LapLastLapTime, 0),
            LastLapTimeDelta = telemetry.LapLastLapTime <= 0 ? 0 : telemetry.LapDeltaToSessionLastlLap,
            BestLapTime = Math.Max(telemetry.LapBestLapTime, 0),
            BestLapTimeDelta = telemetry.LapBestLapTime <= 0 ? 0 : telemetry.LapDeltaToSessionBestLap,
            Gear = telemetry.Gear,
            Rpm = (int)telemetry.RPM,
            Speed = (int)Math.Round(telemetry.Speed * 3.6),
            BrakePct = (int)Math.Round(telemetry.Brake * 100, 0),
            ThrottlePct = (int)Math.Round(telemetry.Throttle * 100, 0),
            BrakeBias = telemetry.DcBrakeBias,
            FuelRemaining = telemetry.FuelLevel,
            AirTemp = telemetry.AirTemp,
            TrackTemp = telemetry.TrackTemp,
            Flag = GetFlag(telemetry.SessionFlags),
            PitLimiterOn = telemetry.EngineWarnings.HasFlag(EngineWarnings.PitSpeedLimiter),
        };

        return new DisplayUpdate{ Type = DisplayType.RaceDashboard, Data = displayUpdate };
    }

    private static string GetFlag(SessionFlags sessionFlags)
    {
        var flag = sessionFlags switch
        {
            SessionFlags.white => "white",
            SessionFlags.green or SessionFlags.greenHeld => "green",
            SessionFlags.yellow or SessionFlags.yellowWaving or SessionFlags.caution or SessionFlags.cautionWaving => "yellow",
            SessionFlags.red => "red",
            SessionFlags.blue => "blue",
            SessionFlags.black => "black",
            SessionFlags.repair => "black-orange",
            SessionFlags.debris => "red-yellow",
            SessionFlags.checkered => "checkered",
            _ => null,
        };

        if (flag is null)
        {
            Logger.Debug($"Unknown flag: {sessionFlags}");
            flag = "green";
        }

        return flag;
    }
}

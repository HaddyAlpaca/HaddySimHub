using System.Text;
using HaddySimHub.Models;
using HaddySimHub.Shared;
using iRacingSDK;
using static SCSSdkClient.Object.SCSTelemetry.Control;

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

        var timingEntries = new List<TimingEntry>();

        foreach (var driver in sessionData.DriverInfo.CompetingDrivers)
        {
            var carIdx = (int)driver.CarIdx;
            var carIdxLap = telemetry.CarIdxLap[carIdx];
            var carIdxLapDistPct = telemetry.CarIdxLapDistPct[carIdx];
            if (carIdxLap > _lastLaps[carIdx] && carIdxLapDistPct > 0.80f)
            {
                // The car has passed the start/finish line and the percentage is in the 80% range.
                // Set the percentage to 0% to avoid the bug in iRacing data stream.
                carIdxLapDistPct = 0;
            }

            _lastLaps[carIdx] = carIdxLap;

            // Set the license color
            // Rookie = Red
            // D = Orange
            // C = Yellow
            // B = Green
            // A = Blue
            // Pro = Purple
            var licenseColor = driver.LicString switch
            {
                "R" => "red",
                "D" => "orange",
                "C" => "yellow",
                "B" => "green",
                "A" => "blue",
                "P" => "purple",
                _ => "white"
            };

            // The license string is in the format R 02.0 remove the zero after the space
            var licenseString = System.Text.RegularExpressions.Regex.Replace(driver.LicString, @"(?<=\s)0", "");

            var entry = new TimingEntry
            {
                CarNumber = driver.CarNumber,
                DriverName = driver.UserName,
                Position = telemetry.CarIdxPosition[carIdx],
                Laps = carIdxLap,
                LapCompletedPct = (int)Math.Round(carIdxLapDistPct * 100, 0),
                License = driver.LicString,
                LicenseColor = licenseColor,
                IRating = driver.IRating,
                IsInPits = telemetry.CarIdxOnPitRoad[carIdx],
                IsPlayer = carIdx == telemetry.PlayerCarIdx,
                IsSafetyCar = carIdx == 0,
                TimeToPlayer = (float)Math.Round(telemetry.CarIdxEstTime[carIdx] - telemetry.CarIdxEstTime[telemetry.PlayerCarIdx], 1),
            };

            timingEntries.Add(entry);
        }

        if (session.IsRace)
        {
            var playerEntry = timingEntries.FirstOrDefault(e => e.IsPlayer);

            if (playerEntry != null)
            {
                timingEntries.Where(e => !e.IsSafetyCar && !e.IsPlayer).ForEach(e =>
                {
                    e.IsLapAhead = (e.Laps + e.LapCompletedPct) - (playerEntry.Laps + playerEntry.LapCompletedPct) > .8;
                    e.IsLapBehind = (playerEntry.Laps + playerEntry.LapCompletedPct) - (e.Laps + e.LapCompletedPct) > .8;
                });
            }
        }

        Console.Clear();
        var orderedEntries = timingEntries.OrderByDescending(e => e.TimeToPlayer).ToArray();

        foreach(var entry in orderedEntries)
        {
            Console.WriteLine($"#{entry.CarNumber} {entry.DriverName} - {entry.License} - {entry.LicenseColor} - {entry.IRating} - {entry.Laps} - {entry.LapCompletedPct}% - {entry.TimeToPlayer}");
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
            Gear = telemetry.Gear == -1 ? "R" : telemetry.Gear == 0 ? "N" : telemetry.Gear.ToString(),
            Rpm = (int)telemetry.RPM,
            RpmGreen = 6300, // Based on F4 manual: 1 green
            RpmRed = 6800, // Based on F4 manual: 1 red
            RpmMax = 7000, // Based on F4 manual: all flashing
            Speed = (int)Math.Round(telemetry.Speed * 3.6),
            BrakePct = (int)Math.Round(telemetry.Brake * 100, 0),
            ThrottlePct = (int)Math.Round(telemetry.Throttle * 100, 0),
            BrakeBias = telemetry.DcBrakeBias,
            FuelRemaining = telemetry.FuelLevel,
            AirTemp = telemetry.AirTemp,
            TrackTemp = telemetry.TrackTemp,
            Flag = GetFlag(telemetry.SessionFlags),
            PitLimiterOn = telemetry.EngineWarnings.HasFlag(EngineWarnings.PitSpeedLimiter),
            TimingEntries = orderedEntries,
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
            SessionFlags.black or SessionFlags.disqualify => "black",
            SessionFlags.repair => "black-orange",
            SessionFlags.debris => "red-yellow",
            SessionFlags.checkered => "checkered",
            _ => null,
        };

        if (flag is null)
        {
            // Ignore some flags, handle them as green
            if (sessionFlags.HasFlag(SessionFlags.servicible))
            {
                sessionFlags &= ~SessionFlags.servicible;
                flag = "green";
            }

            if (sessionFlags.HasFlag(SessionFlags.tenToGo))
            {
                sessionFlags &= ~SessionFlags.tenToGo;
                flag = "green";
            }

            if (sessionFlags.HasFlag(SessionFlags.fiveToGo))
            {
                sessionFlags &= ~SessionFlags.fiveToGo;
                flag = "green";
            }
        }

        if (flag is null)
        {
            Logger.Error($"Unknown flag: {sessionFlags}");
            flag = "green";
        }

        return flag;
    }
}

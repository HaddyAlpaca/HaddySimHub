using HaddySimHub.Models;
using HaddySimHub.Shared;
using iRacingSDK;

namespace HaddySimHub.Displays.IRacing;

internal sealed class Display() : DisplayBase<DataSample>()
{
    private static readonly HashSet<SessionFlags> loggedUnknownFlags = [];
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
        if (iRacing.IsConnected)
        {
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
        string CarScreenName = string.Empty;
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

            if (carIdx == telemetry.PlayerCarIdx)
            {
                CarScreenName = driver.CarScreenName;
            }

            // The license string is in the format R 02.0 remove the zero after the space
            var licenseString = System.Text.RegularExpressions.Regex.Replace(driver.LicString, @"(?<=\s)0", "");

            var entry = new TimingEntry
            {
                CarNumber = driver.CarNumber,
                DriverName = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(driver.UserName)),
                Position = telemetry.CarIdxPosition[carIdx],
                Laps = carIdxLap,
                LapCompletedPct = carIdxLapDistPct * 100,
                License = driver.LicString,
                LicenseColor = licenseColor,
                IRating = driver.IRating,
                IsInPits = telemetry.CarIdxOnPitRoad[carIdx],
                IsPlayer = carIdx == telemetry.PlayerCarIdx,
                IsSafetyCar = driver.UserName.ToLower() == "Pace Car",
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
                    var lapDiff = e.Laps - playerEntry.Laps;
                    var positionInLap = e.LapCompletedPct - playerEntry.LapCompletedPct;
                    
                    // Calculate total position including laps
                    var totalPosition = (lapDiff * 100) + positionInLap;
                    
                    // Car is ahead if total position is positive
                    e.IsLapAhead = totalPosition > 0;
                    
                    // Car is behind if total position is negative
                    e.IsLapBehind = totalPosition < 0;
                });
            }
        }

        var orderedEntries = timingEntries.OrderByDescending(e => e.TimeToPlayer).ToArray();

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
            BestLapTimeDelta = telemetry.LapDeltaToSessionBestLap <= 0 ? 0 : telemetry.LapDeltaToSessionBestLap,
            Gear = telemetry.Gear == -1 ? "R" : telemetry.Gear == 0 ? "N" : telemetry.Gear.ToString(),
            Rpm = (int)telemetry.RPM,
            RpmLights = [.. GenerateRpmLights(CarScreenName)],
            RpmMax = GetRpmMax(CarScreenName),
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

        return new DisplayUpdate { Type = DisplayType.RaceDashboard, Data = displayUpdate };
    }

    private static string GetFlag(SessionFlags sessionFlags)
    {
        // Map known flags to colors
        var flagMappings = new Dictionary<SessionFlags, string>
        {
            { SessionFlags.white, "white" },
            { SessionFlags.green, "green" },
            { SessionFlags.greenHeld, "green" },
            { SessionFlags.yellow, "yellow" },
            { SessionFlags.yellowWaving, "yellow" },
            { SessionFlags.caution, "yellow" },
            { SessionFlags.cautionWaving, "yellow" },
            { SessionFlags.red, "red" },
            { SessionFlags.blue, "blue" },
            { SessionFlags.black, "black" },
            { SessionFlags.disqualify, "black" },
            { SessionFlags.repair, "black-orange" },
            { SessionFlags.debris, "red-yellow" },
            { SessionFlags.checkered, "checkered" }
        };

        string? flag = null;
        foreach (var kvp in flagMappings)
        {
            if (sessionFlags.HasFlag(kvp.Key))
            {
                flag = kvp.Value;
                break;
            }
        }

        // Handle ignored flags generically
        var ignoredFlags = new[]
        {
            SessionFlags.servicible,
            SessionFlags.tenToGo,
            SessionFlags.fiveToGo,
            SessionFlags.oneLapToGreen,
            SessionFlags.startHidden,
            SessionFlags.startReady,
            SessionFlags.startSet,
            SessionFlags.startGo,
            SessionFlags.randomWaving,
            SessionFlags.furled,
            SessionFlags.crossed,
        };

        if (flag is null)
        {
            foreach (var ignored in ignoredFlags)
            {
                if (sessionFlags.HasFlag(ignored))
                {
                    sessionFlags &= ~ignored;
                    flag = "green";
                    break;
                }
            }
        }

        if (flag is null)
        {
            // Only log unknown flags once per unique value
            if (!loggedUnknownFlags.Contains(sessionFlags))
            {
                Logger.Error($"Unknown flag: {sessionFlags}");
                loggedUnknownFlags.Add(sessionFlags);
            }
            flag = "green";
        }

        return flag;
    }

    public static RpmLight[] GenerateRpmLights(string carName)
    {
        return carName switch
        {
            "FIA F4" => [
                new RpmLight { Rpm = 6300, Color = "Green" },
                new RpmLight { Rpm = 6500, Color = "Green" },
                new RpmLight { Rpm = 6600, Color = "Green" },
                new RpmLight { Rpm = 6700, Color = "Green" },
                new RpmLight { Rpm = 6800, Color = "Red" },
                new RpmLight { Rpm = 6900, Color = "Red" }
            ],
            _ => []
        };
    }

    public int GetRpmMax(string carName)
    {
        return carName switch
        {
            "FIA F4" => 7000,
            _ => 0, // Default value for other cars
        };
    }
}

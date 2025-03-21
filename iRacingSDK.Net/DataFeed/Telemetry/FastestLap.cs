﻿namespace iRacingSDK;

public class FastLap
{
    public SessionData._DriverInfo._Drivers Driver;
    public TimeSpan Time;

    public static bool operator ==(FastLap a, FastLap b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;
        
        return a.Driver == b.Driver && a.Time == b.Time;
    }

    public static bool operator !=(FastLap a, FastLap b) => !(a == b);

    public override bool Equals(object obj) => obj is FastLap lap && this == lap;

    public override int GetHashCode() => Driver.GetHashCode();
}

public partial class Telemetry : Dictionary<string, object>
{
    public FastLap FastestLap { get; set; }
}

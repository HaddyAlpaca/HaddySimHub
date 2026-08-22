using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.AC;

public enum ACStatus : int
{
    Off = 0,
    Replay = 1,
    Live = 2,
    Pause = 3,
}

public enum ACSessionType : int
{
    Unknown = -1,
    Practice = 0,
    Qualifying = 1,
    Race = 2,
    Hotlap = 3,
    TimeAttack = 4,
    Drift = 5,
    Drag = 6,
}

/// <summary>
/// Mirrors the leading part of <c>SPageFileGraphic</c> as published by Assetto
/// Corsa on <c>Local\acpmf_graphics</c>.
/// </summary>
/// <remarks>
/// The struct deliberately stops after <see cref="NormalizedCarPosition"/>. That is
/// where the layout starts to differ between Assetto Corsa releases and between
/// Assetto Corsa, Competizione and Rally — older builds carry a single set of car
/// coordinates where newer ones carry a grid of sixty. Only the bytes covered by
/// this struct are mapped, so stopping here keeps every field above it trustworthy.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
public struct ACGraphics
{
    public int PacketId;
    public ACStatus Status;
    public ACSessionType SessionType;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string CurrentTimeStr;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string LastTimeStr;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string BestTimeStr;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string LastSectorTimeStr;

    public int CompletedLaps;

    /// <summary>Race position, 1-based.</summary>
    public int Position;

    /// <summary>Elapsed time on the current lap in milliseconds.</summary>
    public int CurrentTime;

    public int LastTime;

    /// <summary>Best lap in milliseconds. Reads as <see cref="int.MaxValue"/> until a lap is set.</summary>
    public int BestTime;

    /// <summary>Session time left in milliseconds.</summary>
    public float SessionTimeLeft;

    public float DistanceTraveled;
    public int IsInPit;
    public int CurrentSectorIndex;
    public int LastSectorTime;
    public int NumberOfLaps;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]  // 33 chars + 1 char of 2-byte padding
    public string TyreCompound;

    public float ReplayTimeMultiplier;
    public float NormalizedCarPosition;
}

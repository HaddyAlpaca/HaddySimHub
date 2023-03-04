namespace HaddySimHub.Raceroom.Enums;

enum SessionLengthFormat
{
    // N/A
    Unavailable = -1,

    TimeBased = 0,

    LapBased = 1,

    // Time and lap based session means there will be an extra lap after the time has run out
    TimeAndLapBased = 2
};

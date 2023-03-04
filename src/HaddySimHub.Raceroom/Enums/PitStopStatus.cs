namespace HaddySimHub.Raceroom.Enums;

enum PitStopStatus
{
    // No mandatory pitstops
    Unavailable = -1,

    // Mandatory pitstop for two tyres not served yet
    UnservedTwoTyres = 0,

    // Mandatory pitstop for four tyres not served yet
    UnservedFourTyres = 1,

    // Mandatory pitstop served
    Served = 2,
};

namespace HaddySimHub.Raceroom.Enums;

enum FinishStatus
{
    // N/A
    Unavailable = -1,

    // Still on track, not finished
    None = 0,

    // Finished session normally
    Finished = 1,

    // Did not finish
    DNF = 2,

    // Did not qualify
    DNQ = 3,

    // Did not start
    DNS = 4,

    // Disqualified
    DQ = 5,
};

namespace HaddySimHub.Raceroom.Enums;

enum PitWindow
{
    Unavailable = -1,

    // Pit stops are not enabled for this session
    Disabled = 0,

    // Pit stops are enabled, but you're not allowed to perform one right now
    Closed = 1,

    // Allowed to perform a pit stop now
    Open = 2,

    // Currently performing the pit stop changes (changing driver, etc.)
    Stopped = 3,

    // After the current mandatory pitstop have been completed
    Completed = 4,
};

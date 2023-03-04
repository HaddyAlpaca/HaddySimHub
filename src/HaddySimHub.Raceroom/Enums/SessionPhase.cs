namespace HaddySimHub.Raceroom.Enums;

enum SessionPhase
{
    Unavailable = -1,

    // Currently in garage
    Garage = 1,

    // Gridwalk or track walkthrough
    Gridwalk = 2,

    // Formation lap, rolling start etc.
    Formation = 3,

    // Countdown to race is ongoing
    Countdown = 4,

    // Race is ongoing
    Green = 5,

    // End of session
    Checkered = 6,
};

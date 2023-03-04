namespace HaddySimHub.Raceroom.Enums;

enum PitMenuSelection
{
    // Pit menu unavailable
    Unavailable = -1,

    // Pit menu preset
    Preset = 0,

    // Pit menu actions
    Penalty = 1,
    Driverchange = 2,
    Fuel = 3,
    Fronttires = 4,
    Reartires = 5,
    Frontwing = 6,
    Rearwing = 7,
    Suspension = 8,

    // Pit menu buttons
    ButtonTop = 9,
    ButtonBottom = 10,

    // Pit menu nothing selected
    Max = 11
};

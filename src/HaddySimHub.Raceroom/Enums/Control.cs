namespace HaddySimHub.Raceroom.Enums;

enum Control
{
    Unavailable = -1,

    // Controlled by the actual player
    Player = 0,

    // Controlled by AI
    AI = 1,

    // Controlled by a network entity of some sort
    Remote = 2,

    // Controlled by a replay or ghost
    Replay = 3,
};

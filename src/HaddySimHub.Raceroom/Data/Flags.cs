using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Flags
{
    // Whether yellow flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public int Yellow;

    // Whether yellow flag was caused by current slot
    // -1 = no data
    //  0 = didn't cause it
    //  1 = caused it
    public int YellowCausedIt;

    // Whether overtake of car in front by current slot is allowed under yellow flag
    // -1 = no data
    //  0 = not allowed
    //  1 = allowed
    public int YellowOvertake;

    // Whether you have gained positions illegaly under yellow flag to give back
    // -1 = no data
    //  0 = no positions gained
    //  n = number of positions gained
    public int YellowPositionsGained;

    // Yellow flag for each sector; -1 = no data, 0 = not active, 1 = active
    public Sectors<int> SectorYellow;

    // Distance into track for closest yellow, -1.0 if no yellow flag exists
    // Unit: Meters (m)
    public float ClosestYellowDistanceIntoTrack;

    // Whether blue flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public int Blue;

    // Whether black flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public int Black;

    // Whether green flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public int Green;

    // Whether checkered flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public int Checkered;

    // Whether white flag is currently active
    // -1 = no data
    //  0 = not active
    //  1 = active
    public int White;

    // Whether black and white flag is currently active and reason
    // -1 = no data
    //  0 = not active
    //  1 = blue flag 1st warnings
    //  2 = blue flag 2nd warnings
    //  3 = wrong way
    //  4 = cutting track
    public int BlackAndWhite;
}

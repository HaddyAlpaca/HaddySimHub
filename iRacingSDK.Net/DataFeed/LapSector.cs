namespace iRacingSDK;

public struct LapSector(int lapNumber, int sector)
{
    public readonly int LapNumber = lapNumber;
    public readonly int Sector = sector;

    public static LapSector ForLap(int lapNumber) => new LapSector(lapNumber, 0);

    public override bool Equals(object obj) => obj is LapSector sector && this == sector;

    public override int GetHashCode() => LapNumber << 4 + Sector;

    public static bool operator ==(LapSector x, LapSector y) =>
        x.LapNumber == y.LapNumber && x.Sector == y.Sector;

    public static bool operator !=(LapSector x, LapSector y) => !(x == y);

    public static bool operator >=(LapSector x, LapSector y)
    {
        if (x.LapNumber > y.LapNumber)
            return true;

        if (x.LapNumber == y.LapNumber && x.Sector >= y.Sector)
            return true;

        return false;
    }

    public static bool operator <=(LapSector x, LapSector y) => y >= x;

    public override string ToString() => string.Format("Lap: {0}, Sector: {1}", LapNumber, Sector);
}

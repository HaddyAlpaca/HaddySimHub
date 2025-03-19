namespace iRacingSDK;

public partial class Telemetry : Dictionary<string, object>
	{
    int[] positions;
    public int[] Positions
    {
        get
        {
            if (positions != null)
                return positions;

            positions = new int[64];

            var runningOrder = CarIdxDistance
                .Select((d, idx) => new { CarIdx = idx, Distance = d })
                .Where(d => d.Distance > 0)
                .Where(c => c.CarIdx != 0)
                .OrderByDescending(c => c.Distance)
                .Select((c, order) => new { CarIdx = c.CarIdx, Position = order + 1, Distance = c.Distance})
                .ToList();

            var maxRunningOrderIndex = runningOrder.Count == 0 ? 0 : runningOrder.Max(ro => ro.CarIdx);
            var maxSessionIndex = this.SessionData.DriverInfo.CompetingDrivers.Length;

            positions = new int[ Math.Max(maxRunningOrderIndex, maxSessionIndex)+1 ];

            positions[0] = int.MaxValue;
            foreach( var runner in runningOrder )
                positions[runner.CarIdx] = runner.Position;

            var lastKnownPosition = (runningOrder.Count == 0 ? 0 : runningOrder.Max(ro => ro.Position)) + 1;
            for (var i = 0; i < positions.Length; i++)
                if (positions[i] == 0)
                    positions[i] = lastKnownPosition++;

            return positions;
        }
    }
}

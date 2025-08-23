using HaddySimHub.Models;

public static class IRatingCalculator
{
    private static double GetKFactor(int fieldSize) => 32.0 / (fieldSize - 1);

    public static (double delta, double expectedPosition) CalculateDelta(TimingEntry player, List<TimingEntry> allDrivers)
    {
        int n = allDrivers.Count;
        double K = GetKFactor(n);

        double expected = 0;
        double actual = 0;

        foreach (var opponent in allDrivers.Where(d => d != player))
        {
            // Elo expected score vs opponent
            double e = 1.0 / (1.0 + Math.Pow(10, (opponent.IRating - player.IRating) / 400.0));
            expected += e;

            // Actual score: 1 if finished ahead, 0 if behind
            actual += player.Position < opponent.Position ? 1 : 0;
        }

        double delta = K * (actual - expected);

        // Expected finishing position = field size - expected beaten + 1
        double expectedPos = (n - expected) + 1;

        return (delta, expectedPos);
    }
}
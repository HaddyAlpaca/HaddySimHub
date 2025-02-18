using GregsStack.InputSimulatorStandard;

namespace HaddyTruckSDPlugin;

internal static class Helpers
{
    private static readonly InputSimulator _inputSimulator = new();

    public static void SendKeys(string keys)
    {
        if (string.IsNullOrEmpty(keys))
            return;

        _inputSimulator.Keyboard.TextEntry(keys);
    }
}

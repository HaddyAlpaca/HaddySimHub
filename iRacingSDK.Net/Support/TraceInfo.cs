using System.Diagnostics;

namespace iRacingSDK.Support;

public static class TraceInfo
{
    const string Category = "INFO";

    public static void WriteLine(string value, params object[] args)
    {
        Trace.WriteLine(value.F(args), Category);
    }

    public static void WriteLineIf(bool condition, string value, params object[] args)
    {
        Trace.WriteLineIf(condition, value.F(args), Category);
    }
}

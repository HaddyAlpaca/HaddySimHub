using System.Diagnostics;

namespace iRacingSDK.Support;

public static class TraceError
{
    const string Category = "ERROR";

    public static void WriteLine(string value, params object[] args)
    {
        Trace.WriteLine(value.F(args), Category);
    }
}

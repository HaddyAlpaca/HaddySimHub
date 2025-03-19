namespace iRacingSDK.Support;

public static class StringExtensions
{
    public static string F(this string self, params object[] args) => string.Format(self, args);
}

namespace iRacingSDK;

public partial class SessionData
{
    public partial class _SessionInfo
    {
        public partial class _Sessions
        {
            public int _SessionLaps
            {
                get
                {
                    int.TryParse(SessionLaps, out int result);
                    return result;
                }
            }

            public double _SessionTime
            {
                get
                {
                    double.TryParse(SessionTime.Replace(" sec", ""), out double result);
                    return result;
                }
            }

            public bool IsLimitedSessionLaps => SessionLaps.ToLower() != "unlimited";

            public bool IsLimitedTime => SessionTime.ToLower() != "unlimited";
        }
    }
}

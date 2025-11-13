using HaddySimHub.Models;

namespace HaddySimHub.Displays.Dirt2
{
    public class TestDisplay(string name) : TestDisplayBase(name)
    {
        protected override DisplayUpdate GenerateDisplayUpdate()
        {
            int rpmMax = 7000;

            return new DisplayUpdate
            {
                Type = DisplayType.RallyDashboard,
                Data = new RallyData
                {
                    Speed = (short)DateTime.Now.Second,
                    CompletedPct = (short)DateTime.Now.Second,
                    DistanceTravelled = (short)DateTime.Now.Millisecond,
                    Gear = new Random().Next(1, 6).ToString(),
                    Rpm = new Random().Next(0, 10000),
                    RpmLights = Display.GenerateRpmLights(rpmMax),
                    RpmMax = rpmMax,
                    LapTime = new Random().Next(0, 100),
                    Sector1Time = new Random().Next(0, 100),
                    Sector2Time = new Random().Next(0, 100),
                }
            };
        }
    }
}

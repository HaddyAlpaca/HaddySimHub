using HaddySimHub.Models;
using HaddySimHub.Interfaces;
using HaddySimHub.Services;

namespace HaddySimHub.Displays.Dirt2
{
    public class TestDisplay : TestDisplayBase
    {
        public TestDisplay(
            string id, 
            IDataConverter<DisplayUpdate, DisplayUpdate> identityDataConverter, 
            IDisplayUpdateSender displayUpdateSender) 
            : base(id, identityDataConverter, displayUpdateSender)
        {
        }

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
                    Gear = _random.Next(1, 6).ToString(),
                    Rpm = _random.Next(0, 10000),
                    RpmLights = Dirt2DataConverter.GenerateRpmLights(rpmMax), // Use the static method from Dirt2DataConverter
                    RpmMax = rpmMax,
                    LapTime = _random.Next(0, 100),
                    Sector1Time = _random.Next(0, 100),
                    Sector2Time = _random.Next(0, 100),
                }
            };
        }
    }
}

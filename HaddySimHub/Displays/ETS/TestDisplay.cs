using HaddySimHub.Models;
using HaddySimHub.Interfaces;
using HaddySimHub.Services;

namespace HaddySimHub.Displays.ETS
{
    public class TestDisplay : TestDisplayBase
    {
        private const int RpmMax = 3000;
        private const int RpmStep = 100;
        private int _currentRpm = 0;

        public TestDisplay(
            string id,
            IDataConverter<DisplayUpdate, DisplayUpdate> identityDataConverter,
            IDisplayUpdateSender displayUpdateSender)
            : base(id, identityDataConverter, displayUpdateSender)
        {
        }

        protected override DisplayUpdate GenerateDisplayUpdate()
        {
            _currentRpm += RpmStep;
            if (_currentRpm > RpmMax)
            {
                _currentRpm = 0;
            }

            var speed = (short)(_currentRpm * 0.08);
            var recommendedGear = _currentRpm switch
            {
                > 2000 => "6",
                < 1000 => "4",
                _ => string.Empty,
            };

            return new DisplayUpdate
            {
                Type = DisplayType.TruckDashboard,
                Data = new TruckData
                {
                    Speed = speed,
                    Gear = "5",
                    RecommendedGear = recommendedGear,
                    Rpm = (short)_currentRpm,
                    RpmMax = RpmMax,
                    FuelAverageConsumption = 25.5F,
                    FuelAmount = 300,
                    FuelCapacity = 800,
                    FuelDistance = (int)(300 / 25.5 * 100),
                    AdBlueAmount = 60,
                    AdBlueCapacity = 80,
                    SpeedLimit = 80,
                    EngineOn = true,
                    BrakeTemp = 120,
                    BrakeAirPressure = 120,
                }
            };
        }
    }
}

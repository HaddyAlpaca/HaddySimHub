using HaddySimHub.Models;
using HaddySimHub.Interfaces;
using HaddySimHub.Services;

namespace HaddySimHub.Displays.ETS
{
    public class TestDisplay : TestDisplayBase
    {
        private bool _parkingBrakeOn = false;

        public TestDisplay(
            string id,
            IDataConverter<DisplayUpdate, DisplayUpdate> identityDataConverter,
            IDisplayUpdateSender displayUpdateSender)
            : base(id, identityDataConverter, displayUpdateSender)
        {
        }

        protected override DisplayUpdate GenerateDisplayUpdate()
        {
            this._parkingBrakeOn = !this._parkingBrakeOn;
            return new DisplayUpdate
            {
                Type = DisplayType.TruckDashboard,
                Data = new TruckData
                {
                    Speed = (short)DateTime.Now.Second,
                    Gear = _random.Next(1, 12).ToString(),
                    Rpm = (short)_random.Next(0, 2400),
                    RpmMax = 2200,
                    SpeedLimit = (short)_random.Next(0, 90),
                    ParkingBrakeOn = this._parkingBrakeOn,
                    RetarderLevel = (uint)_random.Next(0, 5),
                    RetarderStepCount = 5,
                }
            };
        }
    }
}

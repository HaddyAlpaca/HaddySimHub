using HaddySimHub.Models;

namespace HaddySimHub.Displays.ETS
{
    internal class TestDisplay(string name) : TestDisplayBase(name)
    {
        private bool _parkingBrakeOn = false;

        protected override DisplayUpdate GenerateDisplayUpdate()
        {
            this._parkingBrakeOn = !this._parkingBrakeOn;
            return new DisplayUpdate
            {
                Type = DisplayType.TruckDashboard,
                Data = new TruckData
                {
                    Speed = (short)DateTime.Now.Second,
                    Gear = new Random().Next(1, 12).ToString(),
                    Rpm = (short)new Random().Next(0, 2400),
                    RpmMax = 2200,
                    SpeedLimit = (short)new Random().Next(0, 90),
                    ParkingBrakeOn = this._parkingBrakeOn,
                    RetarderLevel = (uint)new Random().Next(0, 5),
                    RetarderStepCount = 5,
                }
            };
        }
    }
}

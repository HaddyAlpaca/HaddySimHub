using HaddySimHub.Telemetry;
using HaddySimHub.Telemetry.Models;
using System.Text;

namespace HaddySimHub.Ets2;

public sealed class TelemetryReader : ITelemetryReader, IDisposable 
{
    /// <summary>
    /// ETS2 telemetry plugin maps the data to this mapped file name.
    /// </summary>
    private readonly ISharedMemoryReader<Datastruct> sharedMemory;

    public string ProcessName => "eurotrucks2";

    public TelemetryReader(ISharedMemoryReaderFactory sharedMemoryReaderFactory)
    {
        this.sharedMemory = sharedMemoryReaderFactory.Create<Datastruct>("Local\\Ets2TelemetryServer");
    }

    public void Dispose() => sharedMemory?.Dispose();

    public object ReadTelemetry()
    {
        //Read the current data from ETS2
        Datastruct rawData = sharedMemory.Read();

        if (rawData.ets2_telemetry_plugin_revision != 0 && rawData.timeAbsolute != 0)
        {
            //Convert the data to the generic format
            string destination = BytesToString(rawData.jobCityDestination);
            string destinationCompany = BytesToString(rawData.jobCompanyDestination);
            if (!string.IsNullOrEmpty(destinationCompany))
                destination += $" ({destinationCompany})";

            return new TruckData()
            {
                Destination = destination,
                DistanceRemaining = (int)rawData.navigationDistance,
                TimeRemaining = rawData.jobDeadline - rawData.timeAbsolute,
                JobIncome = rawData.jobIncome,
                JobTimeRemaining = rawData.jobDeadline,
                Gear = (short)rawData.gear,
                GearRange = rawData.gearRangeActive == 1 ? GearRange.Low : GearRange.High,
                Rpm = (int)rawData.engineRpm,
                RpmMax = (int)rawData.engineRpmMax,
                Speed = MetersPerSecondToKmPerHour(rawData.speed),
                SpeedLimit = MetersPerSecondToKmPerHour(rawData.navigationSpeedLimit),
                CruiseControlOn = rawData.cruiseControl == 1,
                CruiseControlSpeed = MetersPerSecondToKmPerHour(rawData.cruiseControlSpeed),
                LowBeamOn = rawData.lightsBeamLow == 1,
                HighBeamOn = rawData.lightsBeamHigh == 1,
                ParkingBrakeOn = rawData.parkBrake == 1,
                BatteryWarningOn = rawData.batteryVoltageWarning == 1,
            };
        }
        else
        {
            //Return empty object
            return new TruckData();
        }
    }

    private static DateTime SecondsToDate(uint seconds) => 
        new((long)Math.Max(seconds, 0) * 10000000, DateTimeKind.Utc);

    private static short MetersPerSecondToKmPerHour(float ms) =>
        Convert.ToInt16(ms * 3.6);

    private static string BytesToString(byte[] bytes) =>
        bytes == null ? string.Empty : Encoding.UTF8.GetString(bytes, 0, Array.FindIndex(bytes, b => b == 0));
}
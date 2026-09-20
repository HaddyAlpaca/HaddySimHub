using HaddySimHub.Displays.ETS;
using HaddySimHub.Models;
using SCSSdkClient.Object;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class EtsDataConverterTests
    {
        #region Gear Tests

        [TestMethod]
        public void Convert_GearNeutral()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(selectedGear: 0, forwardGearCount: 12));
            Assert.AreEqual("N", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearReverse()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(selectedGear: -1, forwardGearCount: 12));
            Assert.AreEqual("R1", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearMultipleReverse()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(selectedGear: -2, forwardGearCount: 12));
            Assert.AreEqual("R2", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearForwardNonEuro()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(selectedGear: 5, forwardGearCount: 8));
            Assert.AreEqual("5", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearEuro14Gears_C1()
        {
            // Arrange - In Euro trucks with 14 gears, gear 1 is displayed as "C1"
            // Act
            var truckData = Convert(CreateMockTelemetry(selectedGear: 1, forwardGearCount: 14));
            Assert.AreEqual("C1", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearEuro14Gears_Offset()
        {
            // Arrange - Gear 3 becomes "1" in the display (3-2=1)
            // Act
            var truckData = Convert(CreateMockTelemetry(selectedGear: 3, forwardGearCount: 14));
            Assert.AreEqual("1", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearEuro14Gears_High()
        {
            // Arrange - Gear 14 becomes "12" in the display (14-2=12)
            // Act
            var truckData = Convert(CreateMockTelemetry(selectedGear: 14, forwardGearCount: 14));
            Assert.AreEqual("12", truckData.Gear);
        }

        [TestMethod]
        public void Convert_RecommendedGear_PicksHigherGearAtCruisingSpeed()
        {
            // Arrange - at 20 m/s the lowest-revving drivable gear is closest to the economy target.
            // Act
            var truckData = Convert(CreateMockTelemetry(
                selectedGear: 2,
                forwardGearCount: 4,
                speed: 20,
                rpmMax: 2500,
                forwardRatios: [4f, 3f, 2f, 1f],
                differential: 3f,
                wheelRadius: 0.5f));
            Assert.AreEqual("4", truckData.RecommendedGear);
        }

        [TestMethod]
        public void Convert_RecommendedGear_PicksLowerGearAtLowSpeed()
        {
            // Arrange - at 8 m/s a lower gear is needed to stay near the economy target.
            // Act
            var truckData = Convert(CreateMockTelemetry(
                selectedGear: 5,
                forwardGearCount: 4,
                speed: 8,
                rpmMax: 2500,
                forwardRatios: [4f, 3f, 2f, 1f],
                differential: 3f,
                wheelRadius: 0.5f));
            Assert.AreEqual("2", truckData.RecommendedGear);
        }

        [TestMethod]
        public void Convert_RecommendedGear_EmptyWhenBelowMinimumSpeed()
        {
            // Arrange - 2 m/s (7.2 km/h) is below the advice threshold.
            // Act
            var truckData = Convert(CreateMockTelemetry(
                selectedGear: 1,
                forwardGearCount: 4,
                speed: 2,
                rpmMax: 2500,
                forwardRatios: [4f, 3f, 2f, 1f],
                differential: 3f,
                wheelRadius: 0.5f));
            Assert.AreEqual(string.Empty, truckData.RecommendedGear);
        }

        [TestMethod]
        public void Convert_RecommendedGear_EmptyWhenTransmissionDataMissing()
        {
            // Arrange - no gear ratios, differential or wheel radius available.
            // Act
            var truckData = Convert(CreateMockTelemetry(selectedGear: 3, forwardGearCount: 4, speed: 20, rpmMax: 2500));
            Assert.AreEqual(string.Empty, truckData.RecommendedGear);
        }

        #endregion

        #region Speed Tests

        [TestMethod]
        public void Convert_SpeedPositive()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(speed: 85.0 / 3.6));
            Assert.AreEqual(85, truckData.Speed);
        }

        [TestMethod]
        public void Convert_SpeedNegativeClamped()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(speed: -10.0));
            Assert.AreEqual(0, truckData.Speed);
        }

        [TestMethod]
        public void Convert_SpeedLimit()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(speedLimit: 90.0 / 3.6));
            Assert.AreEqual(90, truckData.SpeedLimit);
        }

        #endregion

        #region Damage Tests

        [TestMethod]
        public void Convert_DamageCabin()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(cabinDamage: 0.25f));
            Assert.AreEqual(25, truckData.DamageTruckCabin);
        }

        [TestMethod]
        public void Convert_DamageEngine()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(engineDamage: 0.75f));
            Assert.AreEqual(75, truckData.DamageTruckEngine);
        }

        #endregion

        #region Fuel Tests

        [TestMethod]
        public void Convert_FuelAverageConsumption()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(fuelAverageConsumption: 0.25f));
            Assert.AreEqual(25.0f, truckData.FuelAverageConsumption);
        }

        [TestMethod]
        public void Convert_FuelAmount()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(fuelAmount: 500f));
            Assert.AreEqual(500f, truckData.FuelAmount);
        }

        [TestMethod]
        public void Convert_FuelDistance()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(fuelDistance: 1500f));
            Assert.AreEqual(1500f, truckData.FuelDistance);
        }

        #endregion

        #region Light Tests

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_ParkingLights(bool parkingLightsOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(parkingLights: parkingLightsOn));
            Assert.AreEqual(parkingLightsOn, truckData.ParkingLightsOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_LowBeam(bool lowBeamOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(lowBeam: lowBeamOn));
            Assert.AreEqual(lowBeamOn, truckData.LowBeamOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_HighBeam(bool highBeamOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(highBeam: highBeamOn));
            Assert.AreEqual(highBeamOn, truckData.HighBeamOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_HazardLights(bool hazardLightsOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(hazardLights: hazardLightsOn));
            Assert.AreEqual(hazardLightsOn, truckData.HazardLightsOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_BlinkerLeft(bool blinkerLeftOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(blinkerLeft: blinkerLeftOn));
            Assert.AreEqual(blinkerLeftOn, truckData.BlinkerLeftOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_BlinkerRight(bool blinkerRightOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(blinkerRight: blinkerRightOn));
            Assert.AreEqual(blinkerRightOn, truckData.BlinkerRightOn);
        }

        #endregion

        #region Brake Tests

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_ParkingBrake(bool parkingBrakeOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(parkingBrake: parkingBrakeOn));
            Assert.AreEqual(parkingBrakeOn, truckData.ParkingBrakeOn);
        }

        [TestMethod]
        public void Convert_RetarderLevel()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(retarderLevel: 3, retarderStepCount: 5));
            Assert.AreEqual(3u, truckData.RetarderLevel);
            Assert.AreEqual(5u, truckData.RetarderStepCount);
        }

        #endregion

        #region Throttle Tests

        [TestMethod]
        public void Convert_Throttle75Percent()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(throttle: 0.75));
            Assert.AreEqual(75, truckData.Throttle);
        }

        [TestMethod]
        public void Convert_ThrottleMinimum()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(throttle: 0.0));
            Assert.AreEqual(0, truckData.Throttle);
        }

        [TestMethod]
        public void Convert_ThrottleMaximum()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(throttle: 1.0));
            Assert.AreEqual(100, truckData.Throttle);
        }

        #endregion

        #region Temperature and Pressure Tests

        [TestMethod]
        public void Convert_OilTemperature()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(oilTemp: 85.5f));
            Assert.AreEqual(85.5f, truckData.OilTemp);
        }

        [TestMethod]
        public void Convert_WaterTemperature()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(waterTemp: 95.0f));
            Assert.AreEqual(95.0f, truckData.WaterTemp);
        }

        [TestMethod]
        public void Convert_OilPressure()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(oilPressure: 4.5f));
            Assert.AreEqual(4.5f, truckData.OilPressure);
        }

        [TestMethod]
        public void Convert_BatteryVoltage()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(batteryVoltage: 13.5f));
            Assert.AreEqual(13.5f, truckData.BatteryVoltage);
        }

        #endregion

        #region Warning Tests

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_FuelWarning(bool fuelWarningOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(fuelWarning: fuelWarningOn));
            Assert.AreEqual(fuelWarningOn, truckData.FuelWarningOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_AdBlueWarning(bool adBlueWarningOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(adBlueWarning: adBlueWarningOn));
            Assert.AreEqual(adBlueWarningOn, truckData.AdBlueWarningOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_OilPressureWarning(bool oilPressureWarningOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(oilPressureWarning: oilPressureWarningOn));
            Assert.AreEqual(oilPressureWarningOn, truckData.OilPressureWarningOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_WaterTempWarning(bool waterTempWarningOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(waterTempWarning: waterTempWarningOn));
            Assert.AreEqual(waterTempWarningOn, truckData.WaterTempWarningOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_BatteryVoltageWarning(bool batteryVoltageWarningOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(batteryVoltageWarning: batteryVoltageWarningOn));
            Assert.AreEqual(batteryVoltageWarningOn, truckData.BatteryVoltageWarningOn);
        }

        #endregion

        #region Job Data Tests

        [TestMethod]
        public void Convert_JobIncome()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(jobIncome: 50000));
            Assert.AreEqual(50000u, truckData.JobIncome);
        }

        [TestMethod]
        public void Convert_CargoMass()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(cargoMass: 12500.7));
            Assert.AreEqual(12501, truckData.JobCargoMass);
        }

        [TestMethod]
        public void Convert_CargoDamage()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(cargoDamage: 0.15f));
            Assert.AreEqual(15, truckData.JobCargoDamage);
        }

        [TestMethod]
        public void Convert_JobCityAndCompany()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(
                sourceCity: "Berlin",
                sourceCompany: "Company A",
                destCity: "Paris",
                destCompany: "Company B"));
            Assert.AreEqual("Berlin", truckData.SourceCity);
            Assert.AreEqual("Company A", truckData.SourceCompany);
            Assert.AreEqual("Paris", truckData.DestinationCity);
            Assert.AreEqual("Company B", truckData.DestinationCompany);
        }

        #endregion

        #region Navigation Tests

        [TestMethod]
        public void Convert_DistanceRemaining()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(navDistance: 50000));
            Assert.AreEqual(50, truckData.DistanceRemaining);
        }

        [TestMethod]
        public void Convert_TimeRemaining()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(navTime: 3600));
            Assert.AreEqual(60, truckData.TimeRemaining);
        }

        [TestMethod]
        public void Convert_RestTimeRemaining()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(restTimeRemaining: 120));
            Assert.AreEqual(120, truckData.RestTimeRemaining);
        }

        #endregion

        #region Cruise Control Tests

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_CruiseControl(bool cruiseControlOn)
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(cruiseControl: cruiseControlOn));
            Assert.AreEqual(cruiseControlOn, truckData.CruiseControlOn);
        }

        [TestMethod]
        public void Convert_CruiseControlSpeed()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(cruiseControlSpeed: 85 / 3.6));
            Assert.AreEqual(85, truckData.CruiseControlSpeed);
        }

        #endregion

        #region RPM Tests

        [TestMethod]
        public void Convert_RPM()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(rpm: 2500, rpmMax: 2800));
            Assert.AreEqual(2500, truckData.Rpm);
            Assert.AreEqual(2800, truckData.RpmMax);
        }

        #endregion

        #region Odometer Tests

        [TestMethod]
        public void Convert_Odometer()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(odometer: 123456.7f));
            Assert.AreEqual(123456.7f, truckData.Odometer);
        }

        #endregion

        #region Dashboard Backlight Tests

        [TestMethod]
        public void Convert_DashboardBacklight()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry(dashboardBacklight: 0.75f));
            Assert.AreEqual(0.75f, truckData.DashboardBacklight);
        }

        [TestMethod]
        public void Convert_DashboardBacklight_DefaultZero()
        {
            // Act
            var truckData = Convert(CreateMockTelemetry());
            Assert.AreEqual(0f, truckData.DashboardBacklight);
        }

        #endregion

        #region Helper Methods

        private static TruckData Convert(SCSTelemetry data)
        {
            var truckData = new EtsDataConverter().Convert(data).Data as TruckData;
            Assert.IsNotNull(truckData);
            return truckData;
        }

        private SCSTelemetry CreateMockTelemetry(
            int selectedGear = 0,
            int forwardGearCount = 12,
            double speed = 0,
            double speedLimit = 0,
            float cabinDamage = 0,
            float engineDamage = 0,
            float fuelAverageConsumption = 0,
            float fuelAmount = 0,
            float fuelDistance = 0,
            bool parkingLights = false,
            bool lowBeam = false,
            bool highBeam = false,
            bool hazardLights = false,
            bool blinkerLeft = false,
            bool blinkerRight = false,
            bool parkingBrake = false,
            uint retarderLevel = 0,
            uint retarderStepCount = 0,
            double throttle = 0,
            float oilTemp = 0,
            float waterTemp = 0,
            float oilPressure = 0,
            float batteryVoltage = 0,
            bool fuelWarning = false,
            bool adBlueWarning = false,
            bool oilPressureWarning = false,
            bool waterTempWarning = false,
            bool batteryVoltageWarning = false,
            ulong jobIncome = 0,
            double cargoMass = 0,
            float cargoDamage = 0,
            string sourceCity = "",
            string sourceCompany = "",
            string destCity = "",
            string destCompany = "",
            double navDistance = 0,
            double navTime = 0,
            int restTimeRemaining = 0,
            bool cruiseControl = false,
            double cruiseControlSpeed = 0,
            int rpm = 0,
            int rpmMax = 0,
            float[]? forwardRatios = null,
            float differential = 0,
            float wheelRadius = 0,
            float odometer = 0,
            float dashboardBacklight = 0)
        {
            return new SCSTelemetryBuilder()
                .WithScale(1f)
                .WithGameTime(0)
                .WithNextRestStop(restTimeRemaining)
                .WithNavigation(navDistance, navTime, speedLimit)
                .WithJob(sourceCity, sourceCompany, destCity, destCompany, jobIncome, cargoMass, cargoDamage)
                .WithTruckConstants(forwardGearCount: forwardGearCount, engineRpmMax: rpmMax, retarderStepCount: retarderStepCount)
                .WithDashboard(speed: speed, cruiseControl: cruiseControl, cruiseSpeed: cruiseControlSpeed, fuelAvg: fuelAverageConsumption, fuelAmount: fuelAmount, fuelRange: fuelDistance, oilPressure: oilPressure, oilTemp: oilTemp, waterTemp: waterTemp, batteryVoltage: batteryVoltage, rpm: rpm, odometer: odometer)
                .WithWarnings(fuelWarning, adBlueWarning, oilPressureWarning, waterTempWarning, batteryVoltageWarning)
                .WithLights(parkingLights, lowBeam, highBeam, hazardLights, blinkerLeft, blinkerRight)
                .WithDashboardBacklight(dashboardBacklight)
                .WithMotor(selectedGear, parkingBrake, retarderLevel)
                .WithTransmission(forwardRatios ?? [], differential, wheelRadius)
                .WithDamage(cabinDamage, engineDamage)
                .WithControl(throttle)
                .WithTrailerEmpty()
                .Build();
        }

        #endregion
    }


}

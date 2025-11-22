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
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(selectedGear: 0, forwardGearCount: 12);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual("N", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearReverse()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(selectedGear: -1, forwardGearCount: 12);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual("R1", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearMultipleReverse()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(selectedGear: -2, forwardGearCount: 12);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual("R2", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearForwardNonEuro()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(selectedGear: 5, forwardGearCount: 8);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual("5", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearEuro14Gears_C1()
        {
            // Arrange - In Euro trucks with 14 gears, gear 1 is displayed as "C1"
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(selectedGear: 1, forwardGearCount: 14);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual("C1", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearEuro14Gears_Offset()
        {
            // Arrange - Gear 3 becomes "1" in the display (3-2=1)
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(selectedGear: 3, forwardGearCount: 14);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual("1", truckData.Gear);
        }

        [TestMethod]
        public void Convert_GearEuro14Gears_High()
        {
            // Arrange - Gear 14 becomes "12" in the display (14-2=12)
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(selectedGear: 14, forwardGearCount: 14);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual("12", truckData.Gear);
        }

        #endregion

        #region Speed Tests

        [TestMethod]
        public void Convert_SpeedPositive()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(speed: 85.0 / 3.6);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(85, truckData.Speed);
        }

        [TestMethod]
        public void Convert_SpeedNegativeClamped()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(speed: -10.0);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(0, truckData.Speed);
        }

        [TestMethod]
        public void Convert_SpeedLimit()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(speedLimit: 90.0 / 3.6);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(90, truckData.SpeedLimit);
        }

        #endregion

        #region Damage Tests

        [TestMethod]
        public void Convert_DamageCabin()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(cabinDamage: 0.25f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(25, truckData.DamageTruckCabin);
        }

        [TestMethod]
        public void Convert_DamageEngine()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(engineDamage: 0.75f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(75, truckData.DamageTruckEngine);
        }

        #endregion

        #region Fuel Tests

        [TestMethod]
        public void Convert_FuelAverageConsumption()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(fuelAverageConsumption: 0.25f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(25.0f, truckData.FuelAverageConsumption);
        }

        [TestMethod]
        public void Convert_FuelAmount()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(fuelAmount: 500f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(500f, truckData.FuelAmount);
        }

        [TestMethod]
        public void Convert_FuelDistance()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(fuelDistance: 1500f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(1500f, truckData.FuelDistance);
        }

        #endregion

        #region Light Tests

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_ParkingLights(bool parkingLightsOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(parkingLights: parkingLightsOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(parkingLightsOn, truckData.ParkingLightsOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_LowBeam(bool lowBeamOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(lowBeam: lowBeamOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(lowBeamOn, truckData.LowBeamOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_HighBeam(bool highBeamOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(highBeam: highBeamOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(highBeamOn, truckData.HighBeamOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_HazardLights(bool hazardLightsOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(hazardLights: hazardLightsOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(hazardLightsOn, truckData.HazardLightsOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_BlinkerLeft(bool blinkerLeftOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(blinkerLeft: blinkerLeftOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(blinkerLeftOn, truckData.BlinkerLeftOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_BlinkerRight(bool blinkerRightOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(blinkerRight: blinkerRightOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(blinkerRightOn, truckData.BlinkerRightOn);
        }

        #endregion

        #region Brake Tests

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_ParkingBrake(bool parkingBrakeOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(parkingBrake: parkingBrakeOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(parkingBrakeOn, truckData.ParkingBrakeOn);
        }

        [TestMethod]
        public void Convert_RetarderLevel()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(retarderLevel: 3, retarderStepCount: 5);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(3u, truckData.RetarderLevel);
            Assert.AreEqual(5u, truckData.RetarderStepCount);
        }

        #endregion

        #region Throttle Tests

        [TestMethod]
        public void Convert_Throttle75Percent()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(throttle: 0.75);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(75, truckData.Throttle);
        }

        [TestMethod]
        public void Convert_ThrottleMinimum()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(throttle: 0.0);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(0, truckData.Throttle);
        }

        [TestMethod]
        public void Convert_ThrottleMaximum()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(throttle: 1.0);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(100, truckData.Throttle);
        }

        #endregion

        #region Temperature and Pressure Tests

        [TestMethod]
        public void Convert_OilTemperature()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(oilTemp: 85.5f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(85.5f, truckData.OilTemp);
        }

        [TestMethod]
        public void Convert_WaterTemperature()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(waterTemp: 95.0f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(95.0f, truckData.WaterTemp);
        }

        [TestMethod]
        public void Convert_OilPressure()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(oilPressure: 4.5f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(4.5f, truckData.OilPressure);
        }

        [TestMethod]
        public void Convert_BatteryVoltage()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(batteryVoltage: 13.5f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(13.5f, truckData.BatteryVoltage);
        }

        #endregion

        #region Warning Tests

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_FuelWarning(bool fuelWarningOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(fuelWarning: fuelWarningOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(fuelWarningOn, truckData.FuelWarningOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_AdBlueWarning(bool adBlueWarningOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(adBlueWarning: adBlueWarningOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(adBlueWarningOn, truckData.AdBlueWarningOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_OilPressureWarning(bool oilPressureWarningOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(oilPressureWarning: oilPressureWarningOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(oilPressureWarningOn, truckData.OilPressureWarningOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_WaterTempWarning(bool waterTempWarningOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(waterTempWarning: waterTempWarningOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(waterTempWarningOn, truckData.WaterTempWarningOn);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_BatteryVoltageWarning(bool batteryVoltageWarningOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(batteryVoltageWarning: batteryVoltageWarningOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(batteryVoltageWarningOn, truckData.BatteryVoltageWarningOn);
        }

        #endregion

        #region Job Data Tests

        [TestMethod]
        public void Convert_JobIncome()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(jobIncome: 50000);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(50000u, truckData.JobIncome);
        }

        [TestMethod]
        public void Convert_CargoMass()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(cargoMass: 12500.7);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(12501, truckData.JobCargoMass);
        }

        [TestMethod]
        public void Convert_CargoDamage()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(cargoDamage: 0.15f);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(15, truckData.JobCargoDamage);
        }

        [TestMethod]
        public void Convert_JobCityAndCompany()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(
                sourceCity: "Berlin",
                sourceCompany: "Company A",
                destCity: "Paris",
                destCompany: "Company B");

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
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
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(navDistance: 50000);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(50, truckData.DistanceRemaining);
        }

        [TestMethod]
        public void Convert_TimeRemaining()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(navTime: 3600);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(60, truckData.TimeRemaining);
        }

        [TestMethod]
        public void Convert_RestTimeRemaining()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(restTimeRemaining: 120);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(120, truckData.RestTimeRemaining);
        }

        #endregion

        #region Cruise Control Tests

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Convert_CruiseControl(bool cruiseControlOn)
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(cruiseControl: cruiseControlOn);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(cruiseControlOn, truckData.CruiseControlOn);
        }

        [TestMethod]
        public void Convert_CruiseControlSpeed()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(cruiseControlSpeed: 85 / 3.6);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(85, truckData.CruiseControlSpeed);
        }

        #endregion

        #region RPM Tests

        [TestMethod]
        public void Convert_RPM()
        {
            // Arrange
            var converter = new EtsDataConverter();
            var data = CreateMockTelemetry(rpm: 2500, rpmMax: 2800);

            // Act
            var update = converter.Convert(data);
            var truckData = update.Data as TruckData;

            // Assert
            Assert.IsNotNull(truckData);
            Assert.AreEqual(2500, truckData.Rpm);
            Assert.AreEqual(2800, truckData.RpmMax);
        }

        #endregion

        #region Helper Methods

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
            int rpmMax = 0)
        {
            return new SCSTelemetryBuilder()
                .WithScale(1f)
                .WithGameTime(0)
                .WithNextRestStop(restTimeRemaining)
                .WithNavigation(navDistance, navTime, speedLimit)
                .WithJob(sourceCity, sourceCompany, destCity, destCompany, jobIncome, cargoMass, cargoDamage)
                .WithTruckConstants(forwardGearCount: forwardGearCount, engineRpmMax: rpmMax, retarderStepCount: retarderStepCount)
                .WithDashboard(speed: speed, cruiseControl: cruiseControl, cruiseSpeed: cruiseControlSpeed, fuelAvg: fuelAverageConsumption, fuelAmount: fuelAmount, fuelRange: fuelDistance, oilPressure: oilPressure, oilTemp: oilTemp, waterTemp: waterTemp, batteryVoltage: batteryVoltage, rpm: rpm)
                .WithWarnings(fuelWarning, adBlueWarning, oilPressureWarning, waterTempWarning, batteryVoltageWarning)
                .WithLights(parkingLights, lowBeam, highBeam, hazardLights, blinkerLeft, blinkerRight)
                .WithMotor(selectedGear, parkingBrake, retarderLevel)
                .WithDamage(cabinDamage, engineDamage)
                .WithControl(throttle)
                .WithTrailerEmpty()
                .Build();
        }

        #endregion
    }


}

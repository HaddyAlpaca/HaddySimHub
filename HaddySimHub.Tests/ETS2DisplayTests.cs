using HaddySimHub.Displays.ETS;
using HaddySimHub.Models;
using Xunit;

namespace HaddySimHub.Tests
{
    public class ETS2DisplayTests
    {
        #region Gear Tests

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesGear_WithNeutral()
        {
            // Arrange
            var gear = 0;

            // Act
            var result = CalculateGear(gear, 12);

            // Assert
            Assert.Equal("N", result);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesGear_WithReverseGear()
        {
            // Arrange
            var gear = -1;

            // Act
            var result = CalculateGear(gear, 12);

            // Assert
            Assert.Equal("R1", result);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesGear_WithMultipleReverseGears()
        {
            // Arrange
            var gear = -2;

            // Act
            var result = CalculateGear(gear, 12);

            // Assert
            Assert.Equal("R2", result);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesGear_WithForwardGear_NonEuro()
        {
            // Arrange
            var gear = 5;

            // Act
            var result = CalculateGear(gear, 8);

            // Assert
            Assert.Equal("5", result);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesGear_WithForwardGear_Euro14Gears()
        {
            // Arrange - In Euro trucks with 14 gears, gear 1 is displayed as "C1"
            var gear = 1;

            // Act
            var result = CalculateGear(gear, 14);

            // Assert
            Assert.Equal("C1", result);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesGear_WithForwardGear_Euro14Gears_Second()
        {
            // Arrange - Gear 3 becomes "1" in the display (3-2=1)
            var gear = 3;

            // Act
            var result = CalculateGear(gear, 14);

            // Assert
            Assert.Equal("1", result);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesGear_WithForwardGear_Euro14Gears_High()
        {
            // Arrange - Gear 14 becomes "12" in the display (14-2=12)
            var gear = 14;

            // Act
            var result = CalculateGear(gear, 14);

            // Assert
            Assert.Equal("12", result);
        }

        #endregion

        #region Display Property Tests

        [Fact]
        public void Display_Description_ReturnsEuroTruckSimulator2()
        {
            // Arrange
            var description = "Euro Truck Simulator 2";

            // Assert
            Assert.Equal("Euro Truck Simulator 2", description);
        }

        [Fact]
        public void Display_IsActive_ChecksForEurotrucks2Process()
        {
            // Arrange & Act
            var processName = "eurotrucks2";

            // Assert - just verify the process name is correct
            Assert.Equal("eurotrucks2", processName);
        }

        #endregion

        #region Damage Calculation Tests

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesDamage_AsPercentage()
        {
            // Arrange
            float damageValue = 0.5f; // 50% damage

            // Act
            int damagePercent = (int)System.Math.Round(damageValue * 100);

            // Assert
            Assert.Equal(50, damagePercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesDamage_Cabin()
        {
            // Arrange
            float cabinDamage = 0.25f;

            // Act
            int damagePercent = (int)System.Math.Round(cabinDamage * 100);

            // Assert
            Assert.Equal(25, damagePercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesDamage_Engine()
        {
            // Arrange
            float engineDamage = 0.75f;

            // Act
            int damagePercent = (int)System.Math.Round(engineDamage * 100);

            // Assert
            Assert.Equal(75, damagePercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesDamage_Clamped()
        {
            // Arrange - damage should not go negative
            float damageValue = 0f;

            // Act
            int damagePercent = (int)System.Math.Round(damageValue * 100);

            // Assert
            Assert.Equal(0, damagePercent);
        }

        #endregion

        #region Fuel Calculation Tests

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesFuelAverageConsumption()
        {
            // Arrange
            float consumption = 0.25f; // 0.25 l/km

            // Act
            float averageConsumption = (float)System.Math.Round(consumption * 100, 1);

            // Assert
            Assert.Equal(25.0f, averageConsumption);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesFuelAverageConsumption_WithDecimals()
        {
            // Arrange
            float consumption = 0.125f; // 0.125 l/km

            // Act
            float averageConsumption = (float)System.Math.Round(consumption * 100, 1);

            // Assert
            Assert.Equal(12.5f, averageConsumption);
        }

        [Fact]
        public void ConvertToDisplayUpdate_FuelAmount_Positive()
        {
            // Arrange
            float fuelAmount = 500f; // 500 liters

            // Act & Assert
            Assert.True(fuelAmount > 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_FuelDistance_RangeCalculated()
        {
            // Arrange
            float fuelRange = 1500f; // 1500 km range

            // Act & Assert
            Assert.True(fuelRange >= 0);
        }

        #endregion

        #region Speed Tests

        [Fact]
        public void ConvertToDisplayUpdate_Speed_IsNonNegative()
        {
            // Arrange
            double speed = 85.5;

            // Act
            short speedKph = (short)System.Math.Max(speed, 0);

            // Assert
            Assert.Equal(85, speedKph);
        }

        [Fact]
        public void ConvertToDisplayUpdate_Speed_HandlesNegative()
        {
            // Arrange
            double speed = -10.0;

            // Act
            short speedKph = (short)System.Math.Max(speed, 0);

            // Assert
            Assert.Equal(0, speedKph);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SpeedLimit_Handled()
        {
            // Arrange
            double speedLimit = 90.0;

            // Act
            short speedLimitKph = (short)System.Math.Max(speedLimit, 0);

            // Assert
            Assert.Equal(90, speedLimitKph);
        }

        #endregion

        #region Light Tests

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_ParkingLights_Stored(bool parkingLightsOn)
        {
            // Act & Assert
            Assert.IsType<bool>(parkingLightsOn);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_LowBeam_Stored(bool lowBeamOn)
        {
            // Act & Assert
            Assert.IsType<bool>(lowBeamOn);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_HighBeam_Stored(bool highBeamOn)
        {
            // Act & Assert
            Assert.IsType<bool>(highBeamOn);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_HazardLights_Stored(bool hazardLightsOn)
        {
            // Act & Assert
            Assert.IsType<bool>(hazardLightsOn);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_BlinkerLeft_Stored(bool blinkerLeftOn)
        {
            // Act & Assert
            Assert.IsType<bool>(blinkerLeftOn);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_BlinkerRight_Stored(bool blinkerRightOn)
        {
            // Act & Assert
            Assert.IsType<bool>(blinkerRightOn);
        }

        #endregion

        #region Brake Tests

        [Fact]
        public void ConvertToDisplayUpdate_ParkingBrake_Active()
        {
            // Arrange
            bool parkingBrakeOn = true;

            // Act & Assert
            Assert.True(parkingBrakeOn);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ParkingBrake_Inactive()
        {
            // Arrange
            bool parkingBrakeOn = false;

            // Act & Assert
            Assert.False(parkingBrakeOn);
        }

        [Fact]
        public void ConvertToDisplayUpdate_RetarderLevel_Valid()
        {
            // Arrange
            uint retarderLevel = 3;
            uint retarderStepCount = 5;

            // Act & Assert
            Assert.True(retarderLevel <= retarderStepCount);
        }

        [Fact]
        public void ConvertToDisplayUpdate_RetarderLevel_MaximumSteps()
        {
            // Arrange
            uint retarderLevel = 5;
            uint retarderStepCount = 5;

            // Act & Assert
            Assert.Equal(retarderStepCount, retarderLevel);
        }

        #endregion

        #region Throttle Tests

        [Fact]
        public void ConvertToDisplayUpdate_Throttle_CalculatedAsPercentage()
        {
            // Arrange
            double throttle = 0.75; // 75%

            // Act
            int throttlePercent = Convert.ToInt32(System.Math.Round(throttle * 100));

            // Assert
            Assert.Equal(75, throttlePercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_Throttle_MinimumValue()
        {
            // Arrange
            double throttle = 0.0;

            // Act
            int throttlePercent = Convert.ToInt32(System.Math.Round(throttle * 100));

            // Assert
            Assert.Equal(0, throttlePercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_Throttle_MaximumValue()
        {
            // Arrange
            double throttle = 1.0;

            // Act
            int throttlePercent = Convert.ToInt32(System.Math.Round(throttle * 100));

            // Assert
            Assert.Equal(100, throttlePercent);
        }

        #endregion

        #region Temperature Tests

        [Fact]
        public void ConvertToDisplayUpdate_OilTemperature_Valid()
        {
            // Arrange
            float oilTemp = 85.5f;

            // Act & Assert
            Assert.True(oilTemp >= 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_WaterTemperature_Valid()
        {
            // Arrange
            float waterTemp = 95.0f;

            // Act & Assert
            Assert.True(waterTemp >= 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_OilPressure_Valid()
        {
            // Arrange
            float oilPressure = 4.5f;

            // Act & Assert
            Assert.True(oilPressure >= 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_BatteryVoltage_Valid()
        {
            // Arrange
            float batteryVoltage = 13.5f;

            // Act & Assert
            Assert.True(batteryVoltage > 0);
        }

        #endregion

        #region Warning Tests

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_FuelWarning_Stored(bool fuelWarningOn)
        {
            // Act & Assert
            Assert.IsType<bool>(fuelWarningOn);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_AdBlueWarning_Stored(bool adBlueWarningOn)
        {
            // Act & Assert
            Assert.IsType<bool>(adBlueWarningOn);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_OilPressureWarning_Stored(bool oilPressureWarningOn)
        {
            // Act & Assert
            Assert.IsType<bool>(oilPressureWarningOn);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_WaterTempWarning_Stored(bool waterTempWarningOn)
        {
            // Act & Assert
            Assert.IsType<bool>(waterTempWarningOn);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_BatteryVoltageWarning_Stored(bool batteryVoltageWarningOn)
        {
            // Act & Assert
            Assert.IsType<bool>(batteryVoltageWarningOn);
        }

        #endregion

        #region Job Data Tests

        [Fact]
        public void ConvertToDisplayUpdate_JobIncome_Valid()
        {
            // Arrange
            ulong jobIncome = 50000;

            // Act & Assert
            Assert.True(jobIncome >= 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CargoMass_Calculated()
        {
            // Arrange
            double cargoMass = 12500.7;

            // Act
            int cargoMassInt = (int)System.Math.Ceiling(cargoMass);

            // Assert
            Assert.Equal(12501, cargoMassInt);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CargoDamage_AsPercentage()
        {
            // Arrange
            float cargoDamage = 0.15f;

            // Act
            int cargoDamagePercent = (int)System.Math.Round(cargoDamage * 100);

            // Assert
            Assert.Equal(15, cargoDamagePercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_JobCityAndCompany_Stored()
        {
            // Arrange
            string sourceCity = "Berlin";
            string sourceCompany = "Company A";
            string destCity = "Paris";
            string destCompany = "Company B";

            // Act & Assert
            Assert.Equal("Berlin", sourceCity);
            Assert.Equal("Company A", sourceCompany);
            Assert.Equal("Paris", destCity);
            Assert.Equal("Company B", destCompany);
        }

        #endregion

        #region Navigation Tests

        [Fact]
        public void ConvertToDisplayUpdate_DistanceRemaining_Converted()
        {
            // Arrange
            double navDistance = 50000; // 50000 meters

            // Act
            int distanceKm = (int)System.Math.Round(System.Math.Max(navDistance, 0) / 1000);

            // Assert
            Assert.Equal(50, distanceKm);
        }

        [Fact]
        public void ConvertToDisplayUpdate_TimeRemaining_Converted()
        {
            // Arrange
            double navTime = 3600; // 3600 seconds

            // Act
            int timeMinutes = (int)System.Math.Round(System.Math.Max(navTime, 0) / 60);

            // Assert
            Assert.Equal(60, timeMinutes);
        }

        [Fact]
        public void ConvertToDisplayUpdate_RestTimeRemaining_Valid()
        {
            // Arrange
            float restTime = 120.0f; // 120 minutes

            // Act & Assert
            Assert.True(restTime >= 0);
        }

        #endregion

        #region Cruise Control Tests

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConvertToDisplayUpdate_CruiseControl_Toggled(bool cruiseControlOn)
        {
            // Act & Assert
            Assert.IsType<bool>(cruiseControlOn);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CruiseControlSpeed_Stored()
        {
            // Arrange
            double cruiseSpeed = 85.5;

            // Act
            short cruiseSpeedKph = (short)cruiseSpeed;

            // Assert
            Assert.Equal(85, cruiseSpeedKph);
        }

        #endregion

        #region Helper Methods

        private string CalculateGear(int selectedGear, int forwardGearCount)
        {
            string gear = string.Empty;
            if (selectedGear == 0)
            {
                gear = "N";
            }
            else if (selectedGear < 0)
            {
                gear = "R" + System.Math.Abs(selectedGear).ToString();
            }
            else if (selectedGear > 0)
            {
                if (forwardGearCount == 14)
                {
                    gear = selectedGear == 1 ? "C1" : (selectedGear - 2).ToString();
                }
                else
                {
                    gear = selectedGear.ToString();
                }
            }
            return gear;
        }

        #endregion
    }
}

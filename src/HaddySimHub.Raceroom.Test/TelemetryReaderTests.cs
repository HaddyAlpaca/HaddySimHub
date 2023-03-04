using FluentAssertions;
using HaddySimHub.Raceroom.Data;
using HaddySimHub.Telemetry;
using HaddySimHub.Telemetry.Models;
using Moq;

namespace HaddySimHub.Raceroom.Test;

[TestClass]
public class TelemetryReaderTests
{
    private readonly TelemetryReader telemetryReader;
    private readonly Mock<ISharedMemoryReaderFactory> sharedMemoryReaderFactory = new();
    private readonly Mock<ISharedMemoryReader<Shared>> mmf = new();

    public TelemetryReaderTests()
    {
        //Setup shared memory reader factory
        this.sharedMemoryReaderFactory.Setup(f => f.Create<Shared>("$R3E")).Returns(this.mmf.Object);

        this.telemetryReader = new TelemetryReader(sharedMemoryReaderFactory.Object);
    }

    [TestMethod]
    public void Gear_is_read()
    {
        //Arrange
        Shared data = new()
        {
            Gear = 4
        };
        this.mmf.Setup(mmf => mmf.Read()).Returns(data);

        //Act
        RaceData telemetry = (RaceData)this.telemetryReader.ReadTelemetry();

        //Assert
        telemetry.Should().NotBeNull();
        telemetry.Gear.Should().Be(4);
    }

    [TestMethod]
    public void Speed_is_converted_from_meters_per_second_to_kilometers_per_hour()
    {
        //Arrange
        Shared data = new()
        {
            CarSpeed = 50f
        };
        this.mmf.Setup(mmf => mmf.Read()).Returns(data);

        //Act
        RaceData telemetry = (RaceData)this.telemetryReader.ReadTelemetry();

        //Assert
        telemetry.Should().NotBeNull();
        telemetry.Speed.Should().Be(180);
    }

    [TestMethod]
    public void Revs_are_converted_from_radians_per_second_to_rotations_per_minute()
    {
        //Arrange
        Shared data = new()
        {
            EngineRps = 157.079633f,
            MaxEngineRps = 628.318531f
        };
        this.mmf.Setup(mmf => mmf.Read()).Returns(data);

        //Act
        RaceData telemetry = (RaceData)this.telemetryReader.ReadTelemetry();

        //Assert
        telemetry.Should().NotBeNull();
        telemetry.Rpm.Should().Be(1500);
        telemetry.RpmMax.Should().Be(6000);
    }
}
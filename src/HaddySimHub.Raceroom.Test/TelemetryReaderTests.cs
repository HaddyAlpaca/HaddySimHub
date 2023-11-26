using FluentAssertions;
using HaddySimHub.Raceroom.Data;
using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using Moq;

namespace HaddySimHub.Raceroom.Test;

[TestClass]
public class GameDataReaderTests
{
    private readonly GameDateReader _reader;
    private readonly Mock<ISharedMemoryReaderFactory> sharedMemoryReaderFactory = new();
    private readonly Mock<ISharedMemoryReader<Shared>> mmf = new();

    public GameDataReaderTests()
    {
        //Setup shared memory reader factory
        this.sharedMemoryReaderFactory.Setup(f => f.Create<Shared>("$R3E")).Returns(this.mmf.Object);

        this._reader = new GameDateReader(sharedMemoryReaderFactory.Object);
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
        RaceData raceData = (RaceData)this._reader.ReadData();

        //Assert
        raceData.Should().NotBeNull();
        raceData.Gear.Should().Be(4);
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
        RaceData raceData = (RaceData)this._reader.ReadData();

        //Assert
        raceData.Should().NotBeNull();
        raceData.Speed.Should().Be(180);
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
        RaceData raceData = (RaceData)this._reader.ReadData();

        //Assert
        raceData.Should().NotBeNull();
        raceData.Rpm.Should().Be(1500);
    }
}
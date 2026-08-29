using System.Reflection;
using System.Runtime.InteropServices;
using HaddySimHub.Displays.Msfs;
using HaddySimHub.Displays.Msfs.Interop;

namespace HaddySimHub.Tests;

/// <summary>
/// Pins the contract between the simvar list and the struct the sim writes into.
/// </summary>
/// <remarks>
/// SimConnect returns one opaque block of bytes laid out in the order the variables
/// were added to the definition. Nothing checks it at runtime: a wrong data type or a
/// reordered entry shifts every field after it, and the sim happily reports success
/// while the dashboard shows an altitude where the airspeed should be. These tests
/// are the only thing standing between that mistake and a green build.
/// </remarks>
[TestClass]
public class SimVarDefinitionsTests
{
    [TestMethod]
    public void TotalDefinitionSize_MatchesTelemetryStructSize()
    {
        Assert.AreEqual(Marshal.SizeOf<MsfsTelemetry>(), SimVarDefinitions.TotalSizeInBytes);
    }

    [TestMethod]
    public void DefinitionCount_MatchesTelemetryFieldCount()
    {
        var fieldCount = typeof(MsfsTelemetry)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Length;

        Assert.AreEqual(fieldCount, SimVarDefinitions.All.Count);
    }

    [TestMethod]
    public void DefinitionSizes_MatchTheTelemetryFieldTheyLandIn()
    {
        var fields = typeof(MsfsTelemetry).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (var index = 0; index < SimVarDefinitions.All.Count; index++)
        {
            var definition = SimVarDefinitions.All[index];
            var field = fields[index];
            var expectedType = definition.DataType == SimConnectDataType.Float64 ? typeof(double) : typeof(string);

            Assert.AreEqual(
                expectedType,
                field.FieldType,
                $"Simvar '{definition.Name}' is defined as {definition.DataType} but lands in field '{field.Name}'.");
        }
    }

    [TestMethod]
    public void TelemetryStruct_IsTheExpectedSize()
    {
        // 62 doubles, then a 32, a 32 and a 256 byte string.
        Assert.AreEqual(816, Marshal.SizeOf<MsfsTelemetry>());
    }

    [TestMethod]
    public void NumericDefinitions_AllCarryAUnit()
    {
        foreach (var definition in SimVarDefinitions.All.Where(d => d.DataType == SimConnectDataType.Float64))
        {
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(definition.Unit),
                $"Simvar '{definition.Name}' has no unit, so the simulator would pick one for us.");
        }
    }

    [TestMethod]
    public void StringDefinitions_CarryNoUnit()
    {
        foreach (var definition in SimVarDefinitions.All.Where(d => d.DataType != SimConnectDataType.Float64))
        {
            Assert.IsNull(definition.Unit, $"Simvar '{definition.Name}' is a string and must be requested without a unit.");
        }
    }

    [TestMethod]
    public void Definitions_HaveNoDuplicateNames()
    {
        var duplicates = SimVarDefinitions.All
            .GroupBy(definition => definition.Name)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        Assert.AreEqual(0, duplicates.Count, $"Duplicate simvars: {string.Join(", ", duplicates)}");
    }

    [TestMethod]
    public void SizeInBytes_RejectsATypeTheStructCannotHold()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => SimVarDefinitions.SizeInBytes(SimConnectDataType.Int32));
    }

    [TestMethod]
    public void SimObjectDataHeader_IsTheSdkSize()
    {
        // The payload starts straight after the header, so this offset is what makes
        // the telemetry read from the right place.
        Assert.AreEqual(40, Marshal.SizeOf<SimConnectRecvSimObjectData>());
    }

    [TestMethod]
    public void RecvHeader_IsTheSdkSize()
    {
        Assert.AreEqual(12, Marshal.SizeOf<SimConnectRecv>());
        Assert.AreEqual(24, Marshal.SizeOf<SimConnectRecvException>());
    }
}

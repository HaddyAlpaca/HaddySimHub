using System.Net;
using System.Text.Json.Nodes;
using HaddySimHub.Capture;
using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Displays.Forza;
using Microsoft.Extensions.Logging.Abstractions;

namespace HaddySimHub.Tests;

[TestClass]
public class RawSourceProviderCaptureTests
{
    private string _directory = string.Empty;

    [TestInitialize]
    public void CreateDirectory()
    {
        _directory = Path.Combine(Path.GetTempPath(), "haddysimhub-source-capture", Guid.NewGuid().ToString("N"));
    }

    [TestCleanup]
    public void RemoveDirectory()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [TestMethod]
    public void DirtRallyUdpCapturePreservesMalformedDatagramBytes()
    {
        var payload = new byte[] { 0, 5, 128, 255 };
        using (var capture = NewCapture())
        {
            var provider = new Dirt2GameDataProvider(capture);
            provider.CaptureDatagram(payload, new IPEndPoint(IPAddress.Loopback, 20777));
        }

        AssertRawPayload("dirtrally2", payload);
    }

    [TestMethod]
    public void ForzaUdpCapturePreservesShortDatagramBytes()
    {
        var payload = new byte[] { 255, 1, 7 };
        using (var capture = NewCapture())
        {
            var provider = new ForzaGameDataProvider(capture);
            provider.CaptureDatagram(payload, new IPEndPoint(IPAddress.Loopback, 5300));
        }

        AssertRawPayload("forza", payload);
    }

    private TelemetryCapture NewCapture() =>
        new(_directory, NullLogger<TelemetryCapture>.Instance);

    private void AssertRawPayload(string game, byte[] expected)
    {
        var frame = JsonNode.Parse(File.ReadAllText(Path.Combine(_directory, $"{game}.jsonl")))!;
        var raw = frame["raw"]!.AsObject();
        Assert.AreEqual("udp", raw["transport"]!.GetValue<string>());
        CollectionAssert.AreEqual(
            expected,
            Convert.FromBase64String(raw["pages"]!["datagram"]!.GetValue<string>()));
    }
}

using System.Text.Json.Nodes;
using System.Security.Cryptography;
using HaddySimHub.Capture;
using Microsoft.Extensions.Logging.Abstractions;

namespace HaddySimHub.Tests;

[TestClass]
public class TelemetryCaptureTests
{
    private string _directory = string.Empty;

    [TestInitialize]
    public void CreateDirectory()
    {
        _directory = Path.Combine(Path.GetTempPath(), "haddysimhub-capture", Guid.NewGuid().ToString("N"));
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
    public void DisabledCaptureWritesNothing()
    {
        using var capture = NewDisabledCapture();

        capture.RecordRawFrame("acc", "shared-memory", "acc", new Dictionary<string, byte[]> { ["data"] = new byte[800] });

        Assert.IsFalse(capture.Enabled);
        Assert.IsFalse(Directory.Exists(_directory));
    }

    [TestMethod]
    public void RawCaptureWritesOnlyTheSourceBytesAndTransportMetadata()
    {
        var capture = NewCapture();
        var physics = Enumerable.Range(0, 800).Select(value => (byte)value).ToArray();
        var graphics = Enumerable.Range(0, 1588).Select(value => (byte)(value % 256)).ToArray();

        capture.RecordRawFrame(
            "acc",
            "shared-memory",
            "Local\\acpmf_physics + Local\\acpmf_graphics",
            new Dictionary<string, byte[]> { ["physics"] = physics, ["graphics"] = graphics });
        capture.Dispose();

        var frames = ReadFrames();
        Assert.AreEqual(1, frames.Count);
        Assert.AreEqual(0, frames[0]["seq"]!.GetValue<int>());
        var frame = frames[0].AsObject();
        CollectionAssert.AreEquivalent(
            new[] { "seq", "captureId", "capturedAtUtc", "raw", "integrity" },
            frame.Select(property => property.Key).ToArray());
        Assert.IsTrue(Guid.TryParseExact(frame["captureId"]!.GetValue<string>(), "N", out _));
        var raw = frame["raw"]!.AsObject();
        Assert.AreEqual("shared-memory", raw["transport"]!.GetValue<string>());
        Assert.AreEqual("Local\\acpmf_physics + Local\\acpmf_graphics", raw["sourceName"]!.GetValue<string>());
        Assert.AreEqual(1, raw["schemaVersion"]!.GetValue<int>());
        var pages = raw["pages"]!.AsObject();
        CollectionAssert.AreEquivalent(new[] { "physics", "graphics" }, pages.Select(page => page.Key).ToArray());
        CollectionAssert.AreEqual(physics, Convert.FromBase64String(pages["physics"]!.GetValue<string>()));
        CollectionAssert.AreEqual(graphics, Convert.FromBase64String(pages["graphics"]!.GetValue<string>()));

        var integrity = frames[0]["integrity"]!["pages"]!.AsObject();
        Assert.AreEqual(physics.Length, integrity["physics"]!["byteLength"]!.GetValue<int>());
        Assert.AreEqual(
            Convert.ToHexString(SHA256.HashData(physics)).ToLowerInvariant(),
            integrity["physics"]!["sha256"]!.GetValue<string>());
    }

    [TestMethod]
    public void CleanShutdownMarksCaptureCompleteAfterAllQueuedFramesAreWritten()
    {
        var capture = NewCapture();
        capture.RecordRawFrame("acc", "shared-memory", "acc", new Dictionary<string, byte[]> { ["physics"] = [1, 2, 3] });
        var statusPath = Directory.GetFiles(_directory, "capture-status-*.json").Single();
        var runningStatus = JsonNode.Parse(File.ReadAllText(statusPath))!;

        Assert.AreEqual("capturing", runningStatus["status"]!.GetValue<string>());

        capture.Dispose();

        var completedStatus = JsonNode.Parse(File.ReadAllText(statusPath))!;
        Assert.AreEqual("complete", completedStatus["status"]!.GetValue<string>());
        Assert.AreEqual(1, completedStatus["framesWritten"]!["acc"]!.GetValue<int>());
        Assert.AreEqual(1, ReadFrames().Count);
    }

    [TestMethod]
    public void WriteFailureMarksCaptureIncomplete()
    {
        var capture = NewCapture();
        var statusPath = Directory.GetFiles(_directory, "capture-status-*.json").Single();
        Directory.CreateDirectory(Path.Combine(_directory, "acc.jsonl"));

        capture.RecordRawFrame("acc", "shared-memory", "acc", new Dictionary<string, byte[]> { ["physics"] = [1, 2, 3] });
        capture.Dispose();

        var status = JsonNode.Parse(File.ReadAllText(statusPath))!;
        Assert.AreEqual("incomplete", status["status"]!.GetValue<string>());
        Assert.AreEqual("write-failed:acc", status["failure"]!.GetValue<string>());
    }

    [TestMethod]
    public void SourceDiscontinuityMarksCaptureIncomplete()
    {
        var capture = NewCapture();
        var statusPath = Directory.GetFiles(_directory, "capture-status-*.json").Single();

        capture.MarkIncomplete("source-gap:iracing:10-12");
        capture.Dispose();

        var status = JsonNode.Parse(File.ReadAllText(statusPath))!;
        Assert.AreEqual("incomplete", status["status"]!.GetValue<string>());
        Assert.AreEqual("source-gap:iracing:10-12", status["failure"]!.GetValue<string>());
    }

    [TestMethod]
    public void FrameLargerThanQueueBudgetMarksCaptureIncomplete()
    {
        var capture = NewCapture();
        var statusPath = Directory.GetFiles(_directory, "capture-status-*.json").Single();
        var oversizedPage = new byte[64 * 1024 * 1024 + 1];

        capture.RecordRawFrame(
            "iracing",
            "shared-memory",
            "Local\\IRSDKMemMapFileName",
            new Dictionary<string, byte[]> { ["data"] = oversizedPage });
        capture.Dispose();

        var status = JsonNode.Parse(File.ReadAllText(statusPath))!;
        Assert.AreEqual("incomplete", status["status"]!.GetValue<string>());
        Assert.AreEqual("queue-capacity-exceeded", status["failure"]!.GetValue<string>());
    }

    [TestMethod]
    public void RawCaptureCanStoreSourceBytesWithoutInterpretingTheirLength()
    {
        var capture = NewCapture();
        var packet = new byte[] { 0, 1, 2, 255 };

        capture.RecordRawFrame(
            "dirtrally2",
            "udp",
            "game telemetry packet",
            new Dictionary<string, byte[]> { ["packet"] = packet });
        capture.Dispose();

        var raw = ReadFrames("dirtrally2").Single()["raw"]!.AsObject();
        Assert.AreEqual("udp", raw["transport"]!.GetValue<string>());
        CollectionAssert.AreEqual(
            packet,
            Convert.FromBase64String(raw["pages"]!["packet"]!.GetValue<string>()));
    }

    [TestMethod]
    public void AppendingRawCapturesContinuesTheSequence()
    {
        using (var first = NewCapture())
        {
            first.RecordRawFrame("acc", "shared-memory", "acc", new Dictionary<string, byte[]> { ["physics"] = new byte[800] });
        }

        using (var second = NewCapture())
        {
            second.RecordRawFrame("acc", "shared-memory", "acc", new Dictionary<string, byte[]> { ["physics"] = new byte[800] });
        }

        var frames = ReadFrames("acc");

        Assert.AreEqual(2, frames.Count);
        Assert.AreEqual(0, frames[0]["seq"]!.GetValue<int>());
        Assert.AreEqual(1, frames[1]["seq"]!.GetValue<int>());
    }

    private TelemetryCapture NewCapture() =>
        new(_directory, NullLogger<TelemetryCapture>.Instance);

    private TelemetryCapture NewDisabledCapture() =>
        new(null, NullLogger<TelemetryCapture>.Instance);

    private List<JsonObject> ReadFrames(string game = "acc")
    {
        var path = Path.Combine(_directory, $"{game}.jsonl");
        if (!File.Exists(path))
        {
            return [];
        }

        return File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonNode.Parse(line)!.AsObject())
            .ToList();
    }
}

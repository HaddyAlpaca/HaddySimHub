using HaddySimHub.Displays.Msfs;
using HaddySimHub.Tests.Mocks;

namespace HaddySimHub.Tests;

[TestClass]
public class MsfsGameDataProviderTests
{
    /// <summary>The provider polls every 10 ms; this is comfortably several ticks.</summary>
    private static readonly TimeSpan PollWindow = TimeSpan.FromMilliseconds(500);

    [TestMethod]
    public void Constructor_RejectsAMissingClient()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new MsfsGameDataProvider(null!));
    }

    [TestMethod]
    public void Start_ConnectsTheClient()
    {
        var client = new MockSimConnectClient();
        using var provider = new MsfsGameDataProvider(client);

        provider.Start();

        Assert.IsTrue(client.IsConnected);
    }

    [TestMethod]
    public void Start_KeepsPollingWhenTheSimulatorIsNotUpYet()
    {
        // The display starts as soon as the process is detected, which is well before
        // the simulator is ready to accept a connection.
        var client = new MockSimConnectClient { ConnectFails = true };
        using var provider = new MsfsGameDataProvider(client);

        provider.Start();
        Assert.IsFalse(client.IsConnected);

        WaitFor(() => client.ConnectCallCount > 1);
        Assert.IsTrue(client.ConnectCallCount > 1, "The provider gave up instead of retrying.");
    }

    [TestMethod]
    public void Poll_ReconnectsAfterTheConnectionDrops()
    {
        var client = new MockSimConnectClient();
        using var provider = new MsfsGameDataProvider(client);
        provider.Start();

        // The simulator quitting leaves the client disconnected mid-flight.
        client.Disconnect();

        WaitFor(() => client.IsConnected);
        Assert.IsTrue(client.IsConnected);
    }

    [TestMethod]
    public void Poll_RaisesDataReceivedForEachTelemetryBlock()
    {
        var client = new MockSimConnectClient();
        using var provider = new MsfsGameDataProvider(client);

        MsfsTelemetry? received = null;
        provider.DataReceived += (_, telemetry) => received = telemetry;

        provider.Start();
        var sent = new MsfsTelemetry { IndicatedAirspeed = 142 };
        client.QueueTelemetry(sent);

        WaitFor(() => received is not null);

        Assert.IsNotNull(received);
        Assert.AreEqual(142, received.Value.IndicatedAirspeed);
    }

    [TestMethod]
    public void Poll_RaisesNothingWhenTheSimulatorSentNoFrame()
    {
        var client = new MockSimConnectClient();
        using var provider = new MsfsGameDataProvider(client);

        var raised = 0;
        provider.DataReceived += (_, _) => Interlocked.Increment(ref raised);

        provider.Start();
        Thread.Sleep(PollWindow);

        Assert.AreEqual(0, raised);
    }

    [TestMethod]
    public void Stop_DisconnectsAndStopsRaisingData()
    {
        var client = new MockSimConnectClient();
        var provider = new MsfsGameDataProvider(client);

        var raised = 0;
        provider.DataReceived += (_, _) => Interlocked.Increment(ref raised);

        provider.Start();
        provider.Stop();

        Assert.IsFalse(client.IsConnected);

        client.QueueTelemetry(new MsfsTelemetry());
        Thread.Sleep(PollWindow);

        Assert.AreEqual(0, raised);
        provider.Dispose();
    }

    [TestMethod]
    public void StartAfterStop_ResumesPolling()
    {
        var client = new MockSimConnectClient();
        using var provider = new MsfsGameDataProvider(client);

        var raised = 0;
        provider.DataReceived += (_, _) => Interlocked.Increment(ref raised);

        provider.Start();
        provider.Stop();
        provider.Start();

        client.QueueTelemetry(new MsfsTelemetry());
        WaitFor(() => raised > 0);

        Assert.IsTrue(raised > 0);
    }

    [TestMethod]
    public void Dispose_ReleasesTheClient()
    {
        var client = new MockSimConnectClient();
        var provider = new MsfsGameDataProvider(client);
        provider.Start();

        provider.Dispose();

        Assert.IsTrue(client.Disposed);
        Assert.IsFalse(client.IsConnected);
    }

    [TestMethod]
    public void Dispose_IsSafeToCallTwice()
    {
        var client = new MockSimConnectClient();
        var provider = new MsfsGameDataProvider(client);

        provider.Dispose();
        provider.Dispose();

        Assert.IsTrue(client.Disposed);
    }

    [TestMethod]
    public void Start_AfterDispose_Throws()
    {
        var client = new MockSimConnectClient();
        var provider = new MsfsGameDataProvider(client);
        provider.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(provider.Start);
    }

    private static void WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + PollWindow;
        while (DateTime.UtcNow < deadline && !condition())
        {
            Thread.Sleep(10);
        }
    }
}

using HaddySimHub.Displays.Msfs;
using HaddySimHub.Tests.Mocks;

namespace HaddySimHub.Tests;

[TestClass]
public class MsfsGameDataProviderTests
{
    /// <summary>
    /// How long a negative test watches for telemetry that must never arrive. Kept
    /// short because these tests always pay the full window.
    /// </summary>
    private static readonly TimeSpan PollWindow = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// How long a positive test waits for telemetry that must arrive.
    /// <para>
    /// The provider delivers on a 10 ms <see cref="Timer"/> callback, so waiting for
    /// one means waiting on the threadpool to schedule that callback. This assembly
    /// runs every test method concurrently (<c>Parallelize</c> at method level), so
    /// a deadline that assumed the callback always lands on schedule turned this
    /// into a flaky test: a spin-wait here occupies a pool thread for the whole
    /// window and starves the very timer it is waiting on, and the failure only
    /// shows up under a full-suite run. Prefer <c>ManualResetEventSlim</c>, which
    /// parks instead of spinning -- its timeout costs nothing on the happy path.
    /// </para>
    /// </summary>
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

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
        using var signal = new ManualResetEventSlim();
        provider.DataReceived += (_, telemetry) =>
        {
            received = telemetry;
            signal.Set();
        };

        provider.Start();
        var sent = new MsfsTelemetry { IndicatedAirspeed = 142 };
        client.QueueTelemetry(sent);

        Assert.IsTrue(signal.Wait(WaitTimeout), "The provider delivered no telemetry block.");

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
        using var signal = new ManualResetEventSlim();
        provider.DataReceived += (_, _) =>
        {
            Interlocked.Increment(ref raised);
            signal.Set();
        };

        provider.Start();
        provider.Stop();
        provider.Start();

        // Two blocks, not one. A single tick after Start satisfies a one-block wait,
        // so one block would pass even if polling wedged permanently after the first
        // frame -- which is what happens if Poll's guard is never released.
        client.QueueTelemetry(new MsfsTelemetry());
        Assert.IsTrue(signal.Wait(WaitTimeout), "The provider did not resume polling after Start following Stop.");
        Assert.IsTrue(raised > 0);

        Interlocked.Exchange(ref raised, 0);
        signal.Reset();
        client.QueueTelemetry(new MsfsTelemetry());
        Assert.IsTrue(signal.Wait(WaitTimeout), "The provider delivered one block then stopped polling.");
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

    /// <summary>
    /// Spins until a mock's state changes. Only for conditions nothing signals, so
    /// it cannot be turned into a parked wait; keep it off the critical path and
    /// give it <see cref="WaitTimeout"/> rather than the short <see cref="PollWindow"/>,
    /// because it too is waiting on the provider's 10 ms timer.
    /// </summary>
    private static void WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + WaitTimeout;
        while (DateTime.UtcNow < deadline && !condition())
        {
            Thread.Sleep(10);
        }
    }
}

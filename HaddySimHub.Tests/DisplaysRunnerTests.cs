using HaddySimHub.Displays;
using HaddySimHub.Tests.Mocks;

namespace HaddySimHub.Tests;

[TestClass]
public class DisplaysRunnerTests
{
    [TestMethod]
    public async Task Run_FeedsOnlyOneDisplayWhenTwoGamesAreRunning()
    {
        // Every display writes to the same stream, so running two would interleave
        // frames of different types on one screen.
        var first = new FakeDisplay("First") { IsActive = true };
        var second = new FakeDisplay("Second") { IsActive = true };

        var selected = await RunUntilSettledAsync(first, second);

        Assert.AreSame(first, selected);
        Assert.AreEqual(1, first.StartCount);
        Assert.AreEqual(0, second.StartCount);
    }

    [TestMethod]
    public async Task Run_KeepsTheRunningDisplayWhileItsGameIsUp()
    {
        var first = new FakeDisplay("First") { IsActive = false };
        var second = new FakeDisplay("Second") { IsActive = true };

        var runner = new DisplaysRunner([first, second], new MockDisplayUpdateSender());
        using var cancellation = new CancellationTokenSource();
        var running = runner.RunAsync(cancellation.Token);

        await WaitUntilAsync(() => runner.CurrentDisplay is not null);
        Assert.AreSame(second, runner.CurrentDisplay);

        // The earlier display's game starts too; the running one should keep its turn.
        first.IsActive = true;
        await Task.Delay(150);

        Assert.AreSame(second, runner.CurrentDisplay);
        Assert.AreEqual(0, first.StartCount);

        await StopAsync(cancellation, running);
    }

    [TestMethod]
    public async Task Run_SwitchesWhenTheRunningDisplaysGameQuits()
    {
        var first = new FakeDisplay("First") { IsActive = true };
        var second = new FakeDisplay("Second") { IsActive = true };

        var runner = new DisplaysRunner([first, second], new MockDisplayUpdateSender());
        using var cancellation = new CancellationTokenSource();
        var running = runner.RunAsync(cancellation.Token);

        await WaitUntilAsync(() => runner.CurrentDisplay is not null);
        first.IsActive = false;

        await WaitUntilAsync(() => ReferenceEquals(runner.CurrentDisplay, second));

        Assert.AreEqual(1, first.StopCount);
        Assert.AreEqual(1, second.StartCount);

        await StopAsync(cancellation, running);
    }

    [TestMethod]
    public async Task Run_StopsTheRunningDisplayOnShutdown()
    {
        var display = new FakeDisplay("Only") { IsActive = true };

        var runner = new DisplaysRunner([display], new MockDisplayUpdateSender());
        using var cancellation = new CancellationTokenSource();
        var running = runner.RunAsync(cancellation.Token);

        await WaitUntilAsync(() => runner.CurrentDisplay is not null);
        await StopAsync(cancellation, running);

        Assert.AreEqual(1, display.StopCount);
        Assert.IsNull(runner.CurrentDisplay);
    }

    [TestMethod]
    public async Task Run_KeepsGoingWhenADisplayThrowsWhileReportingItsState()
    {
        var faulty = new ThrowingDisplay();
        var healthy = new FakeDisplay("Healthy") { IsActive = true };

        var selected = await RunUntilSettledAsync(faulty, healthy);

        Assert.AreSame(healthy, selected);
    }

    /// <summary>
    /// Runs until a display has been selected and returns it. The runner clears
    /// <see cref="DisplaysRunner.CurrentDisplay"/> when it stops, so the choice is
    /// captured while it is still running.
    /// </summary>
    private static async Task<IDisplay?> RunUntilSettledAsync(params IDisplay[] displays)
    {
        var runner = new DisplaysRunner(displays, new MockDisplayUpdateSender());
        using var cancellation = new CancellationTokenSource();
        var running = runner.RunAsync(cancellation.Token);

        await WaitUntilAsync(() => runner.CurrentDisplay is not null);
        var selected = runner.CurrentDisplay;

        await StopAsync(cancellation, running);
        return selected;
    }

    private static async Task StopAsync(CancellationTokenSource cancellation, Task running)
    {
        cancellation.Cancel();
        await Ignoring(running);
    }

    private static async Task Ignoring(Task running)
    {
        try
        {
            await running;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(3);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(20);
        }

        Assert.IsTrue(condition(), "The runner did not reach the expected state in time.");
    }

    private sealed class FakeDisplay(string description) : IDisplay
    {
        public string Description => description;

        public bool IsActive { get; set; }

        public DateTime? LastUpdateUtc => null;

        public int StartCount { get; private set; }

        public int StopCount { get; private set; }

        public void Start() => StartCount++;

        public void Stop() => StopCount++;
    }

    private sealed class ThrowingDisplay : IDisplay
    {
        public string Description => "Throwing";

        public bool IsActive => throw new InvalidOperationException("detection failed");

        public DateTime? LastUpdateUtc => null;

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}

using HaddySimHub.Displays;
using HaddySimHub.Extensions;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HaddySimHub.Tests;

[TestClass]
public class DisplayLifecycleTests
{
    [TestMethod]
    public async Task DisplayBase_CanStartStopAndStartAgain_WithoutDuplicateSubscriptions()
    {
        var provider = new CountingGameDataProvider();
        var sender = new RecordingDisplayUpdateSender();
        var converter = new CountingDataConverter();
        var display = new CountingDisplay(provider, converter, sender);

        display.Start();
        provider.Emit(100);
        display.Stop();

        display.Start();
        provider.Emit(200);
        display.Stop();

        await Task.Delay(50);

        Assert.AreEqual(2, provider.StartCount);
        Assert.AreEqual(2, provider.StopCount);
        Assert.AreEqual(0, provider.SubscriberCount);
        Assert.AreEqual(2, sender.Updates.Count);
        Assert.AreEqual(2, converter.ConvertCount);
    }

    [TestMethod]
    public void RegisterGameDisplay_RegistersOneDisplayInstance_WhenUsedAsSingleRegistrationPath()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDisplayFactory, FakeDisplayFactory>();

        services.RegisterGameDisplay<FakeGameDataProvider, FakeDataConverter, int>(
            new GameDisplayDefinition<int>("fake", "Fake"));

        var serviceProvider = services.BuildServiceProvider();
        var displays = serviceProvider.GetServices<IDisplay>().ToList();

        Assert.AreEqual(1, displays.Count);
        Assert.AreEqual("Fake", displays[0].Description);
    }

    [TestMethod]
    public async Task DisplayBase_TracksLastUpdate_AndResetsOnStop()
    {
        var provider = new CountingGameDataProvider();
        var sender = new RecordingDisplayUpdateSender();
        var converter = new CountingDataConverter();
        var display = new CountingDisplay(provider, converter, sender);

        Assert.IsNull(display.LastUpdateUtc, "No data received yet, so LastUpdateUtc should be null.");

        display.Start();
        provider.Emit(1);
        await Task.Delay(20);

        Assert.IsNotNull(display.LastUpdateUtc, "Receiving telemetry should stamp LastUpdateUtc.");

        display.Stop();

        Assert.IsNull(display.LastUpdateUtc, "Stopping should clear LastUpdateUtc so a restarted game is not shown as live.");
    }

    private sealed class CountingDisplay : DisplayBase<int>
    {
        public CountingDisplay(
            IGameDataProvider<int> gameDataProvider,
            IDataConverter<int, DisplayUpdate> dataConverter,
            IDisplayUpdateSender displayUpdateSender)
            : base(gameDataProvider, dataConverter, displayUpdateSender)
        {
        }

        public override string Description => "CountingDisplay";
        public override bool IsActive => true;
    }

    private sealed class CountingGameDataProvider : IGameDataProvider<int>
    {
        private EventHandler<int>? _dataReceived;

        public int StartCount { get; private set; }
        public int StopCount { get; private set; }
        public int SubscriberCount { get; private set; }

        public event EventHandler<int>? DataReceived
        {
            add
            {
                _dataReceived += value;
                SubscriberCount++;
            }
            remove
            {
                _dataReceived -= value;
                SubscriberCount--;
            }
        }

        public void Start()
        {
            StartCount++;
        }

        public void Stop()
        {
            StopCount++;
        }

        public void Emit(int value)
        {
            _dataReceived?.Invoke(this, value);
        }
    }

    private sealed class CountingDataConverter : IDataConverter<int, DisplayUpdate>
    {
        public int ConvertCount { get; private set; }

        public DisplayUpdate Convert(int input)
        {
            ConvertCount++;
            return new DisplayUpdate { Type = DisplayType.RaceDashboard };
        }
    }

    private sealed class RecordingDisplayUpdateSender : IDisplayUpdateSender
    {
        public List<DisplayUpdate> Updates { get; } = [];

        public Task SendDisplayUpdate(DisplayUpdate displayUpdate)
        {
            Updates.Add(displayUpdate);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeGameDataProvider : IGameDataProvider<int>
    {
        public event EventHandler<int>? DataReceived
        {
            add { }
            remove { }
        }
        public void Start() { }
        public void Stop() { }
    }

    private sealed class FakeDataConverter : IDataConverter<int, DisplayUpdate>
    {
        public DisplayUpdate Convert(int input)
        {
            return new DisplayUpdate { Type = DisplayType.RaceDashboard };
        }
    }

    private sealed class FakeDisplayFactory : IDisplayFactory
    {
        public IDisplay CreateGameDisplay<TTelemetry>(GameDisplayDefinition<TTelemetry> definition)
        {
            if (definition.ProcessName != "fake")
            {
                throw new InvalidOperationException($"Unknown display process: {definition.ProcessName}");
            }

            return new FakeDisplay();
        }

        public TDisplay CreateTestDisplay<TDisplay>(string id) where TDisplay : TestDisplayBase
        {
            throw new NotSupportedException("Not needed for this test.");
        }
    }

    private sealed class FakeDisplay : IDisplay
    {
        public string Description => "Fake";
        public bool IsActive => false;
        public DateTime? LastUpdateUtc => null;
        public void Start() { }
        public void Stop() { }
    }
}

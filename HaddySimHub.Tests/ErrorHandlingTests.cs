using HaddySimHub;
using HaddySimHub.Displays;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Services;
using Microsoft.AspNetCore.SignalR;

namespace HaddySimHub.Tests;

[TestClass]
public class ErrorHandlingTests
{
    [TestMethod]
    public async Task HubService_SendDisplayUpdateAsync_NullUpdate_ThrowsArgumentNullException()
    {
        var service = new HubService(new StubHubContext());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.SendDisplayUpdateAsync(null!));
    }

    [TestMethod]
    public async Task DisplayBase_ConverterThrows_DoesNotCrashAndRecoversOnNextData()
    {
        var provider = new EmittingGameDataProvider();
        var sender = new RecordingSender();
        var converter = new ThrowOnValueConverter(throwOnValue: 1);
        var display = new TestDisplay(provider, converter, sender);

        display.Start();
        provider.Emit(1); // converter throws; must be swallowed
        provider.Emit(2); // valid data must still be processed
        await sender.WaitForUpdatesAsync(1, TimeSpan.FromSeconds(2));
        display.Stop();

        Assert.AreEqual(1, sender.Updates.Count);
    }

    [TestMethod]
    public async Task DisplayBase_SenderThrows_DoesNotPropagateAndStopCompletes()
    {
        var provider = new EmittingGameDataProvider();
        var sender = new ThrowingSender();
        var converter = new PassthroughConverter();
        var display = new TestDisplay(provider, converter, sender);

        display.Start();
        provider.Emit(1); // sender throws inside the send loop; must be swallowed
        await Task.Delay(50);

        // Stop must complete promptly without observing the swallowed exception.
        var stopTask = Task.Run(display.Stop);
        var completed = await Task.WhenAny(stopTask, Task.Delay(TimeSpan.FromSeconds(2)));

        Assert.AreSame(stopTask, completed);
        await stopTask;
    }

    private sealed class TestDisplay : DisplayBase<int>
    {
        public TestDisplay(
            IGameDataProvider<int> gameDataProvider,
            IDataConverter<int, DisplayUpdate> dataConverter,
            IDisplayUpdateSender displayUpdateSender)
            : base(gameDataProvider, dataConverter, displayUpdateSender)
        {
        }

        public override string Description => "TestDisplay";
        public override bool IsActive => true;
    }

    private sealed class EmittingGameDataProvider : IGameDataProvider<int>
    {
        public event EventHandler<int>? DataReceived;

        public void Start() { }
        public void Stop() { }
        public void Emit(int value) => DataReceived?.Invoke(this, value);
    }

    private sealed class ThrowOnValueConverter : IDataConverter<int, DisplayUpdate>
    {
        private readonly int _throwOnValue;

        public ThrowOnValueConverter(int throwOnValue) => _throwOnValue = throwOnValue;

        public DisplayUpdate Convert(int input)
        {
            if (input == _throwOnValue)
            {
                throw new InvalidOperationException("Conversion failed");
            }

            return new DisplayUpdate { Type = DisplayType.RaceDashboard };
        }
    }

    private sealed class PassthroughConverter : IDataConverter<int, DisplayUpdate>
    {
        public DisplayUpdate Convert(int input) => new() { Type = DisplayType.RaceDashboard };
    }

    private sealed class RecordingSender : IDisplayUpdateSender
    {
        private readonly TaskCompletionSource _signal =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<DisplayUpdate> Updates { get; } = [];

        public Task SendDisplayUpdate(DisplayUpdate displayUpdate)
        {
            lock (Updates)
            {
                Updates.Add(displayUpdate);
            }

            _signal.TrySetResult();
            return Task.CompletedTask;
        }

        public async Task WaitForUpdatesAsync(int count, TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                lock (Updates)
                {
                    if (Updates.Count >= count)
                    {
                        return;
                    }
                }

                await Task.Delay(10);
            }
        }
    }

    private sealed class ThrowingSender : IDisplayUpdateSender
    {
        public Task SendDisplayUpdate(DisplayUpdate displayUpdate)
            => throw new InvalidOperationException("Send failed");
    }

    private sealed class StubHubContext : IHubContext<GameDataHub>
    {
        public IHubClients Clients => throw new NotSupportedException();

        public IGroupManager Groups => throw new NotSupportedException();
    }
}

using HaddySimHub.Displays;

namespace HaddySimHub.Tests;

[TestClass]
public class SharedMemoryGameDataProviderTests
{
    [TestMethod]
    public async Task Start_RetriesUntilSharedMemoryBecomesAvailable()
    {
        var reader = new TestReader(connectSuccessAttempt: 2) { Value = 42 };
        var provider = new TestProvider(reader);
        var received = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        provider.DataReceived += (_, telemetry) => received.TrySetResult(telemetry);

        provider.Start();

        var completed = await Task.WhenAny(received.Task, Task.Delay(1000));
        provider.Stop();

        Assert.AreSame(received.Task, completed);
        Assert.AreEqual(42, await received.Task);
        Assert.IsTrue(reader.ConnectAttempts >= 2);
    }

    [TestMethod]
    public void Stop_DisposesTheReader()
    {
        var reader = new TestReader(connectSuccessAttempt: 1);
        var provider = new TestProvider(reader);

        provider.Start();
        provider.Stop();

        Assert.IsTrue(reader.Disposed);
    }

    private sealed class TestReader(int connectSuccessAttempt) : ISharedMemoryTelemetryReader<int>
    {
        public int ConnectAttempts { get; private set; }

        public bool IsConnected { get; private set; }

        public bool Disposed { get; private set; }

        public int Value { get; set; }

        public void Connect()
        {
            ConnectAttempts++;
            if (ConnectAttempts >= connectSuccessAttempt)
            {
                IsConnected = true;
            }
        }

        public void Disconnect()
        {
            IsConnected = false;
        }

        public bool TryReadTelemetry(out int telemetry)
        {
            telemetry = Value;
            return IsConnected;
        }

        public void Dispose()
        {
            Disposed = true;
            Disconnect();
        }
    }

    private sealed class TestProvider(TestReader reader) : SharedMemoryGameDataProviderBase<TestReader, int>
    {
        protected override TestReader CreateReader() => reader;

        protected override bool HasDataChanged(int current, int last) => current != last;
    }
}

using HaddySimHub.Displays;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Tests;

[TestClass]
public class SharedMemoryGameDataProviderTests
{
    [TestMethod]
    public async Task Start_RetriesUntilSharedMemoryBecomesAvailable()
    {
        var reader = new TestReader { Value = 42 };
        var provider = new RetryingProvider(reader, connectSuccessAttempt: 2);
        var received = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        provider.DataReceived += (_, telemetry) => received.TrySetResult(telemetry);

        provider.Start();

        var completed = await Task.WhenAny(received.Task, Task.Delay(1000));
        provider.Stop();

        Assert.AreSame(received.Task, completed);
        Assert.AreEqual(42, await received.Task);
        Assert.IsTrue(reader.ConnectAttempts >= 2);
    }

    private sealed class TestReader
    {
        public int ConnectAttempts { get; set; }
        public bool IsConnected { get; set; }
        public int Value { get; set; }
    }

    private sealed class RetryingProvider : SharedMemoryGameDataProviderBase<TestReader, int>
    {
        private readonly TestReader _reader;
        private readonly int _connectSuccessAttempt;

        public RetryingProvider(TestReader reader, int connectSuccessAttempt)
        {
            _reader = reader;
            _connectSuccessAttempt = connectSuccessAttempt;
        }

        protected override TestReader CreateReader()
        {
            return _reader;
        }

        protected override void ConnectReader(TestReader reader)
        {
            reader.ConnectAttempts++;
            if (reader.ConnectAttempts >= _connectSuccessAttempt)
            {
                reader.IsConnected = true;
            }
        }

        protected override void DisconnectReader(TestReader? reader)
        {
            if (reader != null)
            {
                reader.IsConnected = false;
            }
        }

        protected override bool IsConnected(TestReader? reader)
        {
            return reader?.IsConnected ?? false;
        }

        protected override bool TryReadTelemetry(TestReader reader, out int telemetry)
        {
            telemetry = reader.Value;
            return reader.IsConnected;
        }

        protected override bool HasDataChanged(int current, int last)
        {
            return current != last;
        }
    }
}

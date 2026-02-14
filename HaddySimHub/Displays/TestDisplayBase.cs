using HaddySimHub.Models;
using HaddySimHub.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using HaddySimHub.Services; // Assuming IdentityDataConverter is in HaddySimHub.Services

namespace HaddySimHub.Displays
{
    public abstract class TestDisplayBase : DisplayBase<DisplayUpdate>
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private CancellationToken _cancellationToken;
        protected static readonly Random _random = System.Random.Shared;
        private readonly string _id; // Store id as a field

        public override string Description => $"Test display: {_id}";
        public override bool IsActive => Program.TestId == _id;

        // Traditional constructor
        public TestDisplayBase(
            string id,
            IDataConverter<DisplayUpdate, DisplayUpdate> identityDataConverter,
            IDisplayUpdateSender displayUpdateSender)
            : base(new TestGameDataProviderWrapper(id), identityDataConverter, displayUpdateSender) // Pass a wrapper for IGameDataProvider
        {
            _id = id; // Initialize the id field
        }

        public override void Start()
        {
            _cancellationToken = _cancellationTokenSource.Token;
            Task.Run(async () =>
            {
                try
                {
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        // Directly invoke DataReceived event on the wrapped provider
                        (_gameDataProvider as TestGameDataProviderWrapper)?.InvokeDataReceived(this.GenerateDisplayUpdate());
                        await Task.Delay(TimeSpan.FromSeconds(.5), _cancellationToken);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Handle cancellation
                }
                catch (Exception ex)
                {
                    Logger.Error($"{ex.Message}\n\n{ex.StackTrace}");
                }
            });

            // Call base Start which will subscribe to DataReceived of the wrapped provider.
            base.Start();
        }

        public override void Stop()
        {
            _cancellationTokenSource.Cancel();
            base.Stop();
        }

        protected abstract DisplayUpdate GenerateDisplayUpdate();

        // Internal wrapper class to provide IGameDataProvider functionality for TestDisplayBase
        private class TestGameDataProviderWrapper : IGameDataProvider<DisplayUpdate>
        {
            private readonly string _id;

            public TestGameDataProviderWrapper(string id)
            {
                _id = id;
            }

            public event EventHandler<DisplayUpdate>? DataReceived;

            public void Start()
            {
                // No specific action needed here for this wrapper, actual data generation is in TestDisplayBase.Start()
            }

            public void Stop()
            {
                // No specific action needed here for this wrapper
            }

            public void InvokeDataReceived(DisplayUpdate data)
            {
                DataReceived?.Invoke(this, data);
            }
        }
    }
}

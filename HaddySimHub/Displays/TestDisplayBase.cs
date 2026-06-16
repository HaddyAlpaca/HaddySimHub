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
        private readonly object _sync = new();
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _generatorTask;
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
            lock (_sync)
            {
                if (_generatorTask is { IsCompleted: false })
                {
                    return;
                }

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                // Call base Start which will subscribe to DataReceived of the wrapped provider.
                base.Start();

                _generatorTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            // Directly invoke DataReceived event on the wrapped provider
                            (_gameDataProvider as TestGameDataProviderWrapper)?.InvokeDataReceived(this.GenerateDisplayUpdate());
                            await Task.Delay(TimeSpan.FromSeconds(.5), token);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Expected when stopping.
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when stopping.
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"{ex.Message}\n\n{ex.StackTrace}");
                    }
                }, token);
            }
        }

        public override void Stop()
        {
            CancellationTokenSource? cancellationTokenSource;
            Task? generatorTask;

            lock (_sync)
            {
                cancellationTokenSource = _cancellationTokenSource;
                generatorTask = _generatorTask;
                _cancellationTokenSource = null;
                _generatorTask = null;
            }

            cancellationTokenSource?.Cancel();
            if (generatorTask is not null)
            {
                try
                {
                    generatorTask.Wait(TimeSpan.FromSeconds(1));
                }
                catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException || e is OperationCanceledException))
                {
                }
            }
            cancellationTokenSource?.Dispose();
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

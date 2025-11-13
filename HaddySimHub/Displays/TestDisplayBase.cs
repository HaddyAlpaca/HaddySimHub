using HaddySimHub.Models;

namespace HaddySimHub.Displays
{
    public abstract class TestDisplayBase(string id) : DisplayBase<DisplayUpdate>
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private CancellationToken _cancellationToken;
        // Central shared RNG for test displays to avoid repeated allocations.
        protected static readonly Random _random = System.Random.Shared;

        public override string Description => $"Test display: {id}";
        public override bool IsActive => Program.TestId == id;

        public override void Start()
        {
            this._cancellationToken = this._cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                try
                {
                    while (!this._cancellationToken.IsCancellationRequested)
                    {
                        await this.SendUpdate(this.GenerateDisplayUpdate());
                        await Task.Delay(TimeSpan.FromSeconds(.5), this._cancellationToken);
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

        }

        public override void Stop()
        {
            this._cancellationTokenSource.Cancel();
        }

        protected override DisplayUpdate ConvertToDisplayUpdate(DisplayUpdate data) => data;

        protected abstract DisplayUpdate GenerateDisplayUpdate();
    }
}

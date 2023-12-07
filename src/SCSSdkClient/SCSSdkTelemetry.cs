using SCSSdkClient.Object;

namespace SCSSdkClient
{

    /// <summary>
    ///     Data Event
    ///
    ///     The parameter **newTimeStamp** is deprecated and will be removed in a future release.
    ///
    /// </summary>
    ///
    ///
    /// <param name="data">All data of the telemetry</param>
    /// <param name="newTimestamp">Flag if the data changed</param>
    public delegate void TelemetryData(SCSTelemetry data, bool newTimestamp);

    /// <summary>
    ///     Handle the SCSSdkTelemetry.
    ///     Currently IDisposable. Was implemented because of an error
    /// </summary>
    public class SCSSdkTelemetry : IDisposable {
        private const string DefaultSharedMemoryMap = "Local\\SCSTelemetry";
        private const int DefaultUpdateInterval = 20;
        private const int DefaultPausedUpdateInterval = 1000;

        private Timer? _updateTimer;

        private ulong lastTime = 0xFFFFFFFFFFFFFFFF;

        public void Dispose() => _updateTimer?.Dispose();

        private readonly SharedMemory SharedMemory;

        private bool wasOnJob;
        private bool cancelled;
        private bool delivered;
        private bool fined;
        private bool tollgate;
        private bool ferry;
        private bool train;
        private bool paused;
        private bool refuel;
        private bool refuelPayed;
        private bool wasPaused;

        public SCSSdkTelemetry()
        {
            // Set up SCS telemetry provider.
            // Connects to shared memory map, sets up timebase.
            SharedMemory = new SharedMemory();
            SharedMemory.Connect(DefaultSharedMemoryMap);

            if (!SharedMemory.Hooked) {
                Error = SharedMemory.HookException;
                return;
            }

            var tsInterval = new TimeSpan(0, 0, 0, 0, DefaultUpdateInterval);

            _updateTimer = new Timer(_updateTimer_Elapsed, null, tsInterval.Add(tsInterval), tsInterval);
        }

        public int UpdateInterval => paused ? DefaultPausedUpdateInterval : DefaultUpdateInterval;

        public Exception? Error { get; private set; }

        public event TelemetryData? Data;

        public event EventHandler? JobStarted;

        public event EventHandler? JobCancelled;

        public event EventHandler? JobDelivered;

        public event EventHandler? Fined;

        public event EventHandler? Tollgate;

        public event EventHandler? Ferry;

        public event EventHandler? Train;

        public event EventHandler? RefuelStart;

        public event EventHandler? RefuelEnd;

        public event EventHandler? RefuelPayed;

        public void Resume()
        {
            var tsInterval = new TimeSpan(0, 0, 0, 0, UpdateInterval);
            _updateTimer!.Change(tsInterval, tsInterval);
        }

        private void _updateTimer_Elapsed(object? sender) {
            var scsTelemetry = SharedMemory.Update<SCSTelemetry>();

            if (scsTelemetry == null) {
                return;
            }

            // check if sdk is NOT running
            if (!scsTelemetry.SdkActive && !paused) {
                // if so don't check so often the data
                var tsInterval = new TimeSpan(0, 0, 0, 0, DefaultPausedUpdateInterval);
                _updateTimer!.Change(tsInterval.Add(tsInterval), tsInterval);
                paused = true;

                // if sdk not active we don't need to do something
                return;
            }

            if (paused && scsTelemetry.SdkActive) {
                // ok sdk is active now
                paused = false;
                Resume(); // going back to normal update rate
            }

            var time = scsTelemetry.Timestamp;
            var updated = false;

            if (time != lastTime || wasPaused != scsTelemetry.Paused) {
                // time changed or game state change -> update data
                Data?.Invoke(scsTelemetry, true);
                wasPaused = scsTelemetry.Paused;
                lastTime = time;
                updated = true;
            }

            //TODO: make it nicer thats a lot of code for such less work
            // Job start event
            if (wasOnJob != scsTelemetry.SpecialEventsValues.OnJob) {
                wasOnJob = scsTelemetry.SpecialEventsValues.OnJob;
                if (wasOnJob) {
                    if (!updated) {
                        Data?.Invoke(scsTelemetry, true);
                        updated = true;
                    }

                    JobStarted?.Invoke(this, new EventArgs());
                }
            }

            if (cancelled != scsTelemetry.SpecialEventsValues.JobCancelled) {
                cancelled = scsTelemetry.SpecialEventsValues.JobCancelled;

                if (!updated) {
                    Data?.Invoke(scsTelemetry, true);
                    updated = true;
                }

                JobCancelled?.Invoke(this, new EventArgs());
            }

            if (delivered != scsTelemetry.SpecialEventsValues.JobDelivered) {
                delivered = scsTelemetry.SpecialEventsValues.JobDelivered;

                if (!updated) {
                    Data?.Invoke(scsTelemetry, true);
                    updated = true;
                }

                JobDelivered?.Invoke(this, new EventArgs());
            }

            if (fined != scsTelemetry.SpecialEventsValues.Fined) {
                fined = scsTelemetry.SpecialEventsValues.Fined;

                Fined?.Invoke(this, new EventArgs());
            }

            if (tollgate != scsTelemetry.SpecialEventsValues.Tollgate) {
                tollgate = scsTelemetry.SpecialEventsValues.Tollgate;

                Tollgate?.Invoke(this, new EventArgs());
            }

            if (ferry != scsTelemetry.SpecialEventsValues.Ferry) {
                ferry = scsTelemetry.SpecialEventsValues.Ferry;

                if (!updated) {
                    Data?.Invoke(scsTelemetry, true);
                    updated = true;
                }

                Ferry?.Invoke(this, new EventArgs());
            }

            if (train != scsTelemetry.SpecialEventsValues.Train) {
                train = scsTelemetry.SpecialEventsValues.Train;

                if (!updated) {
                    Data?.Invoke(scsTelemetry, true);
                    updated = true;
                }

                Train?.Invoke(this, new EventArgs());
            }

            if (refuel != scsTelemetry.SpecialEventsValues.Refuel) {
                refuel = scsTelemetry.SpecialEventsValues.Refuel;

                if (scsTelemetry.SpecialEventsValues.Refuel) {
                    RefuelStart?.Invoke(this, new EventArgs());
                } else {
                    RefuelEnd?.Invoke(this, new EventArgs());
                }
            }

            if (refuelPayed != scsTelemetry.SpecialEventsValues.RefuelPayed) {
                refuelPayed = scsTelemetry.SpecialEventsValues.RefuelPayed;

                if (scsTelemetry.SpecialEventsValues.RefuelPayed) {
                    RefuelPayed?.Invoke(this, new EventArgs());
                }
            }

            // currently the design is that the event is called, doesn't matter if data changed
            // also the old demo didn't used the flag and expected to be refreshed each call
            // so without making a big change also call the event without update with false flag
            if (!updated) {
                Data?.Invoke(scsTelemetry, false);
            }
        }
    }
}

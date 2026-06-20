using HaddySimHub.Models;
using HaddySimHub.Interfaces;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HaddySimHub.Displays;

public abstract class DisplayBase<T> : IDisplay
{
    private const int ChannelCapacity = 256;

    private readonly object _sync = new();
    private Channel<DisplayUpdate>? _updateChannel;
    private Task? _sendLoopTask;
    private bool _isStarted;

    private long _lastUpdateTicks;
    private bool _firstDataLogged;
    private DateTime _lastFlowLogUtc = DateTime.MinValue;
    private long _droppedFrames;
    private DateTime _lastDropLogUtc = DateTime.MinValue;

    protected readonly IGameDataProvider<T> _gameDataProvider;
    protected readonly IDataConverter<T, DisplayUpdate> _dataConverter;
    protected readonly IDisplayUpdateSender _displayUpdateSender;

    public abstract string Description { get; }
    public abstract bool IsActive { get; }

    public DateTime? LastUpdateUtc
    {
        get
        {
            var ticks = Interlocked.Read(ref _lastUpdateTicks);
            return ticks == 0 ? null : new DateTime(ticks, DateTimeKind.Utc);
        }
    }

    public DisplayBase(
        IGameDataProvider<T> gameDataProvider,
        IDataConverter<T, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
    {
        _gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));
        _dataConverter = dataConverter ?? throw new ArgumentNullException(nameof(dataConverter));
        _displayUpdateSender = displayUpdateSender ?? throw new ArgumentNullException(nameof(displayUpdateSender));
    }

    public virtual void Start()
    {
        lock (_sync)
        {
            if (_isStarted)
            {
                return;
            }

            var updateChannel = Channel.CreateBounded<DisplayUpdate>(new BoundedChannelOptions(ChannelCapacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });
            _updateChannel = updateChannel;

            Interlocked.Exchange(ref _lastUpdateTicks, 0);
            Interlocked.Exchange(ref _droppedFrames, 0);
            _firstDataLogged = false;
            _lastFlowLogUtc = DateTime.MinValue;
            _lastDropLogUtc = DateTime.MinValue;

            _gameDataProvider.DataReceived += HandleDataReceived;
            _gameDataProvider.Start();
            _sendLoopTask = Task.Run(() => SendLoopAsync(updateChannel));
            _isStarted = true;
        }
    }

    public virtual void Stop()
    {
        Channel<DisplayUpdate>? updateChannel;
        Task? sendLoopTask;

        lock (_sync)
        {
            if (!_isStarted)
            {
                return;
            }

            _isStarted = false;
            _gameDataProvider.DataReceived -= HandleDataReceived;
            _gameDataProvider.Stop();

            Interlocked.Exchange(ref _lastUpdateTicks, 0);
            _firstDataLogged = false;

            updateChannel = _updateChannel;
            sendLoopTask = _sendLoopTask;
            _updateChannel = null;
            _sendLoopTask = null;
        }

        updateChannel?.Writer.TryComplete();
        if (sendLoopTask is not null)
        {
            try
            {
                sendLoopTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException || e is OperationCanceledException))
            {
            }
        }
    }

    private async Task SendLoopAsync(Channel<DisplayUpdate> updateChannel)
    {
        try
        {
            await foreach (var update in updateChannel.Reader.ReadAllAsync())
            {
                await _displayUpdateSender.SendDisplayUpdate(update);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in display send loop: {ex.Message}");
        }
    }

    private void HandleDataReceived(object? sender, T data)
    {
        try
        {
            if (Logger.IsDataLoggingEnabled && data is not null)
            {
                Logger.LogData(data);
            }

            var update = _dataConverter.Convert(data);

            var nowUtc = DateTime.UtcNow;
            Interlocked.Exchange(ref _lastUpdateTicks, nowUtc.Ticks);

            if (!_firstDataLogged)
            {
                _firstDataLogged = true;
                Logger.Info($"First telemetry received from {Description}");
            }
            else if ((nowUtc - _lastFlowLogUtc).TotalSeconds >= 5)
            {
                _lastFlowLogUtc = nowUtc;
                Logger.Debug($"Telemetry flowing from {Description}");
            }

            var channel = _updateChannel;
            if (channel is null)
            {
                return;
            }

            if (channel.Reader.CanCount && channel.Reader.Count >= ChannelCapacity)
            {
                var dropped = Interlocked.Increment(ref _droppedFrames);
                if ((nowUtc - _lastDropLogUtc).TotalSeconds >= 5)
                {
                    _lastDropLogUtc = nowUtc;
                    Logger.Warn($"Dropping telemetry frames for {Description} (buffer full, {dropped} dropped so far) - consumer not keeping up or no client connected");
                }
            }

            channel.Writer.TryWrite(update);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in HandleDataReceived: {ex.Message}");
        }
    }

}

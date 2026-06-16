using HaddySimHub.Models;
using HaddySimHub.Interfaces;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HaddySimHub.Displays;

public abstract class DisplayBase<T> : IDisplay
{
    private readonly object _sync = new();
    private Channel<DisplayUpdate>? _updateChannel;
    private Task? _sendLoopTask;
    private bool _isStarted;

    protected readonly IGameDataProvider<T> _gameDataProvider;
    protected readonly IDataConverter<T, DisplayUpdate> _dataConverter;
    protected readonly IDisplayUpdateSender _displayUpdateSender;

    public abstract string Description { get; }
    public abstract bool IsActive { get; }

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

            var updateChannel = Channel.CreateBounded<DisplayUpdate>(new BoundedChannelOptions(256)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });
            _updateChannel = updateChannel;

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
            var update = _dataConverter.Convert(data);
            _updateChannel?.Writer.TryWrite(update);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in HandleDataReceived: {ex.Message}");
        }
    }

}

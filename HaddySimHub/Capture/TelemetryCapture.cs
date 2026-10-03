using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace HaddySimHub.Capture;

/// <summary>
/// Writes raw game-source bytes to JSON Lines files without projecting or
/// converting telemetry.
/// </summary>
public sealed class TelemetryCapture : IDisposable
{
    private const int QueueCapacity = 128;
    private const long MaxQueuedBytes = 64L * 1024 * 1024;

    private readonly string? _directory;
    private readonly string? _statusPath;
    private readonly string? _captureId;
    private readonly DateTimeOffset _startedAtUtc;
    private readonly ILogger<TelemetryCapture> _logger;
    private readonly Channel<RawFrame>? _queue;
    private readonly Task? _writerTask;
    private readonly Dictionary<string, int> _framesWritten = [];
    private readonly Dictionary<string, int> _nextSequences = [];
    private string? _failure;
    private int _disposed;
    private long _queuedBytes;
    private bool _storageFailed;

    public TelemetryCapture(string? directory, ILogger<TelemetryCapture> logger)
    {
        _directory = directory;
        _logger = logger;
        Enabled = directory is not null;
        if (!Enabled)
        {
            return;
        }

        _startedAtUtc = DateTimeOffset.UtcNow;
        _captureId = Guid.NewGuid().ToString("N");
        _statusPath = Path.Combine(directory!, $"capture-status-{_captureId}.json");
        Directory.CreateDirectory(directory!);
        WriteStatus("capturing", completedAtUtc: null);

        _queue = Channel.CreateBounded<RawFrame>(new BoundedChannelOptions(QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
        _writerTask = Task.Run(WriteFramesAsync);
    }

    public bool Enabled { get; }

    /// <summary>
    /// Marks the run unusable when a source-level discontinuity is detected.
    /// </summary>
    public void MarkIncomplete(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (Enabled)
        {
            SetFailure(reason);
        }
    }

    /// <summary>
    /// Enqueues the immutable byte arrays read from a game source. Callers must
    /// not modify those arrays after this method returns.
    /// </summary>
    public void RecordRawFrame(
        string game,
        string transport,
        string sourceName,
        IReadOnlyDictionary<string, byte[]> pages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(game);
        ArgumentException.ThrowIfNullOrWhiteSpace(transport);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        ArgumentNullException.ThrowIfNull(pages);

        if (!Enabled)
        {
            return;
        }
        if (pages.Count == 0)
        {
            throw new ArgumentException("A raw capture frame must contain at least one page.", nameof(pages));
        }
        long byteLength = 0;
        foreach (var (name, bytes) in pages)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(bytes);
            byteLength = checked(byteLength + bytes.Length);
        }
        if (Volatile.Read(ref _disposed) != 0 || Volatile.Read(ref _failure) is not null)
        {
            return;
        }

        if (Interlocked.Add(ref _queuedBytes, byteLength) > MaxQueuedBytes)
        {
            Interlocked.Add(ref _queuedBytes, -byteLength);
            SetFailure("queue-capacity-exceeded");
            return;
        }

        var timestamp = DateTimeOffset.UtcNow;
        var frame = new RawFrame(game, transport, sourceName, pages, timestamp, byteLength);
        if (!_queue!.Writer.TryWrite(frame))
        {
            Interlocked.Add(ref _queuedBytes, -byteLength);
            SetFailure("queue-overflow");
        }
    }

    private async Task WriteFramesAsync()
    {
        try
        {
            await foreach (var frame in _queue!.Reader.ReadAllAsync())
            {
                try
                {
                    if (_storageFailed)
                    {
                        continue;
                    }

                    WriteFrame(frame);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
                {
                    _storageFailed = true;
                    SetFailure($"write-failed:{frame.Game}");
                    _logger.LogError(ex, "Raw {Game} capture failed while writing", frame.Game);
                }
                finally
                {
                    Interlocked.Add(ref _queuedBytes, -frame.ByteLength);
                }
            }
        }
        catch (Exception ex)
        {
            _storageFailed = true;
            SetFailure("writer-failed");
            _logger.LogError(ex, "Raw capture writer stopped unexpectedly");
        }
        finally
        {
            try
            {
                WriteStatus(
                    Volatile.Read(ref _failure) is null ? "complete" : "incomplete",
                    DateTimeOffset.UtcNow);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Cannot finalize raw capture status at {Path}", _statusPath);
            }
        }
    }

    private void WriteFrame(RawFrame frame)
    {
        var path = Path.Combine(_directory!, $"{frame.Game}.jsonl");
        var nextSequence = _nextSequences.GetValueOrDefault(frame.Game);
        if (!_nextSequences.ContainsKey(frame.Game) && File.Exists(path))
        {
            nextSequence = CountExistingFrames(path);
        }

        var raw = new JsonObject
        {
            ["schemaVersion"] = 1,
            ["transport"] = frame.Transport,
            ["sourceName"] = frame.SourceName,
            ["pages"] = new JsonObject(),
        };
        var rawPages = raw["pages"]!.AsObject();
        var integrityPages = new JsonObject();
        foreach (var (name, bytes) in frame.Pages)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(bytes);
            rawPages[name] = Convert.ToBase64String(bytes);
            integrityPages[name] = new JsonObject
            {
                ["byteLength"] = bytes.Length,
                ["sha256"] = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
            };
        }

        var record = new JsonObject
        {
            ["seq"] = nextSequence,
            ["captureId"] = _captureId,
            ["capturedAtUtc"] = frame.TimestampUtc,
            ["raw"] = raw,
            ["integrity"] = new JsonObject { ["pages"] = integrityPages },
        };
        var json = record.ToJsonString();

        using var stream = new FileStream(
            path,
            FileMode.Append,
            FileAccess.Write,
            FileShare.Read,
            bufferSize: 64 * 1024,
            FileOptions.SequentialScan);
        using var writer = new StreamWriter(stream);
        writer.WriteLine(json);
        writer.Flush();
        stream.Flush(flushToDisk: true);

        _nextSequences[frame.Game] = nextSequence + 1;
        _framesWritten[frame.Game] = _framesWritten.GetValueOrDefault(frame.Game) + 1;
    }

    private static int CountExistingFrames(string path)
    {
        var count = 0;
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            if (!document.RootElement.TryGetProperty("seq", out var sequence) ||
                sequence.GetInt32() != count)
            {
                throw new JsonException($"Capture sequence is invalid in {path} at frame {count}.");
            }
            count++;
        }

        return count;
    }

    private void SetFailure(string failure)
    {
        if (Interlocked.CompareExchange(ref _failure, failure, null) is null)
        {
            _logger.LogError("Raw capture is incomplete: {Reason}", failure);
            _queue?.Writer.TryComplete();
        }
    }

    private void WriteStatus(string status, DateTimeOffset? completedAtUtc)
    {
        var contents = JsonSerializer.SerializeToUtf8Bytes(new
        {
            formatVersion = 1,
            captureId = _captureId,
            status,
            startedAtUtc = _startedAtUtc,
            completedAtUtc,
            failure = Volatile.Read(ref _failure),
            framesWritten = _framesWritten,
        });
        var temporaryPath = $"{_statusPath}.tmp";
        using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            stream.Write(contents);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temporaryPath, _statusPath!, overwrite: true);
    }

    public void Dispose()
    {
        if (!Enabled || Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _queue!.Writer.TryComplete();
        _writerTask!.GetAwaiter().GetResult();
    }

    private sealed record RawFrame(
        string Game,
        string Transport,
        string SourceName,
        IReadOnlyDictionary<string, byte[]> Pages,
        DateTimeOffset TimestampUtc,
        long ByteLength);
}

using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace HaddySimHub.Displays;

/// <summary>
/// One named memory mapped page, marshalled into <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// <para>
/// The struct is read as a raw memory image, so its field order and size are part
/// of the contract with the game. Only <c>Marshal.SizeOf&lt;T&gt;()</c> bytes are
/// mapped, which means a struct may deliberately stop short of the end of the page
/// when the fields beyond that point differ between game versions.
/// </para>
/// <para>
/// Reading goes through <see cref="Marshal.PtrToStructure{T}(nint)"/> rather than
/// <see cref="MemoryMappedViewAccessor.Read{T}(long, out T)"/>: the pages hold
/// fixed-size arrays and UTF-16 strings, and <c>Read&lt;T&gt;</c> rejects any
/// struct containing references.
/// </para>
/// </remarks>
public sealed class SharedMemoryPage<T> : IDisposable
    where T : struct
{
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);

    private readonly string _mapName;
    private readonly string _description;
    private readonly int _size = Marshal.SizeOf<T>();

    private MemoryMappedFile? _file;
    private MemoryMappedViewAccessor? _accessor;
    private DateTime _lastAttemptUtc = DateTime.MinValue;

    /// <param name="mapName">The name the game publishes the page under.</param>
    /// <param name="description">How the page is named in log output, e.g. <c>"[AC] physics"</c>.</param>
    public SharedMemoryPage(string mapName, string description)
    {
        _mapName = mapName ?? throw new ArgumentNullException(nameof(mapName));
        _description = description ?? throw new ArgumentNullException(nameof(description));
    }

    public bool IsConnected => _accessor is not null;

    /// <summary>
    /// Maps the page if it is published. Attempts are rate limited, because opening
    /// a page that is not there throws and providers poll far faster than a game starts.
    /// </summary>
    public void TryOpen()
    {
        if (IsConnected || DateTime.UtcNow - _lastAttemptUtc < RetryInterval)
        {
            return;
        }

        _lastAttemptUtc = DateTime.UtcNow;

        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            _file = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.Read);
#pragma warning restore CA1416 // Validate platform compatibility
            _accessor = _file.CreateViewAccessor(0, _size, MemoryMappedFileAccess.Read);
        }
        catch (FileNotFoundException)
        {
            // The game is not running, or has not published this page yet.
            Close();
        }
        catch (UnauthorizedAccessException)
        {
            Logger.Debug($"{_description}: access denied to {_mapName}");
            Close();
        }
        catch (Exception ex)
        {
            Logger.Debug($"{_description}: failed to map {_mapName}: {ex.GetType().Name}: {ex.Message}");
            Close();
        }
    }

    /// <exception cref="InvalidOperationException">The page is not mapped.</exception>
    public T Read()
    {
        var accessor = _accessor ?? throw new InvalidOperationException($"{_description} is not mapped.");

        var buffer = new byte[_size];
        accessor.ReadArray(0, buffer, 0, buffer.Length);

        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }

    public void Close()
    {
        _accessor?.Dispose();
        _accessor = null;
        _file?.Dispose();
        _file = null;
    }

    public void Dispose()
    {
        Close();
    }
}

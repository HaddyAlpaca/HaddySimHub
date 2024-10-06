using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace HaddySimHub.Server.Games;

public interface ISharedMemoryReader<T>
    where T : struct
{
    void Dispose();

    T Read();
}

public class SharedMemoryReader<T> : IDisposable, ISharedMemoryReader<T>
    where T : struct
{
    private readonly string mapName;

    private MemoryMappedFile? file;
    private MemoryMappedViewAccessor? viewAccessor;

    public SharedMemoryReader(string mapName)
    {
        this.mapName = mapName ?? throw new ArgumentNullException(nameof(mapName));
    }

    public T Read()
    {
        // Open memory mapped file if not open
        if (this.file == null)
        {
            try
            {
#pragma warning disable CA1416 // Validate platform compatibility
                this.file = MemoryMappedFile.OpenExisting(this.mapName, MemoryMappedFileRights.Read);
#pragma warning restore CA1416 // Validate platform compatibility
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException)
                {
                    // Mapped memory file not found
                    return default;
                }

                throw;
            }
        }

        // Create view accessor
        if (this.viewAccessor == null)
        {
            try
            {
                this.viewAccessor = this.file.CreateViewAccessor(0, Marshal.SizeOf(typeof(T)), MemoryMappedFileAccess.Read);
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is UnauthorizedAccessException)
                {
                    // Access failed
                    return default;
                }

                throw;
            }
        }

        // Read the data of the mapped memory file into a byte array
        byte[] data = new byte[Marshal.SizeOf(typeof(T))];
        this.viewAccessor.ReadArray(0, data, 0, data.Length);

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            var result = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            return result == null ? default : (T)result;
        }
        finally
        {
            handle.Free();
        }
    }

    public void Dispose()
    {
        this.viewAccessor?.Dispose();
        this.file?.Dispose();
    }
}
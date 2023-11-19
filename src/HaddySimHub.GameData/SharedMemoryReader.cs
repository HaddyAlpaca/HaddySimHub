using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace HaddySimHub.GameData
{
    public interface ISharedMemoryReader<T> where T : struct
    {
        void Dispose();
        T Read();
    }

    public class SharedMemoryReader<T> : IDisposable, ISharedMemoryReader<T> where T : struct
    {
        private readonly string _mapName;

        private MemoryMappedFile? _file;
        private MemoryMappedViewAccessor? _viewAccessor;

        public SharedMemoryReader(string mapName)
        {
            _mapName = mapName ?? throw new ArgumentNullException(nameof(mapName));
        }

        public T Read()
        {
            //Open memory mapped file if not open
            if (_file == null)
            {
                try
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    _file = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.Read);
#pragma warning restore CA1416 // Validate platform compatibility
                }
                catch (Exception ex)
                {
                    if (ex is FileNotFoundException)
                    {
                        //Mapped memory file not found
                        return default;
                    }

                    throw;
                }
            }

            //Create view accessor
            if (_viewAccessor == null)
            {
                try
                {
                    _viewAccessor = _file.CreateViewAccessor(0, Marshal.SizeOf(typeof(T)), MemoryMappedFileAccess.Read);
                }
                catch (Exception ex)
                {
                    if (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        //Access failed
                        return default;
                    }

                    throw;
                }
            }

            //Read the data of the mapped memory file into a byte array
            byte[] data = new byte[Marshal.SizeOf(typeof(T))];
            _viewAccessor.ReadArray(0, data, 0, data.Length);

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
            _viewAccessor?.Dispose();
            _file?.Dispose();
        }
    }
}
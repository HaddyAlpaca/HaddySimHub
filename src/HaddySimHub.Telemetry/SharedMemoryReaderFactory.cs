namespace HaddySimHub.Telemetry
{
    public interface ISharedMemoryReaderFactory
    {
        ISharedMemoryReader<T> Create<T>(string filename) where T : struct;
    }

    public class SharedMemoryReaderFactory: ISharedMemoryReaderFactory
    {
        public ISharedMemoryReader<T> Create<T>(string filename) where T : struct => 
            new SharedMemoryReader<T>(filename);
    }
}

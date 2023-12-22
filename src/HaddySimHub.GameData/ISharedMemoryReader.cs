namespace HaddySimHub.GameData
{
    public interface ISharedMemoryReader<T>
        where T : struct
    {
        void Dispose();

        T Read();
    }
}
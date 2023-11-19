namespace HaddySimHub.GameData;

public interface IGameDataReader
{
    string ProcessName { get; }

    object ReadRawData();
    object ReadData();
}
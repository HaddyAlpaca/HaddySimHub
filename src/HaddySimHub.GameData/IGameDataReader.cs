namespace HaddySimHub.GameData;

public interface IGameDataReader
{
    event EventHandler<object> RawDataUpdate;

    object Convert(object rawData);
}
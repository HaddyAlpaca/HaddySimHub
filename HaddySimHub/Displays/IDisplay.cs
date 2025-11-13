namespace HaddySimHub.Displays;

public interface IDisplay
{
    string Description { get; }
    bool IsActive { get; }
    void Start();
    void Stop();
}

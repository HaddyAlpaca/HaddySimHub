namespace HaddySimHub.Displays;

internal interface IDisplay
{
    string Description { get; }
    bool IsActive { get; }
    void Start();
    void Stop();
    void NextPage();
}

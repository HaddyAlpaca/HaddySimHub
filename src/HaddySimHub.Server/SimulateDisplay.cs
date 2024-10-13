using HaddySimHub.Server.Models;
using HaddySimHub.Server.Displays;

internal class SimulateDisplay : DisplayBase
{
    private readonly FileSystemWatcher _watcher;

    public override string Description => "Simulate Game";

    public override bool IsActive { get; }

    public SimulateDisplay(Func<object, Func<object, DisplayUpdate>, Task> receivedDataCallBack) : base(receivedDataCallBack)
    {
        string processFolder = Path.GetDirectoryName(Environment.ProcessPath) ?? throw new DirectoryNotFoundException("Process folder cannot be determined");

        this._watcher = new FileSystemWatcher
        {
            Path = processFolder,
            Filter = "display-update.json",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
        };
    }

    public override void Start() => this._watcher.EnableRaisingEvents = true;
    public override void Stop() => this._watcher.EnableRaisingEvents = false;
}
using System.Diagnostics;
using HaddySimHub.GameData;

internal class SimulateGame : Game
{
    private readonly FileSystemWatcher _watcher;

    public override string Description => "Simulate Game";

    protected override string _processName => throw new NotImplementedException();

    protected override Func<object, DisplayUpdate> GetDisplayUpdate => throw new NotImplementedException();

    public bool IsActive { get; set; }

    protected override bool IsGameRunning()
    {
        return this.IsActive;
    }

    public SimulateGame() : base()
    {
        string processFolder = Path.GetDirectoryName(Environment.ProcessPath) ?? throw new DirectoryNotFoundException("Process folder cannot be determined");

        this._watcher = new FileSystemWatcher
        {
            Path = processFolder,
            Filter = "display-update.json",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
        };
    }

    public override void Start()
    {
        base.Start();
        this._watcher.EnableRaisingEvents = true;
    }

    public override void Stop()
    {
        base.Stop();

        this._watcher.EnableRaisingEvents = false;
    }
}
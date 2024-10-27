using HaddySimHub.Server.Models;
using HaddySimHub.Server.Games;

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
        string updatesFile = "display_update.json";
        this._logger.Info($"Start monitoring '{Path.Combine(processFolder, updatesFile)}' for display updates.");

        this._watcher = new FileSystemWatcher
        {
            Path = processFolder,
            Filter = updatesFile,
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
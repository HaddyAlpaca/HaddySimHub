using HaddySimHub.GameData;

public abstract class DisplayBase(Action<DisplayUpdate> updateFn)
{
    private readonly Action<DisplayUpdate> updateFn = updateFn;

    protected abstract DisplayType Type { get; }

    protected void Update(object? data) =>
        this.updateFn(new DisplayUpdate { Type = this.Type, Data = data });
}
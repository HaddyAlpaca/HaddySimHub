using HaddySimHub.Models;

namespace HaddySimHub.Displays;

public abstract class DisplayBase<T> : IDisplay
{
    public abstract string Description { get; }
    public abstract bool IsActive { get; }
    public abstract void Start();
    public abstract void Stop();

    protected async Task SendUpdate(T data)
    {
        var update = this.ConvertToDisplayUpdate(data);
        await GameDataHub.SendDisplayUpdate(update);
    }

    internal abstract DisplayUpdate ConvertToDisplayUpdate(T data);
}

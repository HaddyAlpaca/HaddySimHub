using HaddySimHub.Server.Models;

namespace HaddySimHub.Server.Displays;

internal abstract class DisplayBase
{
    protected readonly Func<object, Func<object, DisplayUpdate>, Task> _receivedDataCallBack;
    public abstract string Description { get; }
    public abstract bool IsActive { get; }
    public abstract void Start();
    public abstract void Stop();

    public DisplayBase(Func<object, Func<object, DisplayUpdate>, Task> receivedDataCallBack)
    {
        this._receivedDataCallBack = receivedDataCallBack;
    }
}

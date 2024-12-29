using HaddySimHub.Shared;

namespace HaddySimHub.Displays;

internal interface IDisplay
{
    string Description { get; }
    bool IsActive { get; }
    void Start();
    void Stop();
}

internal abstract class DisplayBase<T> : IDisplay
{
    protected readonly Func<DisplayUpdate, Task> _updateDisplay;
    public abstract string Description { get; }
    public abstract bool IsActive { get; }
    public abstract void Start();
    public abstract void Stop();

    public DisplayBase(Func<DisplayUpdate, Task> updateDisplay)
    {
        this._updateDisplay = updateDisplay;
    }

    protected abstract DisplayUpdate ConvertToDisplayUpdate(T data);
}

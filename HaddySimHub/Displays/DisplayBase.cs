using HaddySimHub.Models;

namespace HaddySimHub.Displays;

internal abstract class DisplayBase<T> : IDisplay
{
    protected int _pageNumber = 1;
    protected int _pageCount = 1;
    public abstract string Description { get; }
    public abstract bool IsActive { get; }
    public abstract void Start();
    public abstract void Stop();

    public void NextPage()
    {
        if (this._pageNumber >= this._pageCount || this._pageNumber <= 0)
        {
            this._pageNumber = 1;
        }
        else
        {
            this._pageNumber++;
        }
    }

    protected async Task SendUpdate(T data)
    {
        var update = this.ConvertToDisplayUpdate(data);
        await GameDataHub.SendDisplayUpdate(update);
    }

    protected abstract DisplayUpdate ConvertToDisplayUpdate(T data);
}

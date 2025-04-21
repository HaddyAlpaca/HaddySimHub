using HaddySimHub.Models;

namespace HaddySimHub.Displays;

internal abstract class DisplayBase<T> : IDisplay
{
    protected int _page = 1;
    protected virtual int PageCount { get; } = 1;
    public abstract string Description { get; }
    public abstract bool IsActive { get; }
    public abstract void Start();
    public abstract void Stop();

    public void NextPage()
    {
        this._page = (this._page >= this.PageCount || this._page <= 0) ? 1 : this._page + 1;
        Console.WriteLine($"{Description} page {this._page} of {this.PageCount}");
    }

    protected async Task SendUpdate(T data)
    {
        var update = this.ConvertToDisplayUpdate(data);
        update.Page = this._page;
        await GameDataHub.SendDisplayUpdate(update);
    }

    protected abstract DisplayUpdate ConvertToDisplayUpdate(T data);
}

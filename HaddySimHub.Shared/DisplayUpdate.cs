namespace HaddySimHub.Shared;

public sealed record DisplayUpdate
{
    public DisplayType Type { get; init; }

    public object? Data { get; init; }
}

namespace HaddySimHub.Models;

public sealed record DisplayUpdate
{
    public DisplayType Type { get; init; }
    public object? Data { get; init; }
    public int Page { get; set; } = 1;
}

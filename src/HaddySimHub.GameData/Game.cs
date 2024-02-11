public sealed record Game
{
    required public string ProcessName { get; init; }
    required public string Description { get; init; }
    required public Type Type { get; init; }
}
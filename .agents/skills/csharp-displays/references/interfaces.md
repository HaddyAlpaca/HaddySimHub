# Core Interfaces

## IDisplay

```csharp
public interface IDisplay
{
    string Description { get; }
    bool IsActive { get; }
    void Start();
    void Stop();
}
```

## IGameDataProvider<T>

```csharp
public interface IGameDataProvider<T>
{
    event EventHandler<T> DataReceived;
    void Start();
    void Stop();
}
```

## IDataConverter<TInput, TOutput>

```csharp
public interface IDataConverter<TInput, TOutput>
{
    TOutput Convert(TInput input);
}
```

## IDisplayUpdateSender

```csharp
public interface IDisplayUpdateSender
{
    Task SendDisplayUpdate(DisplayUpdate displayUpdate);
}
```

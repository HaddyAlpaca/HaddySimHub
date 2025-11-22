using HaddySimHub.Interfaces;

namespace HaddySimHub.Services;

public class IdentityDataConverter<T> : IDataConverter<T, T>
{
    public T Convert(T input)
    {
        return input;
    }
}

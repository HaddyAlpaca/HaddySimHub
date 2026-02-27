using HaddySimHub.Models;

namespace HaddySimHub.Interfaces;

public interface IHubService
{
    Task SendDisplayUpdateAsync(DisplayUpdate displayUpdate);
}

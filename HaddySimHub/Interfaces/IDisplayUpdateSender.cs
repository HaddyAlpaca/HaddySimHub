using HaddySimHub.Models;

namespace HaddySimHub.Interfaces;

public interface IDisplayUpdateSender
{
    /// <summary>
    /// Sends a DisplayUpdate object to connected clients.
    /// </summary>
    /// <param name="displayUpdate">The DisplayUpdate object to send.</param>
    Task SendDisplayUpdate(DisplayUpdate displayUpdate);
}

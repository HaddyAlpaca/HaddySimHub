using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using System.Threading.Tasks;

namespace HaddySimHub.Services;

public class DisplayUpdateSender : IDisplayUpdateSender
{
    public async Task SendDisplayUpdate(DisplayUpdate displayUpdate)
    {
        await GameDataHub.SendDisplayUpdate(displayUpdate);
    }
}

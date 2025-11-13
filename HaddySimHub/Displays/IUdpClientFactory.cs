using System.Net.Sockets;

namespace HaddySimHub.Displays
{
    public interface IUdpClientFactory
    {
        UdpClient Create(int port);
    }
}

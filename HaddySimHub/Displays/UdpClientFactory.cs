using System.Net.Sockets;

namespace HaddySimHub.Displays
{
    public class UdpClientFactory : IUdpClientFactory
    {
        public UdpClient Create(int port)
        {
            return new UdpClient(port);
        }
    }
}

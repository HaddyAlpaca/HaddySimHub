using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace HaddySimHub.WebServer;

internal class Server
{
    private readonly int portNumber;
    private IWebHost? server = null;

    public Server(int portNumber = 3333)
    {
        this.portNumber = portNumber;
    }

    public void Start(CancellationToken cancellationToken)
    {
        this.server = WebHost.CreateDefaultBuilder().UseKestrel(options =>
        {
            options.ListenAnyIP(this.portNumber);
            options.ListenLocalhost(this.portNumber);
        }).UseStartup<Startup>().UseDefaultServiceProvider((builder, options) => { })
        .Build();

        Task.Run(() => { this.server.RunAsync(); }, cancellationToken);
    }
}
using HaddySimHub.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HaddySimHub.Infrastructure;

public static class WebServerHost
{
    public static WebApplication Create()
    {
        WebApplicationOptions options = new() { ContentRootPath = AppContext.BaseDirectory };
        var builder = WebApplication.CreateBuilder(options);

        builder.WebHost.UseKestrel(options => options.ListenAnyIP(3333));

        builder.Services.AddHaddySimHubApplication();

        var app = builder.Build();
        return app.ConfigureHaddySimHubPipeline();
    }

    public static async Task RunAsync(WebApplication webServer, CancellationToken cancellationToken)
    {
        try
        {
            await webServer.StartAsync(cancellationToken);
            await webServer.WaitForShutdownAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Logger.Fatal($"Web server error: {ex.Message}\n\n{ex.StackTrace}");
        }
        finally
        {
            await webServer.StopAsync(CancellationToken.None);
        }
    }
}

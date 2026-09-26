using HaddySimHub.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HaddySimHub.Infrastructure;

public static class WebServerHost
{
    public static WebApplication Create(bool e2eMode = false)
    {
        WebApplicationOptions options = new() { ContentRootPath = AppContext.BaseDirectory };
        var builder = WebApplication.CreateBuilder(options);

        builder.WebHost.UseKestrel(options =>
        {
            if (e2eMode)
            {
                options.ListenLocalhost(3333);
            }
            else
            {
                options.ListenAnyIP(3333);
            }
        });

        builder.Services.AddHaddySimHubApplication(e2eMode);

        var app = builder.Build();
        return app.ConfigureHaddySimHubPipeline(e2eMode);
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

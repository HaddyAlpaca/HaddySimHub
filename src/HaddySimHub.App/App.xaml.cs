using HaddySimHub.Telemetry;
using HaddySimHub.Telemetry.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HaddySimHub
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IHost? AppHost { get; set; }
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private TelemetryWatcher? watcher;

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<SplashScreenWindow>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<ISharedMemoryReaderFactory, SharedMemoryReaderFactory>();
                    services.AddSingleton<IProcessMonitor, ProcessMonitor>();
                    services.AddSingleton<AssettoCorsa.TelemetryReader>();
                    services.AddSingleton<Ets2.TelemetryReader>();
                    services.AddSingleton<Raceroom.TelemetryReader>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            base.OnStartup(e);

            //Initialize the splash screen and set it as the application main window
            var splashScreen = AppHost.Services.GetRequiredService<SplashScreenWindow>();
            this.MainWindow = splashScreen;
            splashScreen.Show();

            //Get cancellation token
            var token = this.cancellationTokenSource.Token;

            //Start the webserver
            var webServer = new WebServer.Server();
            webServer.Start(token);

            //Create the telemetry readers for the supported games
            var readers = new List<ITelemetryReader>
            {
                AppHost.Services.GetRequiredService<AssettoCorsa.TelemetryReader>(),
                AppHost.Services.GetRequiredService<Raceroom.TelemetryReader>(),
                AppHost.Services.GetRequiredService<Ets2.TelemetryReader>()
            };

            //Start monitoring telemetry
            this.watcher = new TelemetryWatcher(readers, AppHost.Services.GetRequiredService<IProcessMonitor>());
            this.watcher.Start(token);

            //Close the splash screen and create the main window
            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            this.MainWindow = mainWindow;
            splashScreen.Close();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            this.cancellationTokenSource.Cancel();

            await AppHost!.StopAsync();

            base.OnExit(e);
        }
    }
}

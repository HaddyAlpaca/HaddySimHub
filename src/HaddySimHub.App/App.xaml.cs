using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private GameDataWatcher? watcher;

        public App()
        {
            UpdateWebContent();

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<SplashScreenWindow>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<ISharedMemoryReaderFactory, SharedMemoryReaderFactory>();
                    services.AddSingleton<IProcessMonitor, ProcessMonitor>();
                    services.AddSingleton<AssettoCorsa.GameDataReader>();
                    services.AddSingleton<Ets2.GameDataReader>();
                    services.AddSingleton<Raceroom.GameDateReader>();
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

            //Create the game data readers for the supported games
            var readers = new List<IGameDataReader>
            {
                AppHost.Services.GetRequiredService<AssettoCorsa.GameDataReader>(),
                AppHost.Services.GetRequiredService<Raceroom.GameDateReader>(),
                AppHost.Services.GetRequiredService<Ets2.GameDataReader>()
            };

            //Start monitoring game data
            bool rawData = e.Args.Contains("--raw");
            this.watcher = new GameDataWatcher(readers, AppHost.Services.GetRequiredService<IProcessMonitor>());
            this.watcher.Start(rawData, token);

            //Wait some time to show the splash screen
            await Task.Delay(2000);

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

        private static void UpdateWebContent()
        {
            string rootfolder = "wwwroot";
            if (!Directory.Exists(rootfolder))
            {
                try
                {
                    Directory.CreateDirectory(rootfolder);
                }
                catch {}
            }

            if (Directory.Exists(rootfolder))
            {
                //Check if new content needs to be downloaded
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HaddySimHub.DirtRally2;
using HaddySimHub.GameData;
using HaddySimHub.Logging;
using HaddySimHub.WebServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HaddySimHub
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        private readonly CancellationTokenSource cancellationTokenSource = new ();
        private GameWatcher? watcher;
        private ILogger? logger;

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<SplashScreenWindow>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<IProcessMonitor, ProcessMonitor>();
                    services.AddSingleton<ILogger, Logger>();
                })
                .Build();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception exception)
                {
                    this.logger?.Fatal($"Unhandled Exception: {exception.Message}\n\n{exception.StackTrace}");
                }
            };
        }

        private static IHost? AppHost { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Create logger
            bool debugEnabled = e.Args.Contains("--debug");
            this.logger = new Logger(debugEnabled);
            this.logger.Debug("Debugging started...");

            await UpdateWebContent();

            await AppHost!.StartAsync();

            base.OnStartup(e);

            // Initialize the splash screen and set it as the application main window
            var splashScreen = AppHost.Services.GetRequiredService<SplashScreenWindow>();
            this.MainWindow = splashScreen;
            splashScreen.Show();

            // Get cancellation token
            var token = this.cancellationTokenSource.Token;

            // Start the webserver
            var webServer = new WebServer.Server();
            webServer.Start(token);

            var processMonitor = AppHost.Services.GetRequiredService<IProcessMonitor>();

            // Create the list of supported games
            var games = new List<Game>
            {
                new Ets2Game(processMonitor, this.logger, token),
                new IRacingGame(processMonitor, this.logger, token),
                new Dirt2Game(processMonitor, this.logger, token),
            };

            // Start monitoring game data
            this.watcher = new GameWatcher(games);
            this.watcher.Notification += async (sender, message) =>
            {
                await NotificationService.SendNotification(message);
                this.logger.Debug($"Notification: {message}");
            };

            this.watcher.DisplayUpdate += async (sender, update) =>
            {
                await NotificationService.SendDisplayUpdate(update);
                this.logger!.LogData(update);
            };

            // Close the splash screen and create the main window
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

        private static async Task UpdateWebContent()
        {
            string rootfolder = "wwwroot";
            if (!Directory.Exists(rootfolder))
            {
                try
                {
                    Directory.CreateDirectory(rootfolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Client content folder cannot be created: {ex.Message}");
                }
            }

            if (Directory.Exists(rootfolder))
            {
                // Check if new content needs to be downloaded
                string zipFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");

                try
                {
                    const string baseUri = "https://github.com/HaddyAlpaca/HaddySimHubClient";

                    using HttpClient client = new ();
                    string content = await client.GetStringAsync($"{baseUri}/releases/latest");

                    Match match = Regex.Match(content, @"Release v(\S+)");

                    if (match.Success)
                    {
                        string version = match.Groups[1].Value; // Extract text captured by the first group

                        HttpResponseMessage response = await client.GetAsync($"{baseUri}/releases/download/v{version}/haddy-simhub-client.zip");

                        using Stream fileStream = await response.Content.ReadAsStreamAsync();
                        using FileStream outputFileStream = File.Create(zipFile);
                        await fileStream.CopyToAsync(outputFileStream);
                        outputFileStream.Close();

                        // Extract the downloaded ZIP file to the specified folder
                        ZipFile.ExtractToDirectory(zipFile, rootfolder, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking for client update:\n\n{ex.Message}");
                }
                finally
                {
                    if (File.Exists(zipFile))
                    {
                        File.Delete(zipFile); // Delete the temporary ZIP file
                    }
                }
            }
        }
    }
}

﻿using HaddySimHub.GameData;
using HaddySimHub.WebServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<SplashScreenWindow>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<IProcessMonitor, ProcessMonitor>();
                    services.AddSingleton<Ets2.GameDataReader>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await UpdateWebContent();
         
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
            var readers = new Dictionary<string, Type>
            {
                { "eurotrucks2", typeof(Ets2.GameDataReader) },
                { "iracing", typeof(iRacing.GameDataReader) }
            };

            //Start monitoring game data
            var options = new GameDataWatcherOptions
            {
                EmitRawData = e.Args.Contains("--raw"),
                RunDemoMode = e.Args.Contains("--demo")
            };
            bool rawData = e.Args.Contains("--raw");
            this.watcher = new GameDataWatcher(readers, AppHost.Services.GetRequiredService<IProcessMonitor>());
            this.watcher.Start(options, token);
            this.watcher.GameDataIdle += async (sender, e) => { await NotificationService.SendIdle(); };
            this.watcher.RawDataUpdated += async (sender, data) => { await NotificationService.SendRawData(data); };
            this.watcher.TruckDataUpdated += async (sender, data) => { await NotificationService.SendTruckData(data); };
            this.watcher.RaceDataUpdated += async (sender, data) => { await NotificationService.SendRaceData(data); };

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
                //Check if new content needs to be downloaded
                string zipFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");

                try
                {
                    const string baseUri = "https://github.com/HaddyAlpaca/HaddySimHubClient";

                    using HttpClient client = new();
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
                        File.Delete(zipFile); // Delete the temporary ZIP file
                }
            }
        }
    }
}

using HaddySimHub.Displays;
using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Extensions;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class DependencyInjectionTests
    {
        [TestMethod]
        public void AddHaddySimHubApplication_RegistersCoreCompositionRootServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHaddySimHubApplication();
            var provider = services.BuildServiceProvider();

            Assert.IsNotNull(provider.GetRequiredService<DisplaysRunner>());
            Assert.IsNotNull(provider.GetRequiredService<ISseBroadcastService>());
        }

        [TestMethod]
        public void AddHaddySimHubApplication_RegistersExpectedDisplaySet()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHaddySimHubApplication();
            var provider = services.BuildServiceProvider();

            var displays = provider.GetRequiredService<IEnumerable<IDisplay>>().ToList();

            Assert.AreEqual(8, displays.Count);
            Assert.IsTrue(displays.Any(d => d.Description == "Dirt Rally 2"));
            Assert.IsTrue(displays.Any(d => d.Description == "IRacing"));
            Assert.IsTrue(displays.Any(d => d.Description == "Euro Truck Simulator 2"));
            Assert.IsTrue(displays.Any(d => d.Description == "Assetto Corsa"));
            Assert.IsTrue(displays.Any(d => d.Description == "Assetto Corsa Competizione"));
            Assert.IsTrue(displays.Any(d => d.Description == "Assetto Corsa Rally"));
            Assert.IsTrue(displays.Any(d => d.Description == "Microsoft Flight Simulator 2020"));
            Assert.IsTrue(displays.Any(d => d.Description == "Forza Horizon 5"));
        }

        [TestMethod]
        public void AddHaddySimHubApplication_InE2eMode_DoesNotStartDisplayRunner()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHaddySimHubApplication(e2eMode: true);

            Assert.IsFalse(services.Any(service => service.ServiceType == typeof(IHostedService)));
        }

        [TestMethod]
        public async Task ConfigureHaddySimHubPipeline_MapsInjectionRoutesOnlyInE2eMode()
        {
            var normalApp = CreateWebApplication(e2eMode: false);
            var e2eApp = CreateWebApplication(e2eMode: true);

            try
            {
                Assert.IsFalse(HasEndpoint(normalApp, "/__e2e/display-update"));
                Assert.IsFalse(HasEndpoint(normalApp, "/__e2e/health"));
                Assert.IsTrue(HasEndpoint(e2eApp, "/__e2e/display-update"));
                Assert.IsTrue(HasEndpoint(e2eApp, "/__e2e/health"));
            }
            finally
            {
                await normalApp.DisposeAsync();
                await e2eApp.DisposeAsync();
            }
        }

        [TestMethod]
        public void CreateGameDisplay_ThrowsOnInvalidGameDisplayDefinition()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHaddySimHubApplication();
            var provider = services.BuildServiceProvider();

            var ex = Assert.Throws<ArgumentException>(() =>
                DisplayRegistrationExtensions.CreateGameDisplay(provider, new GameDisplayDefinition<Packet>(string.Empty, "Invalid")));
            StringAssert.Contains(ex.Message, "Process name cannot be empty");
        }

        private static WebApplication CreateWebApplication(bool e2eMode)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHaddySimHubApplication(e2eMode);
            return builder.Build().ConfigureHaddySimHubPipeline(e2eMode);
        }

        private static bool HasEndpoint(IEndpointRouteBuilder app, string route)
        {
            return app.DataSources
                .SelectMany(dataSource => dataSource.Endpoints)
                .OfType<RouteEndpoint>()
                .Any(endpoint => endpoint.RoutePattern.RawText == route);
        }
    }
}
using HaddySimHub.Displays;
using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Extensions;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using Microsoft.Extensions.DependencyInjection;
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

            Assert.AreEqual(12, displays.Count);
            Assert.IsTrue(displays.Any(d => d.Description == "Dirt Rally 2"));
            Assert.IsTrue(displays.Any(d => d.Description == "IRacing"));
            Assert.IsTrue(displays.Any(d => d.Description == "Euro Truck Simulator 2"));
            Assert.IsTrue(displays.Any(d => d.Description == "Assetto Corsa"));
            Assert.IsTrue(displays.Any(d => d.Description == "Assetto Corsa Competizione"));
            Assert.IsTrue(displays.Any(d => d.Description == "Assetto Corsa Rally"));
            Assert.IsTrue(displays.Any(d => d.Description == "Microsoft Flight Simulator 2020"));
            Assert.IsTrue(displays.Any(d => d.Description == "Forza Horizon 5"));
            Assert.IsTrue(displays.Any(d => d.Description == $"Test display: {DisplayDefinitions.TestIds.Rally}"));
            Assert.IsTrue(displays.Any(d => d.Description == $"Test display: {DisplayDefinitions.TestIds.Race}"));
            Assert.IsTrue(displays.Any(d => d.Description == $"Test display: {DisplayDefinitions.TestIds.Truck}"));
            Assert.IsTrue(displays.Any(d => d.Description == $"Test display: {DisplayDefinitions.TestIds.Flight}"));
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
    }
}
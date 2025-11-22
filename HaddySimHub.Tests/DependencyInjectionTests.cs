using Microsoft.Extensions.DependencyInjection;
using HaddySimHub.Displays;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class DependencyInjectionTests
    {
        private IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.Configure<TestDisplayOptions>(options => options.Ids = new List<string> { "race", "rally", "truck" });
            services.AddSingleton<IUdpClientFactory, UdpClientFactory>();
            services.AddSingleton<ISCSTelemetryFactory, SCSSdkTelemetryFactory>();
            services.AddSingleton<IDisplayFactory, DisplayFactory>();
            
            // Register IDisplayUpdateSender
            services.AddSingleton<IDisplayUpdateSender, HaddySimHub.Services.DisplayUpdateSender>();

            // Register IGameDataProvider implementations
            services.AddSingleton<IGameDataProvider<Displays.Dirt2.Packet>, Displays.Dirt2.Dirt2GameDataProvider>();
            services.AddSingleton<IGameDataProvider<iRacingSDK.IDataSample>, Displays.IRacing.IRacingGameDataProvider>();
            services.AddSingleton<IGameDataProvider<SCSSdkClient.Object.SCSTelemetry>, Displays.ETS.EtsGameDataProvider>();

            // Register IDataConverter implementations
            services.AddSingleton<IDataConverter<Displays.Dirt2.Packet, DisplayUpdate>, Displays.Dirt2.Dirt2DataConverter>();
            services.AddSingleton<IDataConverter<iRacingSDK.IDataSample, DisplayUpdate>, Displays.IRacing.IRacingDataConverter>();
            services.AddSingleton<IDataConverter<SCSSdkClient.Object.SCSTelemetry, DisplayUpdate>, Displays.ETS.EtsDataConverter>();
            services.AddSingleton<IDataConverter<DisplayUpdate, DisplayUpdate>, Services.IdentityDataConverter<DisplayUpdate>>(); // Added for TestDisplays
            services.AddSingleton<DisplaysRunner>();
            services.AddSingleton<Displays.Dirt2.Display>();
            services.AddSingleton<IDisplay>(sp => sp.GetRequiredService<Displays.Dirt2.Display>());
            services.AddSingleton<Displays.IRacing.Display>();
            services.AddSingleton<IDisplay>(sp => sp.GetRequiredService<Displays.IRacing.Display>());
            services.AddSingleton<Displays.ETS.Display>();
            services.AddSingleton<IDisplay>(sp => sp.GetRequiredService<Displays.ETS.Display>());
            services.AddSingleton<IDisplay>(sp => sp.GetRequiredService<IDisplayFactory>().Create("Dirt2.TestDisplay"));
            services.AddSingleton<IDisplay>(sp => sp.GetRequiredService<IDisplayFactory>().Create("IRacing.TestDisplay"));
            services.AddSingleton<IDisplay>(sp => sp.GetRequiredService<IDisplayFactory>().Create("ETS.TestDisplay"));
            return services;
        }

        [TestMethod]
        public void ServiceProvider_CanResolveUdpClientFactory()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IUdpClientFactory>();
            Assert.IsNotNull(factory);
            Assert.IsInstanceOfType(factory, typeof(UdpClientFactory));
        }

        [TestMethod]
        public void ServiceProvider_CanResolveSCSTelemetryFactory()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<ISCSTelemetryFactory>();
            Assert.IsNotNull(factory);
            Assert.IsInstanceOfType(factory, typeof(SCSSdkTelemetryFactory));
        }

        [TestMethod]
        public void ServiceProvider_CanResolveDisplayFactory()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            Assert.IsNotNull(factory);
            Assert.IsInstanceOfType(factory, typeof(DisplayFactory));
        }

        [TestMethod]
        public void ServiceProvider_CanResolveDirt2Display()
        {
            var provider = CreateServices().BuildServiceProvider();
            var display = provider.GetRequiredService<Displays.Dirt2.Display>();
            Assert.IsNotNull(display);
            Assert.IsInstanceOfType(display, typeof(Displays.Dirt2.Display));
        }

        [TestMethod]
        public void ServiceProvider_CanResolveIracingDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var display = provider.GetRequiredService<Displays.IRacing.Display>();
            Assert.IsNotNull(display);
            Assert.IsInstanceOfType(display, typeof(Displays.IRacing.Display));
        }

        [TestMethod]
        public void ServiceProvider_CanResolveETSDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var display = provider.GetRequiredService<Displays.ETS.Display>();
            Assert.IsNotNull(display);
            Assert.IsInstanceOfType(display, typeof(Displays.ETS.Display));
        }

        [TestMethod]
        public void ServiceProvider_CanResolveDisplaysRunner()
        {
            var provider = CreateServices().BuildServiceProvider();
            var runner = provider.GetRequiredService<DisplaysRunner>();
            Assert.IsNotNull(runner);
            Assert.IsInstanceOfType(runner, typeof(DisplaysRunner));
        }

        [TestMethod]
        public void ServiceProvider_CanResolveAllIDisplayInstances()
        {
            var provider = CreateServices().BuildServiceProvider();
            var displays = provider.GetRequiredService<IEnumerable<IDisplay>>();
            Assert.AreNotEqual(0, displays.Count());
            Assert.AreEqual(6, displays.Count());
        }

        [TestMethod]
        public void DisplayFactory_CanCreateDirt2Display()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("Dirt2.Display");
            Assert.IsNotNull(display);
            Assert.IsInstanceOfType(display, typeof(Displays.Dirt2.Display));
        }

        [TestMethod]
        public void DisplayFactory_CanCreateIracingDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("IRacing.Display");
            Assert.IsNotNull(display);
            Assert.IsInstanceOfType(display, typeof(Displays.IRacing.Display));
        }

        [TestMethod]
        public void DisplayFactory_CanCreateETSDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("ETS.Display");
            Assert.IsNotNull(display);
            Assert.IsInstanceOfType(display, typeof(Displays.ETS.Display));
        }

        [TestMethod]
        public void DisplayFactory_CanCreateDirt2TestDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("Dirt2.TestDisplay");
            Assert.IsNotNull(display);
            Assert.IsInstanceOfType(display, typeof(Displays.Dirt2.TestDisplay));
        }

        [TestMethod]
        public void DisplayFactory_CanCreateIracingTestDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("IRacing.TestDisplay");
            Assert.IsNotNull(display);
            Assert.IsInstanceOfType(display, typeof(Displays.IRacing.TestDisplay));
        }

        [TestMethod]
        public void DisplayFactory_CanCreateETSTestDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("ETS.TestDisplay");
            Assert.IsNotNull(display);
            Assert.IsInstanceOfType(display, typeof(Displays.ETS.TestDisplay));
        }

        [TestMethod]
        public void DisplayFactory_ThrowsOnUnknownDisplayType()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            
            var ex = Assert.Throws<InvalidOperationException>(() => factory.Create("Unknown.Display"));
            StringAssert.Contains(ex.Message, "Unknown display type");
        }

        [TestMethod]
        public void DisplaysRunner_Singleton_ReturnsSameInstance()
        {
            var provider = CreateServices().BuildServiceProvider();
            var runner1 = provider.GetRequiredService<DisplaysRunner>();
            var runner2 = provider.GetRequiredService<DisplaysRunner>();
            Assert.AreSame(runner1, runner2);
        }

        [TestMethod]
        public void Displays_Singleton_ReturnsSameInstance()
        {
            var provider = CreateServices().BuildServiceProvider();
            var display1 = provider.GetRequiredService<Displays.Dirt2.Display>();
            var display2 = provider.GetRequiredService<Displays.Dirt2.Display>();
            Assert.AreSame(display1, display2);
        }

        [TestMethod]
        public void DisplayFactory_Singleton_ReturnsSameInstance()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory1 = provider.GetRequiredService<IDisplayFactory>();
            var factory2 = provider.GetRequiredService<IDisplayFactory>();
            Assert.AreSame(factory1, factory2);
        }

        [TestMethod]
        public void Factories_Singleton_ReturnsSameInstance()
        {
            var provider = CreateServices().BuildServiceProvider();
            var udpFactory1 = provider.GetRequiredService<IUdpClientFactory>();
            var udpFactory2 = provider.GetRequiredService<IUdpClientFactory>();
            var scsFactory1 = provider.GetRequiredService<ISCSTelemetryFactory>();
            var scsFactory2 = provider.GetRequiredService<ISCSTelemetryFactory>();
            Assert.AreSame(udpFactory1, udpFactory2);
            Assert.AreSame(scsFactory1, scsFactory2);
        }
    }
}

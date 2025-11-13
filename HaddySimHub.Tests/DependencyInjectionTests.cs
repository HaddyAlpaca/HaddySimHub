using Microsoft.Extensions.DependencyInjection;
using HaddySimHub;
using HaddySimHub.Displays;
using Xunit;

namespace HaddySimHub.Tests
{
    public class DependencyInjectionTests
    {
        private IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.Configure<TestDisplayOptions>(options => options.Ids = new List<string> { "race", "rally", "truck" });
            services.AddSingleton<IUdpClientFactory, UdpClientFactory>();
            services.AddSingleton<ISCSTelemetryFactory, SCSSdkTelemetryFactory>();
            services.AddSingleton<IDisplayFactory, DisplayFactory>();
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

        [Fact]
        public void ServiceProvider_CanResolveUdpClientFactory()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IUdpClientFactory>();
            Assert.NotNull(factory);
            Assert.IsType<UdpClientFactory>(factory);
        }

        [Fact]
        public void ServiceProvider_CanResolveSCSTelemetryFactory()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<ISCSTelemetryFactory>();
            Assert.NotNull(factory);
            Assert.IsType<SCSSdkTelemetryFactory>(factory);
        }

        [Fact]
        public void ServiceProvider_CanResolveDisplayFactory()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            Assert.NotNull(factory);
            Assert.IsType<DisplayFactory>(factory);
        }

        [Fact]
        public void ServiceProvider_CanResolveDirt2Display()
        {
            var provider = CreateServices().BuildServiceProvider();
            var display = provider.GetRequiredService<Displays.Dirt2.Display>();
            Assert.NotNull(display);
            Assert.IsType<Displays.Dirt2.Display>(display);
        }

        [Fact]
        public void ServiceProvider_CanResolveIracingDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var display = provider.GetRequiredService<Displays.IRacing.Display>();
            Assert.NotNull(display);
            Assert.IsType<Displays.IRacing.Display>(display);
        }

        [Fact]
        public void ServiceProvider_CanResolveETSDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var display = provider.GetRequiredService<Displays.ETS.Display>();
            Assert.NotNull(display);
            Assert.IsType<Displays.ETS.Display>(display);
        }

        [Fact]
        public void ServiceProvider_CanResolveDisplaysRunner()
        {
            var provider = CreateServices().BuildServiceProvider();
            var runner = provider.GetRequiredService<DisplaysRunner>();
            Assert.NotNull(runner);
            Assert.IsType<DisplaysRunner>(runner);
        }

        [Fact]
        public void ServiceProvider_CanResolveAllIDisplayInstances()
        {
            var provider = CreateServices().BuildServiceProvider();
            var displays = provider.GetRequiredService<IEnumerable<IDisplay>>();
            Assert.NotEmpty(displays);
            Assert.Equal(6, displays.Count());
        }

        [Fact]
        public void DisplayFactory_CanCreateDirt2Display()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("Dirt2.Display");
            Assert.NotNull(display);
            Assert.IsType<Displays.Dirt2.Display>(display);
        }

        [Fact]
        public void DisplayFactory_CanCreateIracingDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("IRacing.Display");
            Assert.NotNull(display);
            Assert.IsType<Displays.IRacing.Display>(display);
        }

        [Fact]
        public void DisplayFactory_CanCreateETSDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("ETS.Display");
            Assert.NotNull(display);
            Assert.IsType<Displays.ETS.Display>(display);
        }

        [Fact]
        public void DisplayFactory_CanCreateDirt2TestDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("Dirt2.TestDisplay");
            Assert.NotNull(display);
            Assert.IsType<Displays.Dirt2.TestDisplay>(display);
        }

        [Fact]
        public void DisplayFactory_CanCreateIracingTestDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("IRacing.TestDisplay");
            Assert.NotNull(display);
            Assert.IsType<Displays.IRacing.TestDisplay>(display);
        }

        [Fact]
        public void DisplayFactory_CanCreateETSTestDisplay()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var display = factory.Create("ETS.TestDisplay");
            Assert.NotNull(display);
            Assert.IsType<Displays.ETS.TestDisplay>(display);
        }

        [Fact]
        public void DisplayFactory_ThrowsOnUnknownDisplayType()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory = provider.GetRequiredService<IDisplayFactory>();
            var ex = Assert.Throws<InvalidOperationException>(() => factory.Create("Unknown.Display"));
            Assert.Contains("Unknown display type", ex.Message);
        }

        [Fact]
        public void DisplaysRunner_Singleton_ReturnsSameInstance()
        {
            var provider = CreateServices().BuildServiceProvider();
            var runner1 = provider.GetRequiredService<DisplaysRunner>();
            var runner2 = provider.GetRequiredService<DisplaysRunner>();
            Assert.Same(runner1, runner2);
        }

        [Fact]
        public void Displays_Singleton_ReturnsSameInstance()
        {
            var provider = CreateServices().BuildServiceProvider();
            var display1 = provider.GetRequiredService<Displays.Dirt2.Display>();
            var display2 = provider.GetRequiredService<Displays.Dirt2.Display>();
            Assert.Same(display1, display2);
        }

        [Fact]
        public void DisplayFactory_Singleton_ReturnsSameInstance()
        {
            var provider = CreateServices().BuildServiceProvider();
            var factory1 = provider.GetRequiredService<IDisplayFactory>();
            var factory2 = provider.GetRequiredService<IDisplayFactory>();
            Assert.Same(factory1, factory2);
        }

        [Fact]
        public void Factories_Singleton_ReturnsSameInstance()
        {
            var provider = CreateServices().BuildServiceProvider();
            var udpFactory1 = provider.GetRequiredService<IUdpClientFactory>();
            var udpFactory2 = provider.GetRequiredService<IUdpClientFactory>();
            var scsFactory1 = provider.GetRequiredService<ISCSTelemetryFactory>();
            var scsFactory2 = provider.GetRequiredService<ISCSTelemetryFactory>();
            Assert.Same(udpFactory1, udpFactory2);
            Assert.Same(scsFactory1, scsFactory2);
        }
    }
}

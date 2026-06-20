using HaddySimHub;
using HaddySimHub.Dashboard;
using HaddySimHub.Displays;
using HaddySimHub.Tests.Mocks;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class ConsoleDashboardTests
    {
        private sealed class FakeDisplay(string description) : IDisplay
        {
            public string Description { get; } = description;
            public bool IsActive => false;
            public DateTime? LastUpdateUtc => null;
            public void Start() { }
            public void Stop() { }
        }

        private sealed class ConfigurableDisplay(string description, bool isActive, DateTime? lastUpdateUtc) : IDisplay
        {
            public string Description { get; } = description;
            public bool IsActive { get; } = isActive;
            public DateTime? LastUpdateUtc { get; } = lastUpdateUtc;
            public void Start() { }
            public void Stop() { }
        }

        private static string Render(IDisplay display)
        {
            var displays = new List<IDisplay> { display };
            var runner = new DisplaysRunner(displays, new MockDisplayUpdateSender());
            var dashboard = new ConsoleDashboard(displays, runner, new DashboardLogStore(), 3333);

            var writer = new StringWriter();
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Out = new AnsiConsoleOutput(writer),
            });
            console.Profile.Width = 120;
            console.Profile.Height = 40;
            console.Write(dashboard.BuildLayout(40));
            return writer.ToString();
        }

        [TestMethod]
        public void GamesPanel_NotRunning_ShowsNeitherWaitingNorLive()
        {
            var output = Render(new ConfigurableDisplay("Assetto Corsa", isActive: false, lastUpdateUtc: null));

            StringAssert.Contains(output, "Assetto Corsa");
            Assert.IsFalse(output.Contains("waiting for data"), "Inactive game must not show waiting state.");
            Assert.IsFalse(output.Contains("live"), "Inactive game must not show live state.");
        }

        [TestMethod]
        public void GamesPanel_RunningWithoutData_ShowsWaitingForData()
        {
            var output = Render(new ConfigurableDisplay("Assetto Corsa", isActive: true, lastUpdateUtc: null));

            StringAssert.Contains(output, "waiting for data");
        }

        [TestMethod]
        public void GamesPanel_RunningWithFreshData_ShowsLive()
        {
            var output = Render(new ConfigurableDisplay("Assetto Corsa", isActive: true, lastUpdateUtc: DateTime.UtcNow));

            StringAssert.Contains(output, "live");
        }

        [TestMethod]
        public void GamesPanel_RunningWithStaleData_ShowsNoData()
        {
            var output = Render(new ConfigurableDisplay("Assetto Corsa", isActive: true, lastUpdateUtc: DateTime.UtcNow.AddSeconds(-30)));

            StringAssert.Contains(output, "no data for");
        }

        private static ConsoleDashboard CreateDashboard(int gameCount)
        {
            var displays = Enumerable.Range(0, gameCount)
                .Select(i => (IDisplay)new FakeDisplay($"Game {i}"))
                .ToList();
            var runner = new DisplaysRunner(displays, new MockDisplayUpdateSender());
            return new ConsoleDashboard(displays, runner, new DashboardLogStore(), 3333);
        }

        private static int RenderedHeight(IRenderable renderable, int width, int height)
        {
            var writer = new StringWriter();
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Out = new AnsiConsoleOutput(writer),
            });
            console.Profile.Width = width;
            console.Profile.Height = height;
            console.Write(renderable);

            return writer.ToString()
                .Split('\n')
                .Count(line => line.Length > 0);
        }

        [TestMethod]
        [DataRow(24)]
        [DataRow(30)]
        [DataRow(50)]
        public void BuildLayout_FillsCompleteVerticalSpace(int windowHeight)
        {
            var dashboard = CreateDashboard(gameCount: 7);

            var rendered = dashboard.BuildLayout(windowHeight);

            var lines = RenderedHeight(rendered, width: 120, height: windowHeight);
            Assert.AreEqual(
                windowHeight,
                lines,
                $"Dashboard should fill all {windowHeight} terminal rows, but rendered {lines}.");
        }

        [TestMethod]
        public void BuildLayout_WithManyGames_StillFillsVerticalSpace()
        {
            const int windowHeight = 40;
            var dashboard = CreateDashboard(gameCount: 20);

            var rendered = dashboard.BuildLayout(windowHeight);

            var lines = RenderedHeight(rendered, width: 120, height: windowHeight);
            Assert.AreEqual(windowHeight, lines);
        }
    }
}

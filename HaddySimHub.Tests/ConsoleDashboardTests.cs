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

        private static string Render(IDisplay display) => Render([display]);

        private static string Render(IReadOnlyList<IDisplay> displays)
        {
            var runner = new DisplaysRunner(displays, new MockDisplayUpdateSender());
            var dashboard = new ConsoleDashboard(displays, runner, new DashboardLogStore(), 3333);

            using var cancellation = new CancellationTokenSource();
            var running = runner.RunAsync(cancellation.Token);

            try
            {
                // The games panel distinguishes the game feeding the dashboard from
                // one that is only running, so render while a display is selected.
                WaitForSelection(runner, displays);

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
            finally
            {
                cancellation.Cancel();
                try
                {
                    running.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private static void WaitForSelection(DisplaysRunner runner, IReadOnlyList<IDisplay> displays)
        {
            if (!displays.Any(d => d.IsActive))
            {
                return;
            }

            var deadline = DateTime.UtcNow.AddSeconds(3);
            while (runner.CurrentDisplay is null && DateTime.UtcNow < deadline)
            {
                Thread.Sleep(10);
            }

            Assert.IsNotNull(runner.CurrentDisplay, "The runner did not select a display in time.");
        }

        [TestMethod]
        public void GamesPanel_RunningButNotSelected_ShowsStandingBy()
        {
            // Only one game feeds the dashboard, so a second running game is idle by
            // design and must not be reported as a game that fails to deliver data.
            var output = Render(
            [
                new ConfigurableDisplay("Assetto Corsa", isActive: true, lastUpdateUtc: DateTime.UtcNow),
                new ConfigurableDisplay("Euro Truck Simulator 2", isActive: true, lastUpdateUtc: null),
            ]);

            StringAssert.Contains(output, "standing by");
            Assert.IsFalse(
                output.Contains("waiting for data"),
                "A running game that is not the selected one must not show the waiting state.");
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

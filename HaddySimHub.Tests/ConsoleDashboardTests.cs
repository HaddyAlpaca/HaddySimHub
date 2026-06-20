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
            public void Start() { }
            public void Stop() { }
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

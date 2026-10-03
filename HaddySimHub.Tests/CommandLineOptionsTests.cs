using HaddySimHub.Infrastructure;

namespace HaddySimHub.Tests;

[TestClass]
public class CommandLineOptionsTests
{
    [TestMethod]
    public void NoArgumentsMeansNothingIsEnabled()
    {
        var parsed = Parse();

        Assert.IsFalse(parsed.E2eMode);
        Assert.IsFalse(parsed.NoUpdate);
        Assert.IsFalse(parsed.HelpRequested);
        Assert.IsNull(parsed.CaptureDirectory);
    }

    [TestMethod]
    public void ParsesEveryOption()
    {
        var parsed = Parse("--e2e", "--no-update", "--capture", "/tmp/frames");

        Assert.IsTrue(parsed.E2eMode);
        Assert.IsTrue(parsed.NoUpdate);
        Assert.AreEqual("/tmp/frames", parsed.CaptureDirectory);
    }

    [TestMethod]
    public void AcceptsBothHelpSpellings()
    {
        Assert.IsTrue(Parse("--help").HelpRequested);
        Assert.IsTrue(Parse("-h").HelpRequested);
    }

    [TestMethod]
    public void TheUsageTextNamesEveryOptionItAccepts()
    {
        // A documented option that stops working is the failure this catches.
        foreach (var option in new[] { "--e2e", "--no-update", "--capture", "--help", "-h" })
        {
            StringAssert.Contains(CommandLineOptions.Usage, option);
        }
    }

    [TestMethod]
    public void RejectsAnUnknownOptionInsteadOfSilentlyStartingTheServer()
    {
        var ok = CommandLineOptions.TryParse(["--e2ee"], out _, out var error);

        Assert.IsFalse(ok);
        StringAssert.Contains(error, "--e2ee");
    }

    [TestMethod]
    public void RejectsCaptureWithoutADirectoryInsteadOfThrowing()
    {
        // This used to surface as an unhandled ArgumentException with a stack
        // trace, after the update check had already run.
        var ok = CommandLineOptions.TryParse(["--capture"], out _, out var error);

        Assert.IsFalse(ok);
        StringAssert.Contains(error, "directory");
    }

    private static CommandLineOptions Parse(params string[] args)
    {
        Assert.IsTrue(
            CommandLineOptions.TryParse(args, out var options, out var error),
            $"Expected {string.Join(' ', args)} to parse, but it failed with: {error}");

        return options;
    }
}

using HaddySimHub.Capture;
using HaddySimHub.Displays.AC;

namespace HaddySimHub.Tests;

/// <summary>
/// Covers the layout manifest generator, and checks the committed manifests against
/// what reflection now reports.
/// </summary>
/// <remarks>
/// The manifest is the layout gate for the Rust migration, so the generator has to
/// fail loudly rather than quietly omit a field it cannot measure.
/// </remarks>
[TestClass]
public class LayoutManifestTests
{
    /// <summary>
    /// Cross-check only: <c>ACDataConverterTests</c> already pins this size, so this
    /// asserts that the generator reports the size the rest of the suite believes
    /// rather than repeating the layout assertions.
    /// </summary>
    [TestMethod]
    public void Describe_ReportsTheStructSizeTheRuntimeMarshals()
    {
        var layout = LayoutManifest.Describe(typeof(ACPhysics));

        Assert.AreEqual(580, layout.Size);
    }

    [TestMethod]
    public void Describe_ReportsTheLayoutAttributesThatProduceTheOffsets()
    {
        var msfs = LayoutManifest.Describe(typeof(Displays.Msfs.MsfsTelemetry));
        var acStatic = LayoutManifest.Describe(typeof(ACStatic));

        Assert.AreEqual("Sequential", msfs.LayoutKind);
        Assert.AreEqual(1, msfs.Pack);
        Assert.AreEqual("Ansi", msfs.CharSet);

        Assert.AreEqual(4, acStatic.Pack);
        Assert.AreEqual("Unicode", acStatic.CharSet);
    }

    /// <summary>
    /// A fixed-width string occupies <c>SizeConst</c> characters, so a Unicode struct
    /// spends two bytes per character. Nothing else in the suite covers that, and
    /// getting it wrong silently halves or doubles every string offset after it.
    /// </summary>
    [TestMethod]
    public void Describe_SizesByValTStrFieldsByTheStructsCharSet()
    {
        var fields = LayoutManifest.Describe(typeof(ACStatic)).Fields.ToDictionary(f => f.Name);

        Assert.AreEqual(15 * 2, fields["SmVersion"].Size);
        Assert.AreEqual(33 * 2, fields["CarModel"].Size);

        // ByValTStr with SizeConst 34 is 33 characters plus one of padding.
        Assert.AreEqual(34 * 2, fields["PlayerNick"].Size);
    }

    /// <summary>
    /// Fixed-size arrays are measured as a gap to the next field, so the element
    /// count has to be reflected in the recorded size.
    /// </summary>
    [TestMethod]
    public void Describe_SizesFixedSizeArraysFromTheGapToTheNextField()
    {
        var fields = LayoutManifest.Describe(typeof(ACPhysics)).Fields.ToDictionary(f => f.Name);

        Assert.AreEqual("Single[]", fields["Velocity"].Type);
        Assert.AreEqual(3 * sizeof(float), fields["Velocity"].Size);
        Assert.AreEqual(4 * sizeof(float), fields["WheelSlip"].Size);
    }

    [TestMethod]
    public void Describe_RejectsReferenceTypes()
    {
        Assert.Throws<ArgumentException>(() => LayoutManifest.Describe(typeof(string)));
    }

    /// <summary>
    /// A struct the reader assembles field by field is not a memory image. Describing
    /// it would produce a layout that matches nothing, so it is rejected.
    /// </summary>
    [TestMethod]
    public void Describe_RejectsStructsThatHoldReferences()
    {
        Assert.Throws<ArgumentException>(() => LayoutManifest.Describe(typeof(ACTelemetry)));
    }

    [TestMethod]
    public void Fields_AreOrderedAndCoverTheWholeStruct()
    {
        foreach (var entry in TelemetryLayoutRegistry.All)
        {
            foreach (var type in entry.Types)
            {
                var layout = LayoutManifest.Describe(type);
                var covered = 0;

                foreach (var field in layout.Fields)
                {
                    Assert.AreEqual(covered, field.Offset, $"{layout.Type}.{field.Name} is not contiguous.");
                    Assert.IsTrue(field.Size > 0, $"{layout.Type}.{field.Name} occupies no bytes.");
                    covered += field.Size;
                }

                Assert.AreEqual(layout.Size, covered, $"{layout.Type} fields do not cover the struct.");
            }
        }
    }

    [TestMethod]
    public void Registry_OnlyContainsTypesThatCanBeDescribed()
    {
        foreach (var entry in TelemetryLayoutRegistry.All)
        {
            Assert.IsTrue(entry.Types.Length > 0, $"'{entry.Slug}' has no types.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Description), $"'{entry.Slug}' has no description.");

            foreach (var type in entry.Types)
            {
                Assert.IsTrue(type.IsValueType, $"'{entry.Slug}' lists non-struct type '{type.Name}'.");
            }
        }
    }

    /// <summary>
    /// Regenerate the committed manifests with <c>HADDYSIMHUB_UPDATE_MANIFESTS=1</c>.
    /// A failure here means a struct changed, which is the gate the migration needs.
    /// </summary>
    [TestMethod]
    public void CommittedManifests_MatchTheGeneratedLayout()
    {
        var directory = Path.Combine(RepositoryRoot(), "fixtures", "telemetry", "manifest");
        var update = Environment.GetEnvironmentVariable("HADDYSIMHUB_UPDATE_MANIFESTS") == "1";

        foreach (var entry in TelemetryLayoutRegistry.All)
        {
            var generated = LayoutManifest.Serialize(LayoutManifest.Describe(entry.Slug, entry.Types));
            var path = Path.Combine(directory, $"{entry.Slug}.json");

            if (update)
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(path, generated);
                continue;
            }

            Assert.IsTrue(File.Exists(path), $"Missing manifest '{path}'. Run the tests with HADDYSIMHUB_UPDATE_MANIFESTS=1 to generate it.");
            Assert.AreEqual(File.ReadAllText(path), generated, $"The layout of '{entry.Slug}' changed. Re-record the telemetry corpus for that game as well.");
        }
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "HaddySimHub.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not find HaddySimHub.sln above the test output directory.");
    }
}

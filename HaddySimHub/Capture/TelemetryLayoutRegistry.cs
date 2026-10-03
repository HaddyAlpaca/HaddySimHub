using HaddySimHub.Displays;
using HaddySimHub.Displays.AC;
using HaddySimHub.Displays.ACC;
using HaddySimHub.Displays.ACRally;
using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Displays.Forza;
using HaddySimHub.Displays.Msfs;

namespace HaddySimHub.Capture;

/// <summary>The structs one game publishes, as a layout manifest entry.</summary>
/// <param name="Slug">Short name used for the manifest file name and its <c>game</c> field.</param>
/// <param name="Description">Human-readable game name.</param>
/// <param name="Types">Structs read from memory or off the wire, in declaration order.</param>
public sealed record TelemetryLayoutEntry(string Slug, string Description, Type[] Types);

/// <summary>
/// The explicit list of structs a layout manifest covers.
/// </summary>
/// <remarks>
/// <para>
/// This lists the structs the runtime marshals from a memory image, not the
/// converter inputs. For most of the Assetto Corsa titles the reader assembles a
/// flat converter input field by field, so the converter input has no layout to
/// describe while the page structs do.
/// </para>
/// <para>
/// ETS2 and iRacing are absent on purpose. Both are delivered by a C# SDK that
/// hands over an object graph through a callback, so this repository owns no
/// struct to describe; their wire format belongs to the SDK and is being
/// replaced rather than ported. See <c>docs/telemetry-corpus.md</c>.
/// </para>
/// <para>
/// Entries are listed explicitly rather than discovered by reflection, per
/// ADR-0003.
/// </para>
/// </remarks>
public static class TelemetryLayoutRegistry
{
    public static IReadOnlyList<TelemetryLayoutEntry> All { get; } =
    [
        new TelemetryLayoutEntry(DisplayDefinitions.Game.Ac.Slug, DisplayDefinitions.Game.Ac.Description,
            [typeof(ACPhysics), typeof(ACGraphics), typeof(ACStatic)]),

        new TelemetryLayoutEntry(DisplayDefinitions.Game.Acc.Slug, DisplayDefinitions.Game.Acc.Description,
            [typeof(ACCPhysics), typeof(ACCGraphics)]),

        new TelemetryLayoutEntry(DisplayDefinitions.Game.AcRally.Slug, DisplayDefinitions.Game.AcRally.Description,
            [typeof(ACRallyPhysics), typeof(ACRallyGraphics), typeof(ACRallyStatic)]),

        new TelemetryLayoutEntry(DisplayDefinitions.Game.Dirt2.Slug, DisplayDefinitions.Game.Dirt2.Description,
            [typeof(Packet)]),

        new TelemetryLayoutEntry(DisplayDefinitions.Game.Forza.Slug, DisplayDefinitions.Game.Forza.Description,
            [typeof(ForzaTelemetry)]),

        new TelemetryLayoutEntry(DisplayDefinitions.Game.Msfs.Slug, DisplayDefinitions.Game.Msfs.Description,
            [typeof(MsfsTelemetry)]),
    ];
}

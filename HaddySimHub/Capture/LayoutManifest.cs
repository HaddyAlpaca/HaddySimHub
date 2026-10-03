using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaddySimHub.Capture;

/// <summary>One field's position in a marshalled struct.</summary>
public sealed record ManifestField
{
    public required string Name { get; init; }

    /// <summary>CLR type name without a namespace, for example <c>Single</c> or <c>Single[]</c>.</summary>
    public required string Type { get; init; }

    public required int Offset { get; init; }

    public required int Size { get; init; }
}

/// <summary>
/// Field layout of one struct read from memory or off the wire, together with the
/// layout attributes that produced it.
/// </summary>
public sealed record TypeLayout
{
    public required string Type { get; init; }

    public required string LayoutKind { get; init; }

    /// <summary>Zero means the platform default, which is eight on 64-bit Windows.</summary>
    public required int Pack { get; init; }

    public required string CharSet { get; init; }

    /// <summary>Total marshalled size in bytes.</summary>
    public required int Size { get; init; }

    public required IReadOnlyList<ManifestField> Fields { get; init; }
}

/// <summary>Every struct one game reads, for one game.</summary>
public sealed record GameLayout
{
    public required string Game { get; init; }

    public required IReadOnlyList<TypeLayout> Types { get; init; }
}

/// <summary>
/// Describes the memory layout of the structs a game publishes, so the Rust port
/// can be checked against them.
/// </summary>
/// <remarks>
/// <para>
/// Offsets and sizes are recorded rather than derived, because <c>Pack</c> and
/// <c>CharSet</c> change both. A field's size is taken as the gap to the next
/// field, so fixed-size arrays and fixed-width strings need no special handling
/// and the sizes always add up to the struct total.
/// </para>
/// <para>
/// This is only meaningful for structs the runtime marshals from a memory image.
/// A type the reader assembles field by field has no layout to describe, and
/// passing one here is an error rather than a silent omission.
/// </para>
/// </remarks>
public static class LayoutManifest
{
    // The manifests are committed and compared byte for byte, so the line ending
    // is fixed rather than taken from the platform.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        NewLine = "\n",
    };

    /// <summary>Describes every type in <paramref name="types"/> as one game.</summary>
    public static GameLayout Describe(string game, params Type[] types)
    {
        ArgumentException.ThrowIfNullOrEmpty(game);
        ArgumentNullException.ThrowIfNull(types);

        return new GameLayout
        {
            Game = game,
            Types = Array.ConvertAll(types, Describe),
        };
    }

    /// <summary>Describes a single struct.</summary>
    /// <exception cref="ArgumentException">
    /// The type is not read as a memory image, because it is a reference type or
    /// holds something the runtime cannot marshal inline.
    /// </exception>
    public static TypeLayout Describe(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!type.IsValueType)
        {
            throw new ArgumentException(
                $"'{type.Name}' is a reference type, so it has no memory layout to describe. " +
                "Only structs the runtime marshals are valid manifest entries.",
                nameof(type));
        }

        EnsureMemoryImage(type);

        var layout = type.StructLayoutAttribute;
        var total = Marshal.SizeOf(type);
        var fields = ReadFields(type, total);

        return new TypeLayout
        {
            Type = type.Name,
            LayoutKind = (layout?.Value ?? LayoutKind.Sequential).ToString(),
            Pack = layout?.Pack ?? 0,
            CharSet = (layout?.CharSet ?? CharSet.Ansi).ToString(),
            Size = total,
            Fields = fields,
        };
    }

    public static string Serialize(GameLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        return JsonSerializer.Serialize(layout, JsonOptions) + "\n";
    }

    /// <summary>
    /// Rejects anything the runtime does not marshal inline.
    /// </summary>
    /// <remarks>
    /// <c>Marshal.SizeOf</c> happily reports a size for a struct holding a
    /// <see cref="string"/> or an array, treating each as a pointer. Describing such
    /// a type would produce a layout that matches nothing, and the converters of the
    /// Assetto Corsa titles all take an input the reader assembles field by field
    /// rather than one the runtime reads. Fixed-width strings and fixed-size arrays
    /// are inline, so those are allowed; anything else is not.
    /// </remarks>
    private static void EnsureMemoryImage(Type type)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var fieldType = field.FieldType;
            var marshalAs = field.GetCustomAttribute<MarshalAsAttribute>();

            if (fieldType.IsPrimitive || fieldType.IsEnum)
            {
                continue;
            }

            if (fieldType.IsArray)
            {
                if (marshalAs?.Value != UnmanagedType.ByValArray || marshalAs.SizeConst <= 0)
                {
                    throw NotAMemoryImage(type, field.Name, "an array is only inline when declared as ByValArray with a SizeConst");
                }

                EnsureMemoryImage(fieldType.GetElementType()!);
                continue;
            }

            if (fieldType.IsValueType)
            {
                EnsureMemoryImage(fieldType);
                continue;
            }

            var isFixedWidthString = fieldType == typeof(string)
                && marshalAs?.Value == UnmanagedType.ByValTStr
                && marshalAs.SizeConst > 0;

            if (!isFixedWidthString)
            {
                throw NotAMemoryImage(type, field.Name, $"'{fieldType.Name}' is a reference type");
            }
        }
    }

    private static ArgumentException NotAMemoryImage(Type type, string field, string reason) =>
        new($"'{type.Name}.{field}' is not read as a memory image because {reason}. " +
            "Only structs the runtime marshals are valid manifest entries; a struct the " +
            "reader assembles field by field has no layout to describe.", nameof(type));

    private static IReadOnlyList<ManifestField> ReadFields(Type type, int total)
    {
        var offsets = new List<(FieldInfo Field, int Offset)>();

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            int offset;
            try
            {
                offset = Marshal.OffsetOf(type, field.Name).ToInt32();
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or TypeLoadException)
            {
                throw new InvalidOperationException(
                    $"Cannot determine the offset of '{type.Name}.{field.Name}'. A field with a " +
                    "fixed size that the manifest cannot measure usually means the struct is not " +
                    "read as a memory image.",
                    ex);
            }

            if (offset < 0)
            {
                throw new InvalidOperationException(
                    $"'{type.Name}.{field.Name}' reports a negative offset ({offset}).");
            }

            offsets.Add((field, offset));
        }

        // Ordering by offset rather than by declaration order keeps the manifest
        // correct if reflection ever hands fields back in a different order, and
        // the gap to the next field is then always the field's real size.
        offsets.Sort(static (left, right) => left.Offset.CompareTo(right.Offset));

        var fields = new List<ManifestField>(offsets.Count);

        for (var i = 0; i < offsets.Count; i++)
        {
            var (field, offset) = offsets[i];
            var end = i + 1 < offsets.Count ? offsets[i + 1].Offset : total;
            var size = end - offset;

            if (size <= 0)
            {
                throw new InvalidOperationException(
                    $"'{type.Name}.{field.Name}' occupies {size} bytes at offset {offset}. " +
                    "Overlapping or zero-width fields mean this struct is not a flat memory image.");
            }

            fields.Add(new ManifestField
            {
                Name = field.Name,
                Type = field.FieldType.Name,
                Offset = offset,
                Size = size,
            });
        }

        return fields;
    }
}

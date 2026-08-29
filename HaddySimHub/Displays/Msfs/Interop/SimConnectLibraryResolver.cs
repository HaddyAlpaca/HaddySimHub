using System.Reflection;
using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.Msfs.Interop;

/// <summary>
/// Teaches the runtime where to find the native <c>SimConnect.dll</c>.
/// </summary>
/// <remarks>
/// The library ships with the MSFS SDK rather than with the simulator, and it is not
/// installed anywhere on the default DLL search path. Probing the known locations
/// here means a machine with the SDK installed works with no configuration, and a
/// machine without it degrades to "flight simulator display stays inactive" instead
/// of an unhandled <see cref="DllNotFoundException"/>.
/// </remarks>
internal static class SimConnectLibraryResolver
{
    private static readonly Lock Sync = new();
    private static bool _registered;

    /// <summary>
    /// Registers the resolver once for the given assembly. Calling
    /// <see cref="NativeLibrary.SetDllImportResolver"/> twice throws, so this is guarded.
    /// </summary>
    public static void Register(Assembly assembly)
    {
        lock (Sync)
        {
            if (_registered)
            {
                return;
            }

            NativeLibrary.SetDllImportResolver(assembly, Resolve);
            _registered = true;
        }
    }

    /// <summary>
    /// The locations searched, in order. The application directory comes first so a
    /// copy shipped alongside the executable always wins over an SDK install.
    /// </summary>
    public static IEnumerable<string> CandidatePaths()
    {
        yield return Path.Combine(AppContext.BaseDirectory, NativeMethods.LibraryName);

        var sdkRoot = Environment.GetEnvironmentVariable("MSFS_SDK");
        if (!string.IsNullOrWhiteSpace(sdkRoot))
        {
            yield return Path.Combine(sdkRoot, "SimConnect SDK", "lib", NativeMethods.LibraryName);
        }

        yield return Path.Combine(@"C:\MSFS SDK", "SimConnect SDK", "lib", NativeMethods.LibraryName);
    }

    private static nint Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, NativeMethods.LibraryName, StringComparison.OrdinalIgnoreCase))
        {
            return nint.Zero;
        }

        foreach (var candidate in CandidatePaths())
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out var handle))
            {
                Logger.Debug($"[Msfs] Loaded {NativeMethods.LibraryName} from {candidate}");
                return handle;
            }
        }

        // Zero hands the lookup back to the default probing logic, which still finds
        // the library if it happens to be on PATH.
        return nint.Zero;
    }
}

using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.Msfs.Interop;

/// <summary>
/// The five SimConnect entry points needed to read telemetry.
/// </summary>
/// <remarks>
/// <para>
/// These bind to the native x64 <c>SimConnect.dll</c> directly. The managed
/// <c>Microsoft.FlightSimulator.SimConnect.dll</c> that ships with the SDK is a
/// mixed-mode C++/CLI assembly built against .NET Framework and cannot be loaded by
/// this application, so the interop lives here instead.
/// </para>
/// <para>
/// The client polls with <c>SimConnect_GetNextDispatch</c> rather than passing a
/// window handle to <c>SimConnect_Open</c>. That keeps the whole integration inside
/// the existing timer-based provider pattern, with no Win32 message loop.
/// </para>
/// </remarks>
internal static class NativeMethods
{
    public const string LibraryName = "SimConnect.dll";

    /// <summary>The object id of the aircraft the user is flying.</summary>
    public const uint ObjectIdUser = 0;

    static NativeMethods()
    {
        SimConnectLibraryResolver.Register(typeof(NativeMethods).Assembly);
    }

    /// <summary>
    /// Opens a connection. <paramref name="windowHandle"/>, <paramref name="userEventWin32"/>
    /// and <paramref name="eventHandle"/> stay zero because dispatch is polled.
    /// </summary>
    [DllImport(LibraryName, CharSet = CharSet.Ansi, ExactSpelling = true)]
    public static extern int SimConnect_Open(
        out nint handle,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        nint windowHandle,
        uint userEventWin32,
        nint eventHandle,
        uint configIndex);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SimConnect_Close(nint handle);

    /// <summary>
    /// Appends one simulation variable to a data definition. The order of these calls
    /// is the byte order of the block the sim sends back.
    /// </summary>
    /// <param name="unitName">Null for the string data types, which are unitless.</param>
    [DllImport(LibraryName, CharSet = CharSet.Ansi, ExactSpelling = true)]
    public static extern int SimConnect_AddToDataDefinition(
        nint handle,
        uint defineId,
        [MarshalAs(UnmanagedType.LPStr)] string datumName,
        [MarshalAs(UnmanagedType.LPStr)] string? unitName,
        uint datumType,
        float epsilon,
        uint datumId);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SimConnect_RequestDataOnSimObject(
        nint handle,
        uint requestId,
        uint defineId,
        uint objectId,
        uint period,
        uint flags,
        uint origin,
        uint interval,
        uint limit);

    /// <summary>
    /// Takes the next message off the dispatch queue. Returns a failure HRESULT when
    /// the queue is empty, which is the normal idle case rather than an error.
    /// </summary>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SimConnect_GetNextDispatch(nint handle, out nint data, out uint size);

    /// <summary>SimConnect reports failure the COM way, with the sign bit set.</summary>
    public static bool Failed(int hresult) => hresult < 0;
}

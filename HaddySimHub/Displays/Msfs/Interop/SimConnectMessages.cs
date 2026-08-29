using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.Msfs.Interop;

/// <summary>
/// The subset of <c>SIMCONNECT_RECV_ID</c> this client acts on. Any other message
/// id is skipped: SimConnect multiplexes every kind of notification onto one queue,
/// and we only subscribe to simulation object data.
/// </summary>
internal enum SimConnectRecvId : uint
{
    Null = 0,
    Exception = 1,
    Open = 2,
    Quit = 3,
    SimObjectData = 8,
}

/// <summary>
/// <c>SIMCONNECT_DATATYPE</c>. The numbering is the SDK's, and the payload size of
/// each member is what lets <see cref="SimVarDefinitions"/> be checked against the
/// layout of <see cref="MsfsTelemetry"/>.
/// </summary>
internal enum SimConnectDataType : uint
{
    Invalid = 0,
    Int32 = 1,
    Int64 = 2,
    Float32 = 3,
    Float64 = 4,
    String8 = 5,
    String32 = 6,
    String64 = 7,
    String128 = 8,
    String256 = 9,
    String260 = 10,
    StringV = 11,
}

/// <summary>
/// <c>SIMCONNECT_PERIOD</c>: how often the sim should send a subscribed data block.
/// </summary>
internal enum SimConnectPeriod : uint
{
    Never = 0,
    Once = 1,
    VisualFrame = 2,
    SimFrame = 3,
    Second = 4,
}

/// <summary>
/// The <c>SIMCONNECT_RECV</c> header every message on the dispatch queue starts with.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SimConnectRecv
{
    public uint Size;
    public uint Version;
    public uint Id;
}

/// <summary>
/// <c>SIMCONNECT_RECV_SIMOBJECT_DATA</c>, without its trailing <c>dwData</c> payload.
/// </summary>
/// <remarks>
/// The payload begins immediately after these fields, so
/// <c>Marshal.SizeOf&lt;SimConnectRecvSimObjectData&gt;()</c> is the offset to read
/// the telemetry struct from. Deriving it that way rather than hard-coding 40 means
/// a field added here can never silently shift the payload.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SimConnectRecvSimObjectData
{
    public uint Size;
    public uint Version;
    public uint Id;
    public uint RequestId;
    public uint ObjectId;
    public uint DefineId;
    public uint Flags;
    public uint EntryNumber;
    public uint OutOf;
    public uint DefineCount;
}

/// <summary>
/// <c>SIMCONNECT_RECV_EXCEPTION</c>: the sim's way of reporting that a request we
/// sent was malformed, most often an unknown simvar name or a wrong unit.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SimConnectRecvException
{
    public uint Size;
    public uint Version;
    public uint Id;
    public uint ExceptionCode;
    public uint SendId;
    public uint Index;
}

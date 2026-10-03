using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Win32.Synchronization;

namespace iRacingSDK;

class iRacingMemory
{
    private const int SnapshotAttempts = 3;

    public MemoryMappedViewAccessor Accessor { get; private set; }
    IntPtr dataValidEvent;
    MemoryMappedFile irsdkMappedMemory;

    public bool IsConnected()
    {
        if (Accessor != null)
            return true;

        var dataValidEvent = Event.OpenEvent(Event.EVENT_ALL_ACCESS | Event.EVENT_MODIFY_STATE, false, "Local\\IRSDKDataValidEvent");
        if (dataValidEvent == IntPtr.Zero)
        {
            var lastError = Marshal.GetLastWin32Error();
            if (lastError == Event.ERROR_FILE_NOT_FOUND)
                return false;

            Trace.WriteLine(string.Format("Unable to open event Local\\IRSDKDataValidEvent - Error Code {0}", lastError), "DEBUG");
            return false;
        }

        MemoryMappedFile irsdkMappedMemory = null;
        try
        {
#pragma warning disable CA1416 // Validate platform compatibility
            irsdkMappedMemory = MemoryMappedFile.OpenExisting("Local\\IRSDKMemMapFileName");
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception e)
        {
            Trace.WriteLine("Error accessing shared memory", "DEBUG");
            Trace.WriteLine(e.Message, "DEBUG");
        }

        if (irsdkMappedMemory == null)
            return false;

        var accessor = irsdkMappedMemory.CreateViewAccessor();
        if (accessor == null)
        {
            irsdkMappedMemory.Dispose();
            Trace.WriteLine("Unable to Create View into shared memory", "DEBUG");
            return false;
        }

        this.irsdkMappedMemory = irsdkMappedMemory;
        this.dataValidEvent = dataValidEvent;
        Accessor = accessor;
        return true;
    }

    public bool WaitForData() => Event.WaitForSingleObject(dataValidEvent, 17) == 0;

    public byte[] ReadRawSnapshot()
    {
        var accessor = Accessor ?? throw new InvalidOperationException("iRacing shared memory is not connected.");
        var headerLength = Marshal.SizeOf<iRSDKHeader>();
        var headerBefore = new byte[headerLength];
        var headerAfter = new byte[headerLength];

        for (var attempt = 0; attempt < SnapshotAttempts; attempt++)
        {
            accessor.ReadArray(0, headerBefore, 0, headerLength);
            var snapshot = new byte[checked((int)accessor.Capacity)];
            accessor.ReadArray(0, snapshot, 0, snapshot.Length);
            accessor.ReadArray(0, headerAfter, 0, headerLength);
            if (IsStableSnapshotHeader(headerBefore, snapshot, headerAfter))
            {
                return snapshot;
            }
        }

        throw new InvalidDataException("iRacing shared-memory header changed while capturing a raw snapshot.");
    }

    internal static bool IsStableSnapshotHeader(byte[] before, byte[] snapshot, byte[] after) =>
        before.Length == after.Length &&
        before.Length <= snapshot.Length &&
        before.AsSpan().SequenceEqual(after) &&
        before.AsSpan().SequenceEqual(snapshot.AsSpan(0, before.Length));
}

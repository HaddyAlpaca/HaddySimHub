using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Win32.Synchronization;

namespace iRacingSDK;

class iRacingMemory
{
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
}

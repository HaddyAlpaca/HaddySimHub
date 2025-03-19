using System.Diagnostics;

namespace iRacingSDK;

public class iRacingMessaging
{
    protected readonly int messageId;
    DateTime lastMessagePostedTime = DateTime.Now;
    Task currentMessageTask;
    const double MessageThrottleTime = 1000;

    protected iRacingMessaging()
    {
        messageId = Win32.Messages.RegisterWindowMessage("IRSDK_BROADCASTMSG");
        currentMessageTask = new Task(() => { });
        currentMessageTask.Start();
    }

    protected virtual void SendMessage(BroadcastMessage message, short var1 = 0, int var2 = 0)
    {
        var msgVar1 = FromShorts((short)message, var1);

        var lastTask = currentMessageTask;
        currentMessageTask = new Task(() =>
        {
            lastTask.Wait();
            lastTask.Dispose();
            lastTask = null;

            var timeSinceLastMsg = DateTime.Now - lastMessagePostedTime;
            var throttleTime = (int)(MessageThrottleTime - timeSinceLastMsg.TotalMilliseconds);
            if (throttleTime > 0)
            {
                Trace.WriteLine(string.Format("Throttle message {0} delivery to iRacing by {1} millisecond", message, throttleTime), "DEBUG");
                Thread.Sleep(throttleTime);
            }
            lastMessagePostedTime = DateTime.Now;

            if (!Win32.Messages.SendNotifyMessage(Win32.Messages.HWND_BROADCAST, messageId, msgVar1, var2))
                throw new Exception(String.Format("Error in broadcasting message {0}", message));
        });

        currentMessageTask.Start();
    }

    protected void SendMessage(BroadcastMessage message, short var1, short var2, short var3)
    {
        var var23 = FromShorts(var2, var3);
        SendMessage(message, var1, var23);
    }
    
    protected static int FromShorts(short lowPart, short highPart)
    {
        return ((int)highPart << 16) | (ushort)lowPart;
    }

    public virtual void Wait()
    {
        currentMessageTask.Wait();
    }
}

namespace iRacingSDK;

public static class iRSDKHeaderExtensions
{
    public static bool HasChangedSinceReading(this iRSDKHeader header, VarBufWithIndex buf) =>
        header.varBuf[buf.index].tickCount != buf.tickCount;

    public static VarBufWithIndex FindLatestBuf(this iRSDKHeader header, int requestedTickCount)
    {
        VarBuf maxBuf = new();
        int maxIndex = -1;

        for (var i = 0; i < header.numBuf; i++)
        {
            var b = header.varBuf[i];

            if (b.tickCount == requestedTickCount)
                return new VarBufWithIndex() { tickCount = requestedTickCount, bufOffset = b.bufOffset, index = i };

            if (b.tickCount > maxBuf.tickCount)
            {
                maxBuf = b;
                maxIndex = i;
            }
        }

        return new VarBufWithIndex() { tickCount = maxBuf.tickCount, bufOffset = maxBuf.bufOffset, index = maxIndex };
    }
}

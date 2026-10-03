using System.Buffers.Binary;
using iRacingSDK;

namespace HaddySimHub.Tests;

[TestClass]
public class IRacingRawSnapshotTests
{
    [TestMethod]
    public void TryGetLatestTickCountReadsTheNewestRawRingBufferTick()
    {
        var snapshot = new byte[112];
        BinaryPrimitives.WriteInt32LittleEndian(snapshot.AsSpan(32, sizeof(int)), 3);
        BinaryPrimitives.WriteInt32LittleEndian(snapshot.AsSpan(48, sizeof(int)), 18);
        BinaryPrimitives.WriteInt32LittleEndian(snapshot.AsSpan(64, sizeof(int)), 20);
        BinaryPrimitives.WriteInt32LittleEndian(snapshot.AsSpan(80, sizeof(int)), 19);

        var valid = iRacingConnection.TryGetLatestTickCount(snapshot, out var tickCount);

        Assert.IsTrue(valid);
        Assert.AreEqual(20, tickCount);
    }

    [TestMethod]
    public void TryGetLatestTickCountRejectsInvalidBufferCount()
    {
        var snapshot = new byte[112];
        BinaryPrimitives.WriteInt32LittleEndian(snapshot.AsSpan(32, sizeof(int)), 5);

        Assert.IsFalse(iRacingConnection.TryGetLatestTickCount(snapshot, out _));
    }

    [TestMethod]
    public void IsStableSnapshotHeaderRejectsHeaderChangesDuringCopy()
    {
        var before = new byte[112];
        var after = (byte[])before.Clone();
        var snapshot = new byte[4096];
        BinaryPrimitives.WriteInt32LittleEndian(after.AsSpan(48, sizeof(int)), 21);
        BinaryPrimitives.WriteInt32LittleEndian(snapshot.AsSpan(48, sizeof(int)), 21);

        Assert.IsFalse(iRacingMemory.IsStableSnapshotHeader(before, snapshot, after));
    }

    [TestMethod]
    public void IsStableSnapshotHeaderAcceptsMatchingHeaders()
    {
        var before = new byte[112];
        var snapshot = new byte[4096];
        var after = (byte[])before.Clone();

        Assert.IsTrue(iRacingMemory.IsStableSnapshotHeader(before, snapshot, after));
    }

    [TestMethod]
    public void IsStableSnapshotHeaderRejectsSnapshotHeaderMismatch()
    {
        var before = new byte[112];
        var snapshot = new byte[4096];
        var after = (byte[])before.Clone();
        snapshot[48] = 1;

        Assert.IsFalse(iRacingMemory.IsStableSnapshotHeader(before, snapshot, after));
    }

    [TestMethod]
    public void HasTickGapRejectsConsecutiveTicks() =>
        Assert.IsFalse(iRacingConnection.HasTickGap(10, 11));

    [TestMethod]
    public void HasTickGapDetectsSkippedTicks() =>
        Assert.IsTrue(iRacingConnection.HasTickGap(10, 12));

    [TestMethod]
    public void HasTickGapIgnoresCounterReset() =>
        Assert.IsFalse(iRacingConnection.HasTickGap(12, 10));
}

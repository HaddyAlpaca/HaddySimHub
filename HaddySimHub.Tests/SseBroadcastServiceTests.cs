using HaddySimHub.Models;
using HaddySimHub.Services;
using System.Threading.Channels;

namespace HaddySimHub.Tests;

[TestClass]
public class SseBroadcastServiceTests
{
    [TestMethod]
    public async Task Broadcast_ReachesEveryConnectedClient()
    {
        var service = new SseBroadcastService();
        var first = CreateClient();
        var second = CreateClient();

        service.AddClient(first.Writer);
        service.AddClient(second.Writer);

        var update = new DisplayUpdate { Type = DisplayType.RaceDashboard };
        await service.BroadcastAsync(update);

        Assert.AreSame(update, await ReadOneAsync(first));
        Assert.AreSame(update, await ReadOneAsync(second));
    }

    [TestMethod]
    public async Task Broadcast_StillReachesANewClientAfterAnOlderOneDisconnects()
    {
        // A browser refresh opens the new stream before the old one has finished
        // tearing down, so the late removal must not detach the new client.
        var service = new SseBroadcastService();
        var old = CreateClient();
        var replacement = CreateClient();

        service.AddClient(old.Writer);
        service.AddClient(replacement.Writer);
        service.RemoveClient(old.Writer);

        var update = new DisplayUpdate { Type = DisplayType.RallyDashboard };
        await service.BroadcastAsync(update);

        Assert.AreSame(update, await ReadOneAsync(replacement));
    }

    [TestMethod]
    public async Task Broadcast_SkipsClientsThatHaveDisconnected()
    {
        var service = new SseBroadcastService();
        var client = CreateClient();

        service.AddClient(client.Writer);
        service.RemoveClient(client.Writer);
        await service.BroadcastAsync(new DisplayUpdate { Type = DisplayType.RaceDashboard });

        Assert.IsFalse(client.Reader.TryRead(out _));
    }

    [TestMethod]
    public async Task Broadcast_WithoutClientsDoesNothing()
    {
        var service = new SseBroadcastService();

        await service.BroadcastAsync(new DisplayUpdate { Type = DisplayType.None });
    }

    [TestMethod]
    public void RemoveClient_IsSafeForAClientThatWasNeverAdded()
    {
        var service = new SseBroadcastService();

        service.RemoveClient(CreateClient().Writer);
    }

    private static Channel<DisplayUpdate> CreateClient() =>
        Channel.CreateBounded<DisplayUpdate>(new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        });

    private static async Task<DisplayUpdate> ReadOneAsync(Channel<DisplayUpdate> client)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        return await client.Reader.ReadAsync(timeout.Token);
    }
}

using DDVWF.Core.Zone;

namespace DDVWF.Core.Tests;

public sealed class ZoneSessionTests
{
    [Fact]
    public async Task Client_and_server_populate_before_render()
    {
        var order = new List<string>();
        var session = new ZoneSession(new Client(order), new Server(order), new Viewport(order));
        var zone = await session.OpenAsync("eq", "poknowledge");
        Assert.Equal(["client", "server", "render"], order);
        Assert.Equal("poknowledge", zone.ShortName);
        Assert.Same(zone, session.ActiveZone);
    }

    private sealed class Client(List<string> order) : IClientZoneProvider
    { public Task PopulateAsync(CompleteZone z,string r,CancellationToken c){order.Add("client");return Task.CompletedTask;} }
    private sealed class Server(List<string> order) : IServerDataProvider
    { public bool CanWrite=>false; public Task PopulateAsync(CompleteZone z,CancellationToken c){order.Add("server");return Task.CompletedTask;} }
    private sealed class Viewport(List<string> order) : IRenderViewport
    { public Task LoadAsync(CompleteZone z,CancellationToken c){order.Add("render");return Task.CompletedTask;} public void Upsert(ZoneEntity e){} public void Remove(Guid id){} }
}

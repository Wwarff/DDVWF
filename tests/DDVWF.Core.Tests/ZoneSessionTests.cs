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

    [Fact]
    public async Task Server_native_model_reference_is_resolved_before_render()
    {
        var viewport = new CapturingViewport();
        var session = new ZoneSession(new ResolvingClient(), new DoorServer(), viewport);
        var zone = await session.OpenAsync("eq", "poknowledge");
        var door = Assert.Single(zone.Entities);
        Assert.Equal("objects.wld#object#POKDOOR_DMSPRITEDEF", door.ClientAsset);
        Assert.Equal(door.ClientAsset, Assert.Single(viewport.Entities).ClientAsset);
    }

    private sealed class Client(List<string> order) : IClientZoneProvider
    { public Task PopulateAsync(CompleteZone z,string r,CancellationToken c){order.Add("client");return Task.CompletedTask;} }
    private sealed class Server(List<string> order) : IServerDataProvider
    { public bool CanWrite=>false; public Task PopulateAsync(CompleteZone z,CancellationToken c){order.Add("server");return Task.CompletedTask;} }
    private sealed class Viewport(List<string> order) : IRenderViewport
    { public Task LoadAsync(CompleteZone z,CancellationToken c){order.Add("render");return Task.CompletedTask;} public void Upsert(ZoneEntity e){} public void Remove(Guid id){} }

    private sealed class ResolvingClient : IClientZoneProvider, IClientAssetResolver
    {
        public Task PopulateAsync(CompleteZone z,string r,CancellationToken c)=>Task.CompletedTask;
        public string? ResolveClientAsset(string nativeReference)
            => string.Equals(nativeReference,"POKDOOR",StringComparison.OrdinalIgnoreCase) ? "objects.wld#object#POKDOOR_DMSPRITEDEF" : null;
    }

    private sealed class DoorServer : IServerDataProvider
    {
        public bool CanWrite=>false;
        public Task PopulateAsync(CompleteZone z,CancellationToken c)
        {
            z.Add(new(Guid.NewGuid(),ZoneEntityKind.Door,"POKDOOR",new(1,2,3,90),1,42,"POKDOOR"));
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingViewport : IRenderViewport
    {
        public IReadOnlyCollection<ZoneEntity> Entities { get; private set; } = Array.Empty<ZoneEntity>();
        public Task LoadAsync(CompleteZone z,CancellationToken c){Entities=z.Entities.ToArray();return Task.CompletedTask;}
        public void Upsert(ZoneEntity e){}
        public void Remove(Guid id){}
    }
}

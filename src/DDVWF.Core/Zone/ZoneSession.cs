namespace DDVWF.Core.Zone;

public sealed class ZoneSession
{
    private readonly IClientZoneProvider _client;
    private readonly IServerDataProvider _server;
    private readonly IRenderViewport _viewport;
    public CompleteZone? ActiveZone { get; private set; }

    public ZoneSession(IClientZoneProvider client, IServerDataProvider server, IRenderViewport viewport)
        => (_client, _server, _viewport) = (client, server, viewport);

    public async Task<CompleteZone> OpenAsync(string eqRoot, string shortName, CancellationToken cancellationToken = default)
    {
        var next = new CompleteZone { ShortName = shortName };
        await _client.PopulateAsync(next, eqRoot, cancellationToken);
        await _server.PopulateAsync(next, cancellationToken);
        await _viewport.LoadAsync(next, cancellationToken);
        ActiveZone = next;
        return next;
    }

    public void Close() => ActiveZone = null;
}

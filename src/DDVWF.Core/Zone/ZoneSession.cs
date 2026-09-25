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
        try { await _client.PopulateAsync(next, eqRoot, cancellationToken); }
        catch (Exception ex) { throw new InvalidDataException($"Client/Sage population failed for {shortName}: {ex.Message}", ex); }
        try { await _server.PopulateAsync(next, cancellationToken); }
        catch (Exception ex) { throw new InvalidDataException($"EQEmu server population failed for {shortName}: {ex.Message}", ex); }
        ResolveServerClientAssets(next);
        try { await _viewport.LoadAsync(next, cancellationToken); }
        catch (Exception ex) { throw new InvalidDataException($"Viewport scene-tree load failed for {shortName}: {ex.Message}", ex); }
        ActiveZone = next;
        return next;
    }

    private void ResolveServerClientAssets(CompleteZone zone)
    {
        if (_client is INpcClientAssetResolver npcResolver)
        {
            foreach (var entity in zone.Entities.Where(x => x.Kind==ZoneEntityKind.Npc && x.Data is NpcEntityData).ToArray())
            {
                var npc=(NpcEntityData)entity.Data!;
                var resolved=npcResolver.ResolveNpcAsset(npc.Race,npc.Gender,npc.Model,npc.Texture);
                if(!string.IsNullOrWhiteSpace(resolved)) zone.Replace(entity with { ClientAsset=resolved });
            }
        }
        if (_client is not IClientAssetResolver resolver) return;
        foreach (var entity in zone.Entities.Where(x => x.Kind==ZoneEntityKind.GroundSpawn && x.ServerId is not null && string.IsNullOrWhiteSpace(x.ClientAsset)).ToArray())
        {
            var resolved=resolver.ResolveClientAsset(entity.Name);
            if(!string.IsNullOrWhiteSpace(resolved)) zone.Replace(entity with { ClientAsset=resolved });
        }
        foreach (var entity in zone.Entities.Where(x => x.ServerId is not null && !string.IsNullOrWhiteSpace(x.ClientAsset)).ToArray())
        {
            var resolved = resolver.ResolveClientAsset(entity.ClientAsset!);
            if (!string.IsNullOrWhiteSpace(resolved) && !string.Equals(resolved, entity.ClientAsset, StringComparison.Ordinal))
                zone.Replace(entity with { ClientAsset = resolved });
        }
    }

    public void Close() => ActiveZone = null;
}

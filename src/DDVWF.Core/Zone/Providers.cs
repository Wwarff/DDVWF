namespace DDVWF.Core.Zone;

public interface IClientZoneProvider
{
    Task PopulateAsync(CompleteZone zone, string eqRoot, CancellationToken cancellationToken);
}

public interface IClientAssetResolver
{
    string? ResolveClientAsset(string nativeReference);
}

public interface INpcClientAssetResolver
{
    string? ResolveNpcAsset(int race, int gender, int model = 0);
}

public sealed record ServerReadDiagnostics(int Spawn2,int SpawnEntries,int NpcTypes,int SpawnGroups,int Doors,int ZonePoints,int Objects,int GroundSpawns);

public interface IServerReadDiagnosticsProvider { ServerReadDiagnostics? LastReadDiagnostics { get; } }

public interface IServerDataProvider
{
    bool CanWrite { get; }
    Task PopulateAsync(CompleteZone zone, CancellationToken cancellationToken);
}

public interface IRenderViewport
{
    Task LoadAsync(CompleteZone zone, CancellationToken cancellationToken);
    void Upsert(ZoneEntity entity);
    void Remove(Guid id);
}

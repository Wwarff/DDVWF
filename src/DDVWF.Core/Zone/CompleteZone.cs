namespace DDVWF.Core.Zone;

public enum ZoneEntityKind
{
    WorldGeometry, StaticObject, AnimatedObject, Door, Light, Region, Npc, Spawn, ZonePoint
}

public readonly record struct EqPosition(float X, float Y, float Z, float Heading = 0);

public sealed record ZoneEntity(
    Guid Id,
    ZoneEntityKind Kind,
    string Name,
    EqPosition Position,
    float Scale = 1.0f,
    long? ServerId = null,
    string? ClientAsset = null);

public sealed class CompleteZone
{
    private readonly Dictionary<Guid, ZoneEntity> _entities = new();
    public required string ShortName { get; init; }
    public IReadOnlyCollection<ZoneEntity> Entities => _entities.Values;
    public void Add(ZoneEntity entity) => _entities.Add(entity.Id, entity);
    public ZoneEntity Get(Guid id) => _entities[id];
    public void Replace(ZoneEntity entity) => _entities[entity.Id] = entity;
    public bool Remove(Guid id) => _entities.Remove(id);
    public bool Contains(Guid id) => _entities.ContainsKey(id);
}

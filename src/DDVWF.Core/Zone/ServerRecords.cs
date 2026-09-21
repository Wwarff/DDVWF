namespace DDVWF.Core.Zone;

public sealed record Spawn2Record(long Id, long SpawnGroupId, string Zone, float X, float Y, float Z, float Heading, int RespawnTime, int Variance, int PathGrid);
public sealed record SpawnEntryRecord(long SpawnGroupId, long NpcId, int Chance);
public sealed record NpcTypeRecord(long Id, string Name, int Race, int Gender, float Size);
public sealed record DoorRecord(long Id, int DoorId, string Zone, string Model, float X, float Y, float Z, float Heading, int OpenType, int Size);

public sealed record ServerZoneSnapshot(
    IReadOnlyList<Spawn2Record> Spawn2,
    IReadOnlyList<SpawnEntryRecord> SpawnEntries,
    IReadOnlyList<NpcTypeRecord> NpcTypes,
    IReadOnlyList<DoorRecord> Doors);

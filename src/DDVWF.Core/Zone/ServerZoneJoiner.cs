namespace DDVWF.Core.Zone;

public static class ServerZoneJoiner
{
    public static void Join(CompleteZone zone, ServerZoneSnapshot snapshot)
    {
        var entriesByGroup = snapshot.SpawnEntries.GroupBy(x => x.SpawnGroupId).ToDictionary(x => x.Key, x => x.ToArray());
        var npcs = snapshot.NpcTypes.ToDictionary(x => x.Id);

        foreach (var spawn in snapshot.Spawn2.Where(x => string.Equals(x.Zone, zone.ShortName, StringComparison.OrdinalIgnoreCase)))
        {
            if (!entriesByGroup.TryGetValue(spawn.SpawnGroupId, out var entries)) continue;
            foreach (var entry in entries)
            {
                if (!npcs.TryGetValue(entry.NpcId, out var npc)) continue;
                zone.Add(new ZoneEntity(
                    DeterministicId("spawn", spawn.Id, npc.Id),
                    ZoneEntityKind.Npc,
                    npc.Name,
                    new EqPosition(spawn.X, spawn.Y, spawn.Z, spawn.Heading),
                    npc.Size <= 0 ? 1f : npc.Size,
                    spawn.Id,
                    ClientAsset: null));
            }
        }

        foreach (var door in snapshot.Doors.Where(x => string.Equals(x.Zone, zone.ShortName, StringComparison.OrdinalIgnoreCase)))
            zone.Add(new ZoneEntity(
                DeterministicId("door", door.Id, door.DoorId),
                ZoneEntityKind.Door,
                door.Model,
                new EqPosition(door.X, door.Y, door.Z, door.Heading),
                door.Size <= 0 ? 1f : door.Size / 100f,
                door.Id,
                door.Model));
    }

    private static Guid DeterministicId(string kind, long a, long b)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"{kind}:{a}:{b}"));
        return new Guid(bytes.AsSpan(0, 16));
    }
}

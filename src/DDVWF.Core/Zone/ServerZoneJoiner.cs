namespace DDVWF.Core.Zone;

public static class ServerZoneJoiner
{
    public static void Join(CompleteZone zone, ServerZoneSnapshot snapshot)
    {
        var entriesByGroup = snapshot.SpawnEntries.GroupBy(x => x.SpawnGroupId).ToDictionary(x => x.Key, x => x.ToArray());
        var npcs = snapshot.NpcTypes.ToDictionary(x => x.Id);

        foreach (var spawn in snapshot.Spawn2.Where(x => string.Equals(x.Zone, zone.ShortName, StringComparison.OrdinalIgnoreCase)))
        {
            zone.Add(new ZoneEntity(
                DeterministicId("spawn", spawn.Id, spawn.SpawnGroupId),
                ZoneEntityKind.Spawn,
                $"Spawn {spawn.Id}",
                new EqPosition(spawn.X, spawn.Y, spawn.Z, spawn.Heading),
                1f,
                spawn.Id,
                null,
                new SpawnEntityData(spawn.SpawnGroupId,spawn.RespawnTime,spawn.Variance,spawn.PathGrid,spawn.Version,spawn.PathWhenZoneIdle,spawn.Condition,spawn.ConditionValue,spawn.Animation,spawn.MinExpansion,spawn.MaxExpansion,spawn.ContentFlags,spawn.ContentFlagsDisabled)));

            if (!entriesByGroup.TryGetValue(spawn.SpawnGroupId, out var entries)) continue;
            foreach (var entry in entries)
            {
                if (!npcs.TryGetValue(entry.NpcId, out var npc)) continue;
                zone.Add(new ZoneEntity(
                    DeterministicId("npc", spawn.Id, npc.Id),
                    ZoneEntityKind.Npc,
                    npc.Name,
                    new EqPosition(spawn.X, spawn.Y, spawn.Z, spawn.Heading),
                    npc.Size <= 0 ? 1f : npc.Size,
                    spawn.Id,
                    null,
                    new NpcEntityData(npc.Id,spawn.SpawnGroupId,entry.Chance,npc.Race,npc.Gender,npc.Model,npc.Texture,npc.HelmTexture,npc.HeroForgeModel,npc.Face,npc.HairStyle,npc.HairColor,npc.EyeColor1,npc.EyeColor2,npc.BeardColor,npc.Beard,npc.DrakkinHeritage,npc.DrakkinTattoo,npc.DrakkinDetails,npc.ArmTexture,npc.BracerTexture,npc.HandTexture,npc.LegTexture,npc.FeetTexture,npc.PrimaryWeaponTexture,npc.SecondaryWeaponTexture,npc.Light,entry.ConditionValueFilter,entry.MinTime,entry.MaxTime,entry.MinExpansion,entry.MaxExpansion,entry.ContentFlags,entry.ContentFlagsDisabled)));
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
                door.Model,
                new DoorEntityData(door.DoorId,door.OpenType,door.Version,door.Lockpick,door.KeyItem,door.TriggerDoor,door.TriggerType,door.DoorIsOpen,door.DestZone,door.DestInstance,door.DestX,door.DestY,door.DestZ,door.DestHeading,door.InvertState,door.Incline)));
        foreach (var point in snapshot.ZonePoints ?? Array.Empty<ZonePointRecord>())
        {
            if (!string.Equals(point.Zone, zone.ShortName, StringComparison.OrdinalIgnoreCase)) continue;
            zone.Add(new ZoneEntity(
                DeterministicId("zonepoint", point.Id, point.Number),
                ZoneEntityKind.ZonePoint,
                $"Zone Point {point.Number}",
                new EqPosition(point.X, point.Y, point.Z, point.Heading),
                1f,
                point.Id,
                null,
                new ZonePointEntityData(point.Version,point.Number,point.TargetX,point.TargetY,point.TargetZ,point.TargetHeading,point.TargetZoneId,point.TargetInstance,point.ZoneInstance,point.Buffer,point.ClientVersionMask,point.MinExpansion,point.MaxExpansion,point.ContentFlags,point.ContentFlagsDisabled,point.IsVirtual,point.Height,point.Width)));
        }
    }

    private static Guid DeterministicId(string kind, long a, long b)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"{kind}:{a}:{b}"));
        return new Guid(bytes.AsSpan(0, 16));
    }
}

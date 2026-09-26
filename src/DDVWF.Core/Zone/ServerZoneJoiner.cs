namespace DDVWF.Core.Zone;

public static class ServerZoneJoiner
{
    public static void Join(CompleteZone zone, ServerZoneSnapshot snapshot)
    {
        var entriesByGroup = snapshot.SpawnEntries.GroupBy(x => x.SpawnGroupId).ToDictionary(x => x.Key, x => x.ToArray());
        var npcs = snapshot.NpcTypes.ToDictionary(x => x.Id);
        var groups = (snapshot.SpawnGroups ?? Array.Empty<SpawnGroupRecord>()).ToDictionary(x=>x.Id);

        var gridTypes=(snapshot.Grids??Array.Empty<GridRecord>()).ToDictionary(x=>x.Id);
        foreach(var wp in snapshot.GridEntries??Array.Empty<GridEntryRecord>())
        {
            gridTypes.TryGetValue(wp.GridId,out var grid);
            zone.Add(new ZoneEntity(
                DeterministicId("waypoint",wp.GridId,wp.Number),
                ZoneEntityKind.Spawn,
                $"Grid {wp.GridId} waypoint {wp.Number}",
                new EqPosition(wp.X,wp.Y,wp.Z,wp.Heading),
                1f,null,null,
                new WaypointEntityData(wp.GridId,wp.Number,wp.Pause,wp.CenterPoint,grid?.WanderType??0,grid?.PauseType??0)));
        }

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
                new SpawnEntityData(spawn.SpawnGroupId,spawn.RespawnTime,spawn.Variance,spawn.PathGrid,spawn.Version,spawn.PathWhenZoneIdle,spawn.Condition,spawn.ConditionValue,spawn.Animation,spawn.MinExpansion,spawn.MaxExpansion,spawn.ContentFlags,spawn.ContentFlagsDisabled,groups.TryGetValue(spawn.SpawnGroupId,out var sg)?sg.Name:"",sg?.SpawnLimit??0,sg?.Distance??0,sg?.MaxX??0,sg?.MinX??0,sg?.MaxY??0,sg?.MinY??0,sg?.Delay??0,sg?.MinDelay??0,sg?.Despawn??0,sg?.DespawnTimer??0,sg?.WaypointSpawns??false)));

            if (!entriesByGroup.TryGetValue(spawn.SpawnGroupId, out var entries)) continue;
            // Pinned EQ Sage SpawnController.addSpawns renders spawn.spawnentries[0].npc_type for each spawn2 point.
            // Preserve Spire/API order explicitly instead of inventing a chance-weighted or random active NPC here.
            var entry=entries.FirstOrDefault();
            if (entry is not null && npcs.TryGetValue(entry.NpcId, out var npc))
            {
                zone.Add(new ZoneEntity(
                    DeterministicId("npc", spawn.Id, npc.Id),
                    ZoneEntityKind.Npc,
                    npc.Name,
                    new EqPosition(spawn.X, spawn.Y, spawn.Z, spawn.Heading),
                    npc.Size,
                    spawn.Id,
                    null,
                    new NpcEntityData(npc.Id,spawn.SpawnGroupId,entry.Chance,npc.Race,npc.Gender,npc.Model,npc.Texture,npc.HelmTexture,npc.HeroForgeModel,npc.Face,npc.HairStyle,npc.HairColor,npc.EyeColor1,npc.EyeColor2,npc.BeardColor,npc.Beard,npc.DrakkinHeritage,npc.DrakkinTattoo,npc.DrakkinDetails,npc.ArmTexture,npc.BracerTexture,npc.HandTexture,npc.LegTexture,npc.FeetTexture,npc.PrimaryWeaponTexture,npc.SecondaryWeaponTexture,npc.Light,entry.ConditionValueFilter,entry.MinTime,entry.MaxTime,entry.MinExpansion,entry.MaxExpansion,entry.ContentFlags,entry.ContentFlagsDisabled)));
            }
        }

        foreach (var ground in snapshot.GroundSpawns ?? Array.Empty<GroundSpawnRecord>())
        {
            zone.Add(new ZoneEntity(
                DeterministicId("groundspawn",ground.Id,ground.ZoneId),
                ZoneEntityKind.GroundSpawn,
                ground.Name,
                new EqPosition((ground.MinX+ground.MaxX)/2f,(ground.MinY+ground.MaxY)/2f,ground.MaxZ,ground.Heading),
                1f,
                ground.Id,
                null,
                new GroundSpawnEntityData(ground.Version,ground.MinX,ground.MaxX,ground.MinY,ground.MaxY,ground.MaxZ,ground.ItemId,ground.MaxAllowed,ground.Comment,ground.RespawnTimer,ground.FixZ,ground.MinExpansion,ground.MaxExpansion,ground.ContentFlags,ground.ContentFlagsDisabled)));
        }

        foreach (var obj in snapshot.Objects ?? Array.Empty<ObjectRecord>())
        {
            var native=obj.ObjectName.Replace("_ACTORDEF","",StringComparison.OrdinalIgnoreCase);
            if(obj.Type==0)
            {
                var openType=obj.SolidType switch{0=>31,1=>9,_=>obj.SolidType};
                zone.Add(new ZoneEntity(
                    DeterministicId("object-door",1000000000L+obj.Id,-1),
                    ZoneEntityKind.Door,
                    native,
                    new EqPosition(obj.X,obj.Y,obj.Z,obj.Heading),
                    1f,
                    1000000000L+obj.Id,
                    native,
                    new DoorEntityData(-1,openType,obj.Version,Incline:obj.Incline)));
                continue;
            }
            if(obj.Type==1&&obj.ItemId!=0)continue;
            zone.Add(new ZoneEntity(
                DeterministicId("object", obj.Id, obj.ZoneId),
                ZoneEntityKind.StaticObject,
                string.IsNullOrWhiteSpace(obj.DisplayName)?native:obj.DisplayName,
                new EqPosition(obj.X,obj.Y,obj.BestZ??obj.Z,obj.Heading),
                obj.Size>0&&obj.Size<5000?obj.Size/100f:1f,
                obj.Id,
                native,
                new ObjectEntityData(obj.Version,obj.ItemId,obj.Charges,obj.Type,obj.Icon,obj.SizePercentage,obj.Unknown24,obj.Unknown60,obj.Unknown64,obj.Unknown68,obj.Unknown72,obj.Unknown76,obj.Unknown84,obj.Size,obj.SolidType,obj.Incline,obj.TiltX,obj.TiltY,obj.DisplayName,obj.MinExpansion,obj.MaxExpansion,obj.ContentFlags,obj.ContentFlagsDisabled,(snapshot.ObjectContents??Array.Empty<ObjectContentRecord>()).Where(x=>x.ParentId==obj.Id).Select(x=>new ObjectContentData(x.BagIndex,x.ItemId,x.Charges,x.DropTime,x.AugSlot1,x.AugSlot2,x.AugSlot3,x.AugSlot4,x.AugSlot5,x.AugSlot6)).ToArray()),
                new EqRotation(obj.TiltX,obj.Heading,obj.TiltY)));
        }

        foreach (var door in snapshot.Doors.Where(x => string.Equals(x.Zone, zone.ShortName, StringComparison.OrdinalIgnoreCase)))
            zone.Add(new ZoneEntity(
                DeterministicId("door", door.Id, door.DoorId),
                ZoneEntityKind.Door,
                door.Model,
                new EqPosition(door.X, door.Y, door.Z, door.Heading),
                door.Size / 100f,
                door.Id,
                door.Model,
                new DoorEntityData(door.DoorId,door.OpenType,door.Version,door.Lockpick,door.KeyItem,door.TriggerDoor,door.TriggerType,door.DoorIsOpen,door.DestZone,door.DestInstance,door.DestX,door.DestY,door.DestZ,door.DestHeading,door.InvertState,door.Incline,door.Guild,door.NoKeyRing,door.DisableTimer,door.DoorParam,door.Buffer,door.ClientVersionMask,door.IsLdonDoor,door.CloseTimerMs,door.DzSwitchId,door.MinExpansion,door.MaxExpansion,door.ContentFlags,door.ContentFlagsDisabled)));
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

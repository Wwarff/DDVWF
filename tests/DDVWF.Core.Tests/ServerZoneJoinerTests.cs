using DDVWF.Core.Zone;

namespace DDVWF.Core.Tests;

public sealed class ServerZoneJoinerTests
{
    [Fact]
    public void Preserves_spawn_graph_npc_semantics_and_doors_for_active_zone_only()
    {
        var zone = new CompleteZone { ShortName = "poknowledge" };
        var snapshot = new ServerZoneSnapshot(
            [new(10, 20, "poknowledge", 1, 2, 3, 64, 1200, 25, 7), new(11, 21, "other", 9, 9, 9, 0, 1, 0, 0)],
            [new(20, 30, 75), new(21, 31, 100)],
            [new(30, "Scholar", 1, 0, 6), new(31, "Elsewhere", 1, 0, 6)],
            [new(40, 5, "poknowledge", "POKDOOR", 4, 5, 6, 32, 2, 100)]);

        ServerZoneJoiner.Join(zone, snapshot);

        Assert.Equal(3, zone.Entities.Count);
        var spawn=Assert.Single(zone.Entities.Where(x=>x.Kind==ZoneEntityKind.Spawn));
        Assert.Equal(new SpawnEntityData(20,1200,25,7),spawn.Data);
        var npc=Assert.Single(zone.Entities.Where(x=>x.Kind==ZoneEntityKind.Npc));
        Assert.Equal("Scholar",npc.Name);
        Assert.Equal(new NpcEntityData(30,20,75,1,0),npc.Data);
        var door=Assert.Single(zone.Entities.Where(x=>x.Kind==ZoneEntityKind.Door));
        Assert.Equal(new DoorEntityData(5,2),door.Data);
        Assert.DoesNotContain(zone.Entities,x=>x.Name=="Elsewhere");
    }
    [Fact]
    public void Preserves_current_eqemu_door_runtime_semantics()
    {
        var zone=new CompleteZone{ShortName="poknowledge"};
        var door=new DoorRecord(40,5,"poknowledge","POKDOOR",4,5,6,128,2,125,0,10,1001,7,8,true,"nexus",2,11,12,13,64,1,32,77,true,true,1234,6.5f,0x12345678,true,9000,55,2,8,"era","disabled");
        ServerZoneJoiner.Join(zone,new ServerZoneSnapshot(Array.Empty<Spawn2Record>(),Array.Empty<SpawnEntryRecord>(),Array.Empty<NpcTypeRecord>(),new[]{door}));
        var data=Assert.IsType<DoorEntityData>(Assert.Single(zone.Entities.Where(x=>x.Kind==ZoneEntityKind.Door)).Data);
        Assert.Equal(77,data.Guild);Assert.True(data.NoKeyRing);Assert.True(data.DisableTimer);Assert.Equal(1234,data.DoorParam);Assert.Equal(6.5f,data.Buffer);Assert.Equal(0x12345678u,data.ClientVersionMask);Assert.True(data.IsLdonDoor);Assert.Equal(9000,data.CloseTimerMs);Assert.Equal(55,data.DzSwitchId);Assert.Equal(2,data.MinExpansion);Assert.Equal(8,data.MaxExpansion);Assert.Equal("era",data.ContentFlags);Assert.Equal("disabled",data.ContentFlagsDisabled);
    }

    [Fact]
    public void Preserves_eqemu_world_objects_static_locked_doors_and_ground_spawn_bounds()
    {
        var zone=new CompleteZone{ShortName="poknowledge"};
        var obj=new ObjectRecord(7,202,0,10,20,30,64,0,0,"POKDOOR_ACTORDEF",0,66,150,1,2,3,4,5,6,7,125,1,9,11,12,"Portal",-1,-1,"era","off") with{BestZ=35};
        var ground=new GroundSpawnRecord(8,202,0,100,200,30,50,150,128,"IT63_ACTORDEF",1001,2,"test",60,true,-1,-1,"era","");
        var snapshot=new ServerZoneSnapshot(Array.Empty<Spawn2Record>(),Array.Empty<SpawnEntryRecord>(),Array.Empty<NpcTypeRecord>(),Array.Empty<DoorRecord>(),Objects:new[]{obj},GroundSpawns:new[]{ground});

        ServerZoneJoiner.Join(zone,snapshot);

        var world=Assert.Single(zone.Entities.Where(x=>x.Data is ObjectEntityData));
        Assert.Equal("POKDOOR",world.ClientAsset);
        Assert.Equal(1.25f,world.Scale);
        Assert.Equal(new EqPosition(10,20,35,64),world.Position);
        Assert.Equal(new EqRotation(11,64,12),world.Rotation);
        var data=Assert.IsType<ObjectEntityData>(world.Data);
        Assert.Equal(125,data.NativeSize);
        Assert.Equal(7,data.Unknown84);
        var synthetic=Assert.Single(zone.Entities.Where(x=>x.Kind==ZoneEntityKind.Door));
        Assert.Equal(1000000007L,synthetic.ServerId);
        Assert.Equal(1f,synthetic.Scale);
        Assert.Equal(new EqPosition(10,20,30,64),synthetic.Position);
        Assert.Equal(new DoorEntityData(-1,9,0,Incline:9),synthetic.Data);
        var gs=Assert.Single(zone.Entities.Where(x=>x.Kind==ZoneEntityKind.GroundSpawn));
        Assert.Equal(75,gs.Position.X);
        Assert.Equal(175,gs.Position.Y);
        Assert.Equal(30,gs.Position.Z);
        Assert.Equal(1001,Assert.IsType<GroundSpawnEntityData>(gs.Data).ItemId);
    }

    [Fact]
    public void Matches_eqemu_temporary_item_object_removal()
    {
        var zone=new CompleteZone{ShortName="poknowledge"};
        var obj=new ObjectRecord(10,202,0,1,2,3,64,1001,1,"IT63_ACTORDEF",1,0,0,0,0,0,0,0,0,0,100,0,0,0,0,"Temporary item");
        var snapshot=new ServerZoneSnapshot(Array.Empty<Spawn2Record>(),Array.Empty<SpawnEntryRecord>(),Array.Empty<NpcTypeRecord>(),Array.Empty<DoorRecord>(),Objects:new[]{obj});
        ServerZoneJoiner.Join(zone,snapshot);
        Assert.DoesNotContain(zone.Entities,x=>x.ServerId==10&&x.Data is ObjectEntityData);
    }

    [Fact]
    public void Matches_eqemu_ground_object_size_guard()
    {
        var zone=new CompleteZone{ShortName="poknowledge"};
        var obj=new ObjectRecord(9,202,0,1,2,3,0,0,0,"IT63_ACTORDEF",2,0,0,0,0,0,0,0,0,0,5000,0,0,0,0,"Oversize");
        var snapshot=new ServerZoneSnapshot(Array.Empty<Spawn2Record>(),Array.Empty<SpawnEntryRecord>(),Array.Empty<NpcTypeRecord>(),Array.Empty<DoorRecord>(),Objects:new[]{obj});
        ServerZoneJoiner.Join(zone,snapshot);
        var world=Assert.Single(zone.Entities.Where(x=>x.Data is ObjectEntityData));
        Assert.Equal(1f,world.Scale);
    }

    [Fact]
    public void Joins_eqemu_object_contents_by_parent_object()
    {
        var zone=new CompleteZone{ShortName="poknowledge"};
        var obj=new ObjectRecord(7,202,0,1,2,3,0,0,0,"CHEST_ACTORDEF",1,0,100,0,0,0,0,0,0,0,100,0,0,0,0,"Chest");
        var content=new ObjectContentRecord(202,7,2,1001,3,null,11,12,13,14,15,16);
        var snapshot=new ServerZoneSnapshot(Array.Empty<Spawn2Record>(),Array.Empty<SpawnEntryRecord>(),Array.Empty<NpcTypeRecord>(),Array.Empty<DoorRecord>(),Objects:new[]{obj},ObjectContents:new[]{content});
        ServerZoneJoiner.Join(zone,snapshot);
        var data=Assert.IsType<ObjectEntityData>(Assert.Single(zone.Entities.Where(x=>x.Data is ObjectEntityData)).Data);
        var item=Assert.Single(data.Contents!);Assert.Equal(2,item.BagIndex);Assert.Equal(1001,item.ItemId);Assert.Equal(16,item.AugSlot6);
    }

    [Fact]
    public void Matches_eq_sage_first_spawnentry_visualization_for_multi_entry_groups()
    {
        var zone=new CompleteZone{ShortName="poknowledge"};
        var snapshot=new ServerZoneSnapshot(
            [new(10,20,"poknowledge",1,2,3,64,1200,0,0)],
            [new(20,30,25),new(20,31,75)],
            [new(30,"First",1,0,6),new(31,"Alternate",1,0,6)],
            Array.Empty<DoorRecord>());
        ServerZoneJoiner.Join(zone,snapshot);
        var npc=Assert.Single(zone.Entities.Where(x=>x.Kind==ZoneEntityKind.Npc));
        Assert.Equal("First",npc.Name);
        Assert.Equal(30,Assert.IsType<NpcEntityData>(npc.Data).NpcTypeId);
    }

    [Fact]
    public void Preserves_zero_npc_size_for_exact_sage_default_scaling()
    {
        var zone=new CompleteZone{ShortName="poknowledge"};
        var snapshot=new ServerZoneSnapshot(
            [new(10,20,"poknowledge",1,2,3,64,1200,0,0)],
            [new(20,30,100)],
            [new(30,"DefaultSize",1,0,0)],
            Array.Empty<DoorRecord>());
        ServerZoneJoiner.Join(zone,snapshot);
        var npc=Assert.Single(zone.Entities.Where(x=>x.Kind==ZoneEntityKind.Npc));
        Assert.Equal(0f,npc.Scale);
    }
}

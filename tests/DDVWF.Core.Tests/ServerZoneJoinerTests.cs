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
}

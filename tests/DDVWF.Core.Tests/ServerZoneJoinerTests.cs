using DDVWF.Core.Zone;

namespace DDVWF.Core.Tests;

public sealed class ServerZoneJoinerTests
{
    [Fact]
    public void Joins_spawn_graph_and_doors_for_active_zone_only()
    {
        var zone = new CompleteZone { ShortName = "poknowledge" };
        var snapshot = new ServerZoneSnapshot(
            [new(10, 20, "poknowledge", 1, 2, 3, 64, 1200, 0, 7), new(11, 21, "other", 9, 9, 9, 0, 1, 0, 0)],
            [new(20, 30, 100), new(21, 31, 100)],
            [new(30, "Scholar", 1, 0, 6), new(31, "Elsewhere", 1, 0, 6)],
            [new(40, 5, "poknowledge", "POKDOOR", 4, 5, 6, 32, 0, 100)]);

        ServerZoneJoiner.Join(zone, snapshot);

        Assert.Equal(2, zone.Entities.Count);
        Assert.Contains(zone.Entities, x => x.Kind == ZoneEntityKind.Npc && x.Name == "Scholar" && x.ServerId == 10);
        Assert.Contains(zone.Entities, x => x.Kind == ZoneEntityKind.Door && x.Name == "POKDOOR" && x.ServerId == 40);
    }
}

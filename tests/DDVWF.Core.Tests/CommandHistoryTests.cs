using DDVWF.Core.Commands;
using DDVWF.Core.Zone;

namespace DDVWF.Core.Tests;

public sealed class CommandHistoryTests
{
    [Fact]
    public void Redo_reapplies_command()
    {
        var zone = new CompleteZone { ShortName = "test" };
        var id = Guid.NewGuid();
        zone.Add(new(id, ZoneEntityKind.StaticObject, "tree", new(0,0,0)));
        var history = new CommandHistory();
        history.Execute(new TransformCommand(zone, id, new(3,4,5), 2));
        Assert.True(history.Undo());
        Assert.Equal(new EqPosition(0,0,0), zone.Get(id).Position);
        Assert.True(history.Redo());
        Assert.Equal(new EqPosition(3,4,5), zone.Get(id).Position);
    }
}

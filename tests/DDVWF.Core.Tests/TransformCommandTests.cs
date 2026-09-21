using DDVWF.Core.Commands;
using DDVWF.Core.Zone;

namespace DDVWF.Core.Tests;

public sealed class TransformCommandTests
{
    [Fact]
    public void Transform_is_reversible()
    {
        var zone = new CompleteZone { ShortName = "test" };
        var id = Guid.NewGuid();
        var original = new ZoneEntity(id, ZoneEntityKind.StaticObject, "rock", new(1,2,3), 1);
        zone.Add(original);
        var command = new TransformCommand(zone, id, new(4,5,6), 2);
        command.Execute();
        Assert.Equal(new EqPosition(4,5,6), zone.Get(id).Position);
        Assert.Equal(2, zone.Get(id).Scale);
        command.Undo();
        Assert.Equal(original, zone.Get(id));
    }
}

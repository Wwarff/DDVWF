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
    [Fact]
    public void Full_rotation_is_reversible()
    {
        var zone=new CompleteZone{ShortName="test"};var id=Guid.NewGuid();
        var original=new ZoneEntity(id,ZoneEntityKind.StaticObject,"sign",new(1,2,3,90),1,null,"sign",null,new EqRotation(0,90,45));zone.Add(original);
        var command=new TransformCommand(zone,id,new(4,5,6,180),2,new EqRotation(10,180,20));command.Execute();
        Assert.Equal(new EqRotation(10,180,20),zone.Get(id).Rotation);
        command.Undo();Assert.Equal(original,zone.Get(id));
    }
}

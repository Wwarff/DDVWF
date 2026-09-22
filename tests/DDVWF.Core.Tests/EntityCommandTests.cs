using DDVWF.Core.Commands;
using DDVWF.Core.Zone;
namespace DDVWF.Core.Tests;
public sealed class EntityCommandTests
{
 [Fact] public void Delete_and_undo_restore_same_identity(){var z=new CompleteZone{ShortName="x"};var id=Guid.NewGuid();z.Add(new(id,ZoneEntityKind.StaticObject,"rock",new()));var h=new CommandHistory();h.Execute(new DeleteEntityCommand(z,id));Assert.False(z.Contains(id));h.Undo();Assert.True(z.Contains(id));}
 [Fact] public void Duplicate_does_not_copy_server_identity(){var z=new CompleteZone{ShortName="x"};var id=Guid.NewGuid();z.Add(new(id,ZoneEntityKind.Npc,"npc",new(),1,99));var c=new DuplicateEntityCommand(z,id,new(1,2,3));c.Execute();var e=z.Get(c.NewId);Assert.Null(e.ServerId);Assert.Equal(new EqPosition(1,2,3),e.Position);}
}

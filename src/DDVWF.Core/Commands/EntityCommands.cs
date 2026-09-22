using DDVWF.Core.Zone;

namespace DDVWF.Core.Commands;

public sealed class DeleteEntityCommand : IEditorCommand
{
    private readonly CompleteZone _zone;
    private readonly ZoneEntity _entity;
    public DeleteEntityCommand(CompleteZone zone, Guid id) => (_zone,_entity)=(zone,zone.Get(id));
    public void Execute()=>_zone.Remove(_entity.Id);
    public void Undo(){ if(!_zone.Contains(_entity.Id)) _zone.Add(_entity); }
}

public sealed class DuplicateEntityCommand : IEditorCommand
{
    private readonly CompleteZone _zone;
    private readonly ZoneEntity _copy;
    public Guid NewId=>_copy.Id;
    public DuplicateEntityCommand(CompleteZone zone,Guid sourceId,EqPosition position)
    {
        var source=zone.Get(sourceId);
        _copy=source with { Id=Guid.NewGuid(), Position=position, ServerId=null };
    }
    public void Execute(){if(!_zone.Contains(_copy.Id))_zone.Add(_copy);}
    public void Undo()=>_zone.Remove(_copy.Id);
}

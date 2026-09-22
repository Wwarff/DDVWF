using DDVWF.Core.Zone;

namespace DDVWF.Core.Commands;

public interface IEditorCommand
{
    void Execute();
    void Undo();
}

public sealed class TransformCommand : IEditorCommand
{
    private readonly CompleteZone _zone;
    private readonly Guid _id;
    private readonly ZoneEntity _before;
    private readonly ZoneEntity _after;

    public TransformCommand(CompleteZone zone, Guid id, EqPosition position, float scale, EqRotation? rotation = null)
    {
        _zone = zone;
        _id = id;
        _before = zone.Get(id);
        _after = _before with { Position = position, Scale = scale, Rotation = rotation ?? _before.Rotation };
    }

    public void Execute() => _zone.Replace(_after);
    public void Undo() => _zone.Replace(_before);
}

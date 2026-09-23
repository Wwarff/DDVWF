namespace DDVWF.Core.Zone;

public enum ZoneEntityKind
{
    WorldGeometry, StaticObject, AnimatedObject, Door, Light, Region, Npc, Spawn, ZonePoint
}

public readonly record struct EqPosition(float X, float Y, float Z, float Heading = 0);
public readonly record struct EqRotation(float X, float Y, float Z);

public abstract record ZoneEntityData;
public sealed record SpawnEntityData(long SpawnGroupId,int RespawnTime,int Variance,int PathGrid) : ZoneEntityData;
public sealed record NpcEntityData(long NpcTypeId,long SpawnGroupId,int Chance,int Race,int Gender,int Model=0,int Texture=0,int HelmTexture=0,int HeroForgeModel=0,int Face=0,int HairStyle=0,int HairColor=0,int EyeColor1=0,int EyeColor2=0,int BeardColor=0,int Beard=0,int DrakkinHeritage=0,int DrakkinTattoo=0,int DrakkinDetails=0,int ArmTexture=0,int BracerTexture=0,int HandTexture=0,int LegTexture=0,int FeetTexture=0,int PrimaryWeaponTexture=0,int SecondaryWeaponTexture=0,int Light=0) : ZoneEntityData;
public sealed record DoorEntityData(int DoorId,int OpenType) : ZoneEntityData;
public enum RegionSemantic { Normal, Water, Lava, Pvp, Zoneline, WaterBlockLos, FreezingWater, Slippery, Unknown }
public sealed record LightFrame(float Level,float Red,float Green,float Blue);
public sealed record LightEntityData(float Radius,uint Flags,uint CurrentFrame,uint Sleep,IReadOnlyList<LightFrame> Frames,bool GlobalAmbient=false) : ZoneEntityData;
public sealed record RegionEntityData(IReadOnlyList<RegionSemantic> Semantics,int? ZoneLineReference=null,int? TargetZoneIndex=null,int? TargetX=null,int? TargetY=null,int? TargetZ=null,int? TargetRotation=null) : ZoneEntityData;
public sealed record ZonePointEntityData(int Version,int Number,float TargetX,float TargetY,float TargetZ,float TargetHeading,uint TargetZoneId,uint TargetInstance) : ZoneEntityData;

public sealed record ZoneEntity(
    Guid Id,
    ZoneEntityKind Kind,
    string Name,
    EqPosition Position,
    float Scale = 1.0f,
    long? ServerId = null,
    string? ClientAsset = null,
    ZoneEntityData? Data = null,
    EqRotation? Rotation = null);

public sealed class CompleteZone
{
    private readonly Dictionary<Guid, ZoneEntity> _entities = new();
    public required string ShortName { get; init; }
    public IReadOnlyCollection<ZoneEntity> Entities => _entities.Values;
    public void Add(ZoneEntity entity) => _entities.Add(entity.Id, entity);
    public ZoneEntity Get(Guid id) => _entities[id];
    public void Replace(ZoneEntity entity) => _entities[entity.Id] = entity;
    public bool Remove(Guid id) => _entities.Remove(id);
    public bool Contains(Guid id) => _entities.ContainsKey(id);
}

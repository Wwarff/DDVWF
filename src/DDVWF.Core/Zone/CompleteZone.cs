namespace DDVWF.Core.Zone;

public enum ZoneEntityKind
{
    WorldGeometry, StaticObject, AnimatedObject, Door, Light, Region, Npc, Spawn, ZonePoint
}

public readonly record struct EqPosition(float X, float Y, float Z, float Heading = 0);
public readonly record struct EqRotation(float X, float Y, float Z);

public abstract record ZoneEntityData;
public sealed record SpawnEntityData(long SpawnGroupId,int RespawnTime,int Variance,int PathGrid,int Version=0,bool PathWhenZoneIdle=false,int Condition=0,int ConditionValue=0,int Animation=0,int MinExpansion=-1,int MaxExpansion=-1,string ContentFlags="",string ContentFlagsDisabled="",string GroupName="",int SpawnLimit=0,float GroupDistance=0,float GroupMaxX=0,float GroupMinX=0,float GroupMaxY=0,float GroupMinY=0,int GroupDelay=0,int GroupMinDelay=0,int Despawn=0,int DespawnTimer=0,bool WaypointSpawns=false) : ZoneEntityData;
public sealed record NpcEntityData(long NpcTypeId,long SpawnGroupId,int Chance,int Race,int Gender,int Model=0,int Texture=0,int HelmTexture=0,int HeroForgeModel=0,int Face=0,int HairStyle=0,int HairColor=0,int EyeColor1=0,int EyeColor2=0,int BeardColor=0,int Beard=0,int DrakkinHeritage=0,int DrakkinTattoo=0,int DrakkinDetails=0,int ArmTexture=0,int BracerTexture=0,int HandTexture=0,int LegTexture=0,int FeetTexture=0,int PrimaryWeaponTexture=0,int SecondaryWeaponTexture=0,int Light=0,int SpawnConditionValueFilter=0,int SpawnMinTime=0,int SpawnMaxTime=0,int SpawnMinExpansion=-1,int SpawnMaxExpansion=-1,string SpawnContentFlags="",string SpawnContentFlagsDisabled="") : ZoneEntityData;
public sealed record ActorEntityData(float BoundingRadius=0,string? SoundName=null,int? VertexColorReference=null,string UserData="") : ZoneEntityData;
public sealed record ObjectEntityData(int Version,int ItemId,int Charges,int Type,int Icon,float SizePercentage,int Unknown24,int Unknown60,int Unknown64,int Unknown68,int Unknown72,int Unknown76,int Unknown84,float NativeSize,int SolidType,int Incline,float TiltX,float TiltY,string DisplayName,int MinExpansion=-1,int MaxExpansion=-1,string ContentFlags="",string ContentFlagsDisabled="") : ZoneEntityData;\npublic sealed record DoorEntityData(int DoorId,int OpenType,int Version=-1,int Lockpick=0,int KeyItem=0,int TriggerDoor=0,int TriggerType=0,bool DoorIsOpen=false,string DestZone="NONE",uint DestInstance=0,float DestX=0,float DestY=0,float DestZ=0,float DestHeading=0,int InvertState=0,int Incline=0) : ZoneEntityData;
public enum RegionSemantic { Normal, Water, Lava, Pvp, Zoneline, WaterBlockLos, FreezingWater, Slippery, Unknown }
public sealed record LightFrame(float Level,float Red,float Green,float Blue);
public sealed record LightEntityData(float Radius,uint Flags,uint CurrentFrame,uint Sleep,IReadOnlyList<LightFrame> Frames,bool GlobalAmbient=false,IReadOnlyList<int>? Regions=null) : ZoneEntityData;
public sealed record RegionEntityData(IReadOnlyList<RegionSemantic> Semantics,int? ZoneLineReference=null,int? TargetZoneIndex=null,int? TargetX=null,int? TargetY=null,int? TargetZ=null,int? TargetRotation=null) : ZoneEntityData;
public sealed record ZonePointEntityData(int Version,int Number,float TargetX,float TargetY,float TargetZ,float TargetHeading,uint TargetZoneId,uint TargetInstance,int ZoneInstance=0,float Buffer=0,uint ClientVersionMask=0xFFFFFFFF,int MinExpansion=-1,int MaxExpansion=-1,string ContentFlags="",string ContentFlagsDisabled="",bool IsVirtual=false,int Height=0,int Width=0) : ZoneEntityData;

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

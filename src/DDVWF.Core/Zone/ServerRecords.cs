namespace DDVWF.Core.Zone;

public sealed record GridRecord(int Id,uint ZoneId,int WanderType,int PauseType);
public sealed record GridEntryRecord(int GridId,uint ZoneId,int Number,float X,float Y,float Z,float Heading,int Pause,bool CenterPoint);
public sealed record Spawn2Record(long Id,long SpawnGroupId,string Zone,float X,float Y,float Z,float Heading,int RespawnTime,int Variance,int PathGrid,int Version=0,bool PathWhenZoneIdle=false,int Condition=0,int ConditionValue=0,int Animation=0,int MinExpansion=-1,int MaxExpansion=-1,string ContentFlags="",string ContentFlagsDisabled="");
public sealed record SpawnGroupRecord(long Id,string Name,int SpawnLimit,float Distance,float MaxX,float MinX,float MaxY,float MinY,int Delay,int MinDelay,int Despawn,int DespawnTimer,bool WaypointSpawns);
public sealed record SpawnEntryRecord(long SpawnGroupId,long NpcId,int Chance,int ConditionValueFilter=0,int MinTime=0,int MaxTime=0,int MinExpansion=-1,int MaxExpansion=-1,string ContentFlags="",string ContentFlagsDisabled="");
public sealed record NpcTypeRecord(long Id,string Name,int Race,int Gender,float Size,int Model=0,int Texture=0,int HelmTexture=0,int HeroForgeModel=0,int Face=0,int HairStyle=0,int HairColor=0,int EyeColor1=0,int EyeColor2=0,int BeardColor=0,int Beard=0,int DrakkinHeritage=0,int DrakkinTattoo=0,int DrakkinDetails=0,int ArmTexture=0,int BracerTexture=0,int HandTexture=0,int LegTexture=0,int FeetTexture=0,int PrimaryWeaponTexture=0,int SecondaryWeaponTexture=0,int Light=0);
public sealed record GroundSpawnRecord(long Id,uint ZoneId,int Version,float MaxX,float MaxY,float MaxZ,float MinX,float MinY,float Heading,string Name,int ItemId,int MaxAllowed,string Comment,int RespawnTimer,bool FixZ,int MinExpansion=-1,int MaxExpansion=-1,string ContentFlags="",string ContentFlagsDisabled="");
public sealed record ObjectContentRecord(uint ZoneId,long ParentId,int BagIndex,int ItemId,int Charges,DateTime? DropTime,int AugSlot1,int AugSlot2,int AugSlot3,int AugSlot4,int AugSlot5,int AugSlot6);
public sealed record ObjectRecord(long Id,uint ZoneId,int Version,float X,float Y,float Z,float Heading,int ItemId,int Charges,string ObjectName,int Type,int Icon,float SizePercentage,int Unknown24,int Unknown60,int Unknown64,int Unknown68,int Unknown72,int Unknown76,int Unknown84,float Size,int SolidType,int Incline,float TiltX,float TiltY,string DisplayName,int MinExpansion=-1,int MaxExpansion=-1,string ContentFlags="",string ContentFlagsDisabled="",float? BestZ=null);
public sealed record DoorRecord(long Id,int DoorId,string Zone,string Model,float X,float Y,float Z,float Heading,int OpenType,int Size,int Version=-1,int Lockpick=0,int KeyItem=0,int TriggerDoor=0,int TriggerType=0,bool DoorIsOpen=false,string DestZone="NONE",uint DestInstance=0,float DestX=0,float DestY=0,float DestZ=0,float DestHeading=0,int InvertState=0,int Incline=0,int Guild=0,bool NoKeyRing=false,bool DisableTimer=false,int DoorParam=-1,float Buffer=0,uint ClientVersionMask=0xFFFFFFFF,bool IsLdonDoor=false,int CloseTimerMs=5000,int DzSwitchId=0,int MinExpansion=-1,int MaxExpansion=-1,string ContentFlags="",string ContentFlagsDisabled="");
public sealed record ZoneRuntimeRecord(string MapFileName,float Underworld,float MaxZ,int Ruleset,int FindBestZHeightAdjust=1);
public sealed record ZonePointRecord(long Id,string Zone,int Version,int Number,float X,float Y,float Z,float Heading,float TargetX,float TargetY,float TargetZ,float TargetHeading,uint TargetZoneId,uint TargetInstance,int ZoneInstance=0,float Buffer=0,uint ClientVersionMask=0xFFFFFFFF,int MinExpansion=-1,int MaxExpansion=-1,string ContentFlags="",string ContentFlagsDisabled="",bool IsVirtual=false,int Height=0,int Width=0);

public sealed record ServerZoneSnapshot(
    IReadOnlyList<Spawn2Record> Spawn2,
    IReadOnlyList<SpawnEntryRecord> SpawnEntries,
    IReadOnlyList<NpcTypeRecord> NpcTypes,
    IReadOnlyList<DoorRecord> Doors,
    IReadOnlyList<ZonePointRecord>? ZonePoints = null,
    IReadOnlyList<SpawnGroupRecord>? SpawnGroups = null,
    IReadOnlyList<ObjectRecord>? Objects = null,
    IReadOnlyList<GroundSpawnRecord>? GroundSpawns = null,
    IReadOnlyList<GridRecord>? Grids = null,
    IReadOnlyList<GridEntryRecord>? GridEntries = null,
    IReadOnlyList<ObjectContentRecord>? ObjectContents = null,
    ZoneRuntimeRecord? Runtime = null);

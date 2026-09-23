namespace DDVWF.Core.Zone;

public sealed record Spawn2Record(long Id, long SpawnGroupId, string Zone, float X, float Y, float Z, float Heading, int RespawnTime, int Variance, int PathGrid);
public sealed record SpawnEntryRecord(long SpawnGroupId, long NpcId, int Chance);
public sealed record NpcTypeRecord(long Id,string Name,int Race,int Gender,float Size,int Model=0,int Texture=0,int HelmTexture=0,int HeroForgeModel=0,int Face=0,int HairStyle=0,int HairColor=0,int EyeColor1=0,int EyeColor2=0,int BeardColor=0,int Beard=0,int DrakkinHeritage=0,int DrakkinTattoo=0,int DrakkinDetails=0,int ArmTexture=0,int BracerTexture=0,int HandTexture=0,int LegTexture=0,int FeetTexture=0,int PrimaryWeaponTexture=0,int SecondaryWeaponTexture=0,int Light=0);
public sealed record DoorRecord(long Id, int DoorId, string Zone, string Model, float X, float Y, float Z, float Heading, int OpenType, int Size);
public sealed record ZonePointRecord(long Id,string Zone,int Version,int Number,float X,float Y,float Z,float Heading,float TargetX,float TargetY,float TargetZ,float TargetHeading,uint TargetZoneId,uint TargetInstance);

public sealed record ServerZoneSnapshot(
    IReadOnlyList<Spawn2Record> Spawn2,
    IReadOnlyList<SpawnEntryRecord> SpawnEntries,
    IReadOnlyList<NpcTypeRecord> NpcTypes,
    IReadOnlyList<DoorRecord> Doors,
    IReadOnlyList<ZonePointRecord>? ZonePoints = null);

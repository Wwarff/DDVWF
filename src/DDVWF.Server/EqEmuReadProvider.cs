using System.Data.Common;
using DDVWF.Core.Zone;

namespace DDVWF.Server;

public sealed record EqEmuConnectionOptions(string Host, int Port, string Database, string User);

public sealed class EqEmuReadProvider : IServerDataProvider, IServerReadDiagnosticsProvider
{
    private readonly Func<CancellationToken,Task<DbConnection>> _open;private readonly int _zoneVersion;
    public bool CanWrite => false;
    public ServerReadDiagnostics? LastReadDiagnostics { get; private set; }

    public EqEmuReadProvider(Func<CancellationToken,Task<DbConnection>> open,int zoneVersion=0) => (_open,_zoneVersion)=(open,zoneVersion);

    public async Task PopulateAsync(CompleteZone zone,CancellationToken cancellationToken)
    {
        await using var connection=await _open(cancellationToken);
        var spawn2=new List<Spawn2Record>(); var entries=new List<SpawnEntryRecord>();
        var npcs=new Dictionary<long,NpcTypeRecord>(); var doors=new List<DoorRecord>(); var zonePoints=new List<ZonePointRecord>(); var spawnGroups=new List<SpawnGroupRecord>(); var objects=new List<ObjectRecord>(); var groundSpawns=new List<GroundSpawnRecord>(); var grids=new List<GridRecord>(); var gridEntries=new List<GridEntryRecord>(); var objectContents=new List<ObjectContentRecord>();

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT id,spawngroupID,zone,x,y,z,heading,respawntime,variance,pathgrid,version,path_when_zone_idle,_condition,cond_value,animation,min_expansion,max_expansion,content_flags,content_flags_disabled FROM spawn2 WHERE zone = @zone AND (version = @version OR version = -1)";
            Add(cmd,"@zone",zone.ShortName);Add(cmd,"@version",_zoneVersion);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))
                spawn2.Add(new(r.GetInt64(0),r.GetInt64(1),r.GetString(2),F(r,3),F(r,4),F(r,5),F(r,6),I(r,7),I(r,8),I(r,9),I(r,10),I(r,11)!=0,I(r,12),I(r,13),I(r,14),I(r,15),I(r,16),r.IsDBNull(17)?"":r.GetString(17),r.IsDBNull(18)?"":r.GetString(18)));
        }

        var groups=spawn2.Select(x=>x.SpawnGroupId).Distinct().ToArray();
        foreach(var group in groups)
        {
            await using var groupCmd=connection.CreateCommand();
            groupCmd.CommandText="SELECT id,name,spawn_limit,dist,max_x,min_x,max_y,min_y,delay,mindelay,despawn,despawn_timer,wp_spawns FROM spawngroup WHERE id = @group";
            Add(groupCmd,"@group",group);
            await using var gr=await groupCmd.ExecuteReaderAsync(cancellationToken);
            if(await gr.ReadAsync(cancellationToken))spawnGroups.Add(new(gr.GetInt64(0),gr.IsDBNull(1)?"":gr.GetString(1),I(gr,2),F(gr,3),F(gr,4),F(gr,5),F(gr,6),F(gr,7),I(gr,8),I(gr,9),I(gr,10),I(gr,11),I(gr,12)!=0));
        }

        foreach(var group in groups)
        {
            await using var cmd=connection.CreateCommand();
            cmd.CommandText="SELECT spawngroupID,npcID,chance,condition_value_filter,min_time,max_time,min_expansion,max_expansion,content_flags,content_flags_disabled FROM spawnentry WHERE spawngroupID = @group";
            Add(cmd,"@group",group);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken)) entries.Add(new(r.GetInt64(0),r.GetInt64(1),I(r,2),I(r,3),I(r,4),I(r,5),I(r,6),I(r,7),r.IsDBNull(8)?"":r.GetString(8),r.IsDBNull(9)?"":r.GetString(9)));
        }

        foreach(var id in entries.Select(x=>x.NpcId).Distinct())
        {
            await using var cmd=connection.CreateCommand();
            cmd.CommandText="SELECT id,name,race,gender,size,model,texture,helmtexture,herosforgemodel,face,luclin_hairstyle,luclin_haircolor,luclin_eyecolor,luclin_eyecolor2,luclin_beardcolor,luclin_beard,drakkin_heritage,drakkin_tattoo,drakkin_details,armtexture,bracertexture,handtexture,legtexture,feettexture,d_melee_texture1,d_melee_texture2,light FROM npc_types WHERE id = @id";
            Add(cmd,"@id",id);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            if(await r.ReadAsync(cancellationToken)) npcs[id]=new(r.GetInt64(0),r.GetString(1),r.GetInt32(2),r.GetInt32(3),F(r,4),I(r,5),I(r,6),I(r,7),I(r,8),I(r,9),I(r,10),I(r,11),I(r,12),I(r,13),I(r,14),I(r,15),I(r,16),I(r,17),I(r,18),I(r,19),I(r,20),I(r,21),I(r,22),I(r,23),I(r,24),I(r,25),I(r,26));
        }

        uint zoneId;
        await using(var zoneCmd=connection.CreateCommand())
        {
            zoneCmd.CommandText="SELECT zoneidnumber FROM zone WHERE short_name = @zone ORDER BY version LIMIT 1";
            Add(zoneCmd,"@zone",zone.ShortName);
            var value=await zoneCmd.ExecuteScalarAsync(cancellationToken);
            if(value is null||value is DBNull)throw new InvalidDataException($"EQEmu zone row was not found for '{zone.ShortName}'.");
            zoneId=Convert.ToUInt32(value,System.Globalization.CultureInfo.InvariantCulture);
        }

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT id,zoneid,type,type2 FROM grid WHERE zoneid=@zoneid";
            Add(cmd,"@zoneid",zoneId);await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))grids.Add(new(I(r,0),Convert.ToUInt32(r.GetValue(1),System.Globalization.CultureInfo.InvariantCulture),I(r,2),I(r,3)));
        }
        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT gridid,zoneid,number,x,y,z,heading,pause,centerpoint FROM grid_entries WHERE zoneid=@zoneid";
            Add(cmd,"@zoneid",zoneId);await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))gridEntries.Add(new(I(r,0),Convert.ToUInt32(r.GetValue(1),System.Globalization.CultureInfo.InvariantCulture),I(r,2),F(r,3),F(r,4),F(r,5),F(r,6),I(r,7),I(r,8)!=0));
        }

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT id,zoneid,version,max_x,max_y,max_z,min_x,min_y,heading,name,item,max_allowed,comment,respawn_timer,fix_z,min_expansion,max_expansion,content_flags,content_flags_disabled FROM ground_spawns WHERE zoneid = @zoneid AND (version = @version OR version = -1)";
            Add(cmd,"@zoneid",zoneId);Add(cmd,"@version",_zoneVersion);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))
                groundSpawns.Add(new(r.GetInt64(0),Convert.ToUInt32(r.GetValue(1),System.Globalization.CultureInfo.InvariantCulture),I(r,2),F(r,3),F(r,4),F(r,5),F(r,6),F(r,7),F(r,8),r.IsDBNull(9)?"":r.GetString(9),I(r,10),I(r,11),r.IsDBNull(12)?"":r.GetString(12),I(r,13),I(r,14)!=0,I(r,15),I(r,16),r.IsDBNull(17)?"":r.GetString(17),r.IsDBNull(18)?"":r.GetString(18)));
        }

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT id,zoneid,version,xpos,ypos,zpos,heading,itemid,charges,objectname,type,icon,size_percentage,unknown24,unknown60,unknown64,unknown68,unknown72,unknown76,unknown84,size,solid_type,incline,tilt_x,tilt_y,display_name,min_expansion,max_expansion,content_flags,content_flags_disabled FROM object WHERE zoneid = @zoneid AND (version = @version OR version = -1)";
            Add(cmd,"@zoneid",zoneId);Add(cmd,"@version",_zoneVersion);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))
                objects.Add(new(r.GetInt64(0),Convert.ToUInt32(r.GetValue(1),System.Globalization.CultureInfo.InvariantCulture),I(r,2),F(r,3),F(r,4),F(r,5),F(r,6),I(r,7),I(r,8),r.IsDBNull(9)?"":r.GetString(9),I(r,10),I(r,11),F(r,12),I(r,13),I(r,14),I(r,15),I(r,16),I(r,17),I(r,18),I(r,19),F(r,20),I(r,21),I(r,22),F(r,23),F(r,24),r.IsDBNull(25)?"":r.GetString(25),I(r,26),I(r,27),r.IsDBNull(28)?"":r.GetString(28),r.IsDBNull(29)?"":r.GetString(29)));
        }

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT zoneid,parentid,bagidx,itemid,charges,droptime,augslot1,augslot2,augslot3,augslot4,augslot5,augslot6 FROM object_contents WHERE zoneid = @zoneid";
            Add(cmd,"@zoneid",zoneId);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))
                objectContents.Add(new(Convert.ToUInt32(r.GetValue(0),System.Globalization.CultureInfo.InvariantCulture),Convert.ToInt64(r.GetValue(1),System.Globalization.CultureInfo.InvariantCulture),I(r,2),I(r,3),I(r,4),r.IsDBNull(5)?null:Convert.ToDateTime(r.GetValue(5),System.Globalization.CultureInfo.InvariantCulture),I(r,6),I(r,7),I(r,8),I(r,9),I(r,10),I(r,11)));
        }

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT id,doorid,zone,name,pos_x,pos_y,pos_z,heading,opentype,size,version,lockpick,keyitem,triggerdoor,triggertype,doorisopen,dest_zone,dest_instance,dest_x,dest_y,dest_z,dest_heading,invert_state,incline FROM doors WHERE zone = @zone AND (version = @version OR version = -1)";
            Add(cmd,"@zone",zone.ShortName);Add(cmd,"@version",_zoneVersion);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))
                doors.Add(new(r.GetInt64(0),I(r,1),r.GetString(2),r.GetString(3),F(r,4),F(r,5),F(r,6),F(r,7),I(r,8),I(r,9),I(r,10),I(r,11),I(r,12),I(r,13),I(r,14),I(r,15)!=0,r.IsDBNull(16)?"NONE":r.GetString(16),r.IsDBNull(17)?0:Convert.ToUInt32(r.GetValue(17),System.Globalization.CultureInfo.InvariantCulture),F(r,18),F(r,19),F(r,20),F(r,21),I(r,22),I(r,23)));
        }

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT id,zone,version,number,x,y,z,heading,target_x,target_y,target_z,target_heading,target_zone_id,target_instance,zoneinst,buffer,client_version_mask,min_expansion,max_expansion,content_flags,content_flags_disabled,is_virtual,height,width FROM zone_points WHERE zone = @zone AND (version = @version OR version = -1)";
            Add(cmd,"@zone",zone.ShortName);Add(cmd,"@version",_zoneVersion);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))
                zonePoints.Add(new(r.GetInt64(0),r.GetString(1),I(r,2),I(r,3),F(r,4),F(r,5),F(r,6),F(r,7),F(r,8),F(r,9),F(r,10),F(r,11),Convert.ToUInt32(r.GetValue(12),System.Globalization.CultureInfo.InvariantCulture),Convert.ToUInt32(r.GetValue(13),System.Globalization.CultureInfo.InvariantCulture),I(r,14),F(r,15),r.IsDBNull(16)?0xFFFFFFFF:Convert.ToUInt32(r.GetValue(16),System.Globalization.CultureInfo.InvariantCulture),I(r,17),I(r,18),r.IsDBNull(19)?"":r.GetString(19),r.IsDBNull(20)?"":r.GetString(20),I(r,21)!=0,I(r,22),I(r,23)));
        }

        LastReadDiagnostics=new(spawn2.Count,entries.Count,npcs.Count,spawnGroups.Count,doors.Count,zonePoints.Count,objects.Count,groundSpawns.Count,grids.Count,gridEntries.Count,objectContents.Count);
        ServerZoneJoiner.Join(zone,new ServerZoneSnapshot(spawn2,entries,npcs.Values.ToArray(),doors,zonePoints,spawnGroups,objects,groundSpawns,grids,gridEntries,objectContents));
    }

    private static float F(DbDataReader r,int i)=>Convert.ToSingle(r.GetValue(i),System.Globalization.CultureInfo.InvariantCulture);
    private static int I(DbDataReader r,int i)=>r.IsDBNull(i)?0:Convert.ToInt32(r.GetValue(i),System.Globalization.CultureInfo.InvariantCulture);
    private static void Add(DbCommand command,string name,object value){var p=command.CreateParameter();p.ParameterName=name;p.Value=value;command.Parameters.Add(p);}
}

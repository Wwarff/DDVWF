using System.Data.Common;
using DDVWF.Core.Zone;

namespace DDVWF.Server;

public sealed record EqEmuConnectionOptions(string Host, int Port, string Database, string User);

public sealed class EqEmuReadProvider : IServerDataProvider
{
    private readonly Func<CancellationToken,Task<DbConnection>> _open;
    public bool CanWrite => false;

    public EqEmuReadProvider(Func<CancellationToken,Task<DbConnection>> open) => _open=open;

    public async Task PopulateAsync(CompleteZone zone,CancellationToken cancellationToken)
    {
        await using var connection=await _open(cancellationToken);
        var spawn2=new List<Spawn2Record>(); var entries=new List<SpawnEntryRecord>();
        var npcs=new Dictionary<long,NpcTypeRecord>(); var doors=new List<DoorRecord>(); var zonePoints=new List<ZonePointRecord>();

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT id, spawngroupID, zone, x, y, z, heading, respawntime, variance, pathgrid FROM spawn2 WHERE zone = @zone";
            Add(cmd,"@zone",zone.ShortName);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))
                spawn2.Add(new(r.GetInt64(0),r.GetInt64(1),r.GetString(2),F(r,3),F(r,4),F(r,5),F(r,6),r.GetInt32(7),r.GetInt32(8),r.GetInt32(9)));
        }

        var groups=spawn2.Select(x=>x.SpawnGroupId).Distinct().ToArray();
        foreach(var group in groups)
        {
            await using var cmd=connection.CreateCommand();
            cmd.CommandText="SELECT spawngroupID, npcID, chance FROM spawnentry WHERE spawngroupID = @group";
            Add(cmd,"@group",group);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken)) entries.Add(new(r.GetInt64(0),r.GetInt64(1),r.GetInt32(2)));
        }

        foreach(var id in entries.Select(x=>x.NpcId).Distinct())
        {
            await using var cmd=connection.CreateCommand();
            cmd.CommandText="SELECT id, name, race, gender, size FROM npc_types WHERE id = @id";
            Add(cmd,"@id",id);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            if(await r.ReadAsync(cancellationToken)) npcs[id]=new(r.GetInt64(0),r.GetString(1),r.GetInt32(2),r.GetInt32(3),F(r,4));
        }

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT id, doorid, zone, name, pos_x, pos_y, pos_z, heading, opentype, size FROM doors WHERE zone = @zone";
            Add(cmd,"@zone",zone.ShortName);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))
                doors.Add(new(r.GetInt64(0),r.GetInt32(1),r.GetString(2),r.GetString(3),F(r,4),F(r,5),F(r,6),F(r,7),r.GetInt32(8),r.GetInt32(9)));
        }

        await using(var cmd=connection.CreateCommand())
        {
            cmd.CommandText="SELECT id, zone, version, number, x, y, z, heading, target_x, target_y, target_z, target_heading, target_zone_id, target_instance FROM zone_points WHERE zone = @zone";
            Add(cmd,"@zone",zone.ShortName);
            await using var r=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await r.ReadAsync(cancellationToken))
                zonePoints.Add(new(r.GetInt64(0),r.GetString(1),r.GetInt32(2),r.GetInt32(3),F(r,4),F(r,5),F(r,6),F(r,7),F(r,8),F(r,9),F(r,10),F(r,11),Convert.ToUInt32(r.GetValue(12),System.Globalization.CultureInfo.InvariantCulture),Convert.ToUInt32(r.GetValue(13),System.Globalization.CultureInfo.InvariantCulture)));
        }

        ServerZoneJoiner.Join(zone,new ServerZoneSnapshot(spawn2,entries,npcs.Values.ToArray(),doors,zonePoints));
    }

    private static float F(DbDataReader r,int i)=>Convert.ToSingle(r.GetValue(i),System.Globalization.CultureInfo.InvariantCulture);
    private static void Add(DbCommand command,string name,object value){var p=command.CreateParameter();p.ParameterName=name;p.Value=value;command.Parameters.Add(p);}
}

using System.Data.Common;using MySqlConnector;
namespace DDVWF.Server;
public sealed record EqEmuMySqlSettings(string Host,int Port,string Database,string User,string Password);
public static class EqEmuMySqlConnection
{
 public static async Task<DbConnection> OpenAsync(EqEmuMySqlSettings s,CancellationToken ct=default){var cs=new MySqlConnectionStringBuilder{Server=s.Host,Port=(uint)s.Port,Database=s.Database,UserID=s.User,Password=s.Password,Pooling=true,ConnectionTimeout=5};var c=new MySqlConnection(cs.ConnectionString);await c.OpenAsync(ct);return c;}
 public static async Task<string> ProbeAsync(EqEmuMySqlSettings s,CancellationToken ct=default){await using var c=await OpenAsync(s,ct);await using var cmd=c.CreateCommand();cmd.CommandText="SELECT DATABASE(), VERSION()";await using var r=await cmd.ExecuteReaderAsync(ct);if(!await r.ReadAsync(ct))throw new InvalidDataException("EQEmu SQL probe returned no row.");return $"{r.GetString(0)} | MySQL {r.GetString(1)}";}
 public static EqEmuReadProvider CreateReadProvider(EqEmuMySqlSettings s)=>new(ct=>OpenAsync(s,ct));
}

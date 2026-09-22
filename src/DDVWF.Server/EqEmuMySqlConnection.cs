using System.Data.Common;using MySqlConnector;
namespace DDVWF.Server;
public sealed record EqEmuMySqlSettings(string Host,int Port,string Database,string User,string Password);
public static class EqEmuMySqlConnection
{
 public static async Task<DbConnection> OpenAsync(EqEmuMySqlSettings s,CancellationToken ct=default){var cs=new MySqlConnectionStringBuilder{Server=s.Host,Port=(uint)s.Port,Database=s.Database,UserID=s.User,Password=s.Password,Pooling=true,ConnectionTimeout=5};var c=new MySqlConnection(cs.ConnectionString);await c.OpenAsync(ct);return c;}
 public static EqEmuReadProvider CreateReadProvider(EqEmuMySqlSettings s)=>new(ct=>OpenAsync(s,ct));
}

using System.Text.Json;
namespace DDVWF.Server;
public static class EqEmuServerPaths
{
 public static string ResolveMapsPath(string serverRoot)
 {
  if(string.IsNullOrWhiteSpace(serverRoot))throw new InvalidDataException("EQEmu server root is empty.");
  var root=Path.GetFullPath(serverRoot);
  var configured="Maps/";
  var config=Path.Combine(root,"eqemu_config.json");
  if(File.Exists(config))
  {
   using var doc=JsonDocument.Parse(File.ReadAllText(config));
   if(doc.RootElement.TryGetProperty("server",out var server)&&server.TryGetProperty("directories",out var dirs)&&dirs.TryGetProperty("maps",out var maps)&&maps.ValueKind==JsonValueKind.String&&!string.IsNullOrWhiteSpace(maps.GetString()))configured=maps.GetString()!;
  }
  string Candidate(string p)=>Path.IsPathRooted(p)?Path.GetFullPath(p):Path.GetFullPath(Path.Combine(root,p));
  var primary=Candidate(configured);if(Directory.Exists(primary))return primary;
  foreach(var fallback in new[]{"maps","Maps"}){var p=Path.Combine(root,fallback);if(Directory.Exists(p))return Path.GetFullPath(p);}
  return primary;
 }
 public static string ResolveBaseMap(string serverRoot,string mapName)
 {
  if(string.IsNullOrWhiteSpace(mapName))throw new InvalidDataException("EQEmu map name is empty.");
  return Path.Combine(ResolveMapsPath(serverRoot),"base",mapName.ToLowerInvariant()+".map");
 }
}

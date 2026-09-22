using System.Text.Json;using DDVWF.Core.Zone;
namespace DDVWF.Renderer.Babylon;
public static class BabylonSceneSerializer
{
 public static string Serialize(CompleteZone zone,IReadOnlyDictionary<string,RenderMesh> assets)
 {
  var meshes=zone.Entities.Where(e=>e.ClientAsset is not null&&assets.ContainsKey(e.ClientAsset)).Select(e=>new{e.Id,e.Name,Kind=e.Kind.ToString(),e.Position,e.Scale,Asset=assets[e.ClientAsset!]}).ToArray();
  return JsonSerializer.Serialize(new{zone=zone.ShortName,meshes},new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
 }
}

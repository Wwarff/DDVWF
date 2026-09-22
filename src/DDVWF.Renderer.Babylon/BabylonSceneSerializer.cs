using System.Text.Json;using DDVWF.Core.Zone;
namespace DDVWF.Renderer.Babylon;
public static class BabylonSceneSerializer
{
 public static string Serialize(CompleteZone zone,IReadOnlyDictionary<string,RenderMesh> assets,IReadOnlyDictionary<string,byte[]>? textures=null)
 {
  var meshes=zone.Entities.Where(e=>e.ClientAsset is not null&&assets.ContainsKey(e.ClientAsset)).Select(e=>new{e.Id,e.Name,Kind=e.Kind.ToString(),e.Position,e.Scale,Asset=assets[e.ClientAsset!]}).ToArray();
  var markers=zone.Entities.Where(e=>e.ServerId is not null&&(e.ClientAsset is null||!assets.ContainsKey(e.ClientAsset))).Select(e=>new{e.Id,e.Name,Kind=e.Kind.ToString(),e.Position,e.Scale,e.ServerId}).ToArray();
  var textureData=(textures??new Dictionary<string,byte[]>()).Where(x=>meshes.Any(m=>m.Asset.Primitives.Any(p=>string.Equals(p.TextureName,x.Key,StringComparison.OrdinalIgnoreCase)||(p.Material?.TextureFrames?.Any(t=>string.Equals(t,x.Key,StringComparison.OrdinalIgnoreCase))??false)))).ToDictionary(x=>x.Key,x=>Convert.ToBase64String(x.Value),StringComparer.OrdinalIgnoreCase);
  return JsonSerializer.Serialize(new{zone=zone.ShortName,meshes,markers,textures=textureData},new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
 }
}
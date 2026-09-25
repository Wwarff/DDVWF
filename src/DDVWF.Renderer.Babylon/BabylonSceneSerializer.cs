using System.Text.Json;using DDVWF.Core.Zone;
namespace DDVWF.Renderer.Babylon;
public static class BabylonSceneSerializer
{
 public static string Serialize(CompleteZone zone,IReadOnlyDictionary<string,RenderMesh> assets,IReadOnlyDictionary<string,byte[]>? textures=null,bool shellOnly=false)
 {
  var renderEntities=shellOnly?zone.Entities.Where(e=>e.Kind==ZoneEntityKind.WorldGeometry):zone.Entities;
  var meshes=renderEntities.Where(e=>e.ClientAsset is not null&&assets.ContainsKey(e.ClientAsset)).Select(e=>new{e.Id,e.Name,Kind=e.Kind.ToString(),e.Position,e.Rotation,e.Scale,e.Data,Asset=assets[e.ClientAsset!]}).ToArray();
  var lights=zone.Entities.Where(e=>e.Kind==ZoneEntityKind.Light&&e.Data is LightEntityData).Select(e=>new{e.Id,e.Name,e.Position,Data=(LightEntityData)e.Data!}).ToArray();
  var markers=(shellOnly?Enumerable.Empty<ZoneEntity>():zone.Entities).Where(e=>e.Kind is ZoneEntityKind.Door or ZoneEntityKind.ZonePoint or ZoneEntityKind.Npc or ZoneEntityKind.GroundSpawn || e.Data is ObjectEntityData).Where(e=>e.Data is not ObjectEntityData).Where(e=>e.ClientAsset is null||!assets.ContainsKey(e.ClientAsset)).Select(e=>new{e.Id,e.Name,Kind=e.Kind.ToString(),e.Position,e.Scale,e.ServerId,Data=e.Data}).ToArray();
  var groundSpawnAreas=(shellOnly?Enumerable.Empty<ZoneEntity>():zone.Entities).Where(e=>e.Kind==ZoneEntityKind.GroundSpawn&&e.Data is GroundSpawnEntityData).Select(e=>new{e.Id,e.Name,e.Position,Data=(GroundSpawnEntityData)e.Data!}).ToArray();
  var textureData=(textures??new Dictionary<string,byte[]>()).Where(x=>meshes.Any(m=>m.Asset.Primitives.Any(p=>string.Equals(p.TextureName,x.Key,StringComparison.OrdinalIgnoreCase)||(p.Material?.TextureFrames?.Any(t=>string.Equals(t,x.Key,StringComparison.OrdinalIgnoreCase))??false)||string.Equals(p.Material?.NormalTextureName,x.Key,StringComparison.OrdinalIgnoreCase)))).ToDictionary(x=>x.Key,x=>Convert.ToBase64String(x.Value),StringComparer.OrdinalIgnoreCase);
  return JsonSerializer.Serialize(new{zone=zone.ShortName,meshes,markers,lights,groundSpawnAreas,textures=textureData},new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
 }
}
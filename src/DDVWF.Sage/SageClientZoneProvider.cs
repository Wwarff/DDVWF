using System.Security.Cryptography;using System.Text;using DDVWF.Core.Zone;using DDVWF.Sage.Pfs;using DDVWF.Sage.Wld;
namespace DDVWF.Sage;
public sealed class SageClientZoneProvider:IClientRenderAssetProvider
{
 readonly Dictionary<string,RenderMesh> _renderMeshes=new(StringComparer.OrdinalIgnoreCase);
 public IReadOnlyDictionary<string,RenderMesh> RenderMeshes=>_renderMeshes;

 public async Task PopulateAsync(CompleteZone zone,string eqRoot,CancellationToken ct)
 {
  if(string.IsNullOrWhiteSpace(eqRoot)||!Directory.Exists(eqRoot))throw new DirectoryNotFoundException(eqRoot);
  var candidates=new[]{Path.Combine(eqRoot,zone.ShortName+".s3d"),Path.Combine(eqRoot,zone.ShortName+"_obj.s3d"),Path.Combine(eqRoot,zone.ShortName+"_obj2.s3d")};
  var archives=candidates.Where(File.Exists).ToArray();if(archives.Length==0)throw new FileNotFoundException($"No native S3D archive was found for zone '{zone.ShortName}' in the configured EQ root.");
  foreach(var archivePath in archives){ct.ThrowIfCancellationRequested();var bytes=await File.ReadAllBytesAsync(archivePath,ct);var archive=PfsArchive.Open(bytes);
   foreach(var pair in archive.Files.Where(x=>x.Key.EndsWith(".wld",StringComparison.OrdinalIgnoreCase))){var doc=WldDocument.Parse(pair.Value,pair.Key);
    if(doc.Kind==WldKind.Zone){var scene=SageSceneBuilder.Build(doc,pair.Value);foreach(var sm in scene.Meshes){var rp=sm.Primitives.Select(p=>new RenderPrimitive(p.Name,p.Vertices.Select(v=>new RenderVertex(v.Position,v.Normal,v.Uv)).ToArray(),p.Indices,scene.Materials[p.MaterialIndex].Frames.FirstOrDefault()?.FileName,p.PassThrough)).ToArray();_renderMeshes[$"{pair.Key}#{sm.Name}"]=new(sm.Name,rp,scene.FlipRootX);}var ordinal=0;foreach(var mesh in scene.Meshes.Where(x=>x.Primitives.Count>0)){zone.Add(new(Stable(zone.ShortName,pair.Key,"world",mesh.Name,ordinal++),ZoneEntityKind.WorldGeometry,mesh.Name,new(0,0,0),1,null,$"{pair.Key}#{mesh.Name}"));}}
    if(doc.Kind==WldKind.ZoneObjects){var ordinal=0;foreach(var fragment in doc.Fragments.Where(x=>x.KnownType==WldFragmentType.ActorInstance)){var actor=WldActorReader.ReadActorInstance(doc,fragment,pair.Value);if(actor.Location is not{} loc)continue;zone.Add(new(Stable(zone.ShortName,pair.Key,"actor",actor.ObjectName,ordinal++),ZoneEntityKind.StaticObject,actor.ObjectName,new(loc.X,loc.Y,loc.Z,loc.RotateZ),actor.ScaleFactor==0?1:actor.ScaleFactor,null,actor.ObjectName));}}
   }
  }
 }
 public static Guid Stable(params object[] parts){var text=string.Join("|",parts.Select(x=>x?.ToString()??""));var hash=MD5.HashData(Encoding.UTF8.GetBytes(text));return new Guid(hash);}
}

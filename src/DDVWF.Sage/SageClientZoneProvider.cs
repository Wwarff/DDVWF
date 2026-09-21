using DDVWF.Core.Zone;
using DDVWF.Sage.Pfs;
using DDVWF.Sage.Wld;

namespace DDVWF.Sage;

public sealed class SageClientZoneProvider : IClientZoneProvider
{
    public async Task PopulateAsync(CompleteZone zone,string eqRoot,CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(eqRoot) || !Directory.Exists(eqRoot)) throw new DirectoryNotFoundException(eqRoot);
        var candidates=new[]{Path.Combine(eqRoot,zone.ShortName+".s3d"),Path.Combine(eqRoot,zone.ShortName+"_obj.s3d"),Path.Combine(eqRoot,zone.ShortName+"_obj2.s3d")};
        var archives=candidates.Where(File.Exists).ToArray();
        if(archives.Length==0) throw new FileNotFoundException($"No native S3D archive was found for zone '{zone.ShortName}' in the configured EQ root.");

        foreach(var archivePath in archives)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes=await File.ReadAllBytesAsync(archivePath,cancellationToken);
            var archive=PfsArchive.Open(bytes);
            foreach(var pair in archive.Files.Where(x=>x.Key.EndsWith(".wld",StringComparison.OrdinalIgnoreCase)))
            {
                var document=WldDocument.Parse(pair.Value,pair.Key);
                if(document.Kind==WldKind.ZoneObjects)
                {
                    foreach(var fragment in document.Fragments.Where(x=>x.KnownType==WldFragmentType.ActorInstance))
                    {
                        var actor=WldActorReader.ReadActorInstance(document,fragment,pair.Value);
                        if(actor.Location is not { } loc) continue;
                        zone.Add(new ZoneEntity(Guid.NewGuid(),ZoneEntityKind.StaticObject,actor.ObjectName,
                            new EqPosition(loc.X,loc.Y,loc.Z,loc.RotateZ),actor.ScaleFactor==0?1:actor.ScaleFactor,null,actor.ObjectName));
                    }
                }
            }
        }
    }
}

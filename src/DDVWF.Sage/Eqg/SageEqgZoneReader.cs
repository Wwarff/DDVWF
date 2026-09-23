using System.Buffers.Binary;
using System.Text;

namespace DDVWF.Sage.Eqg;

// Sage v1.8.15 src/lib/eqg/zone/zone.js v1-v3 zone semantics.
public sealed record SageEqgPlaceable(string ModelName,string ModelFile,float X,float Y,float Z,float RotateX,float RotateY,float RotateZ,float Scale);
public sealed record SageEqgRegion(string Name,float X,float Y,float Z,float RotateZ,uint Flag1,uint Flag2,float ExtX,float ExtY,float ExtZ);
public sealed record SageEqgLight(string Name,float X,float Y,float Z,float Red,float Green,float Blue,float Radius);
public sealed record SageEqgZone(IReadOnlyList<SageEqgPlaceable> Placeables,IReadOnlyList<SageEqgRegion> Regions,IReadOnlyList<SageEqgLight> Lights,float ZoneRotation=0,float ZoneOffsetX=0,float ZoneOffsetY=0,float ZoneOffsetZ=0);

public static class SageEqgZoneReader
{
 public static SageEqgZone Read(string name,ReadOnlySpan<byte> source)
 {
  var b=source.ToArray();var p=0;void Need(int n){if(n<0||p+n>b.Length)throw new InvalidDataException($"EQG zone '{name}' is truncated.");}
  uint U32(){Need(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}int I32()=>unchecked((int)U32());float F32()=>BitConverter.Int32BitsToSingle(I32());
  string Str(int at){if(at<0||at>=b.Length)throw new InvalidDataException($"EQG zone '{name}' string offset is invalid.");var e=at;while(e<b.Length&&b[e]!=0)e++;return Encoding.UTF8.GetString(b,at,e-at);}
  Need(4);var magic=Encoding.ASCII.GetString(b,p,4);p+=4;if(magic=="EQTZ")throw new NotSupportedException($"EQG zone '{name}' uses Sage ZoneV4 text/data terrain semantics, which require the V4 terrain decoder before production use.");
  var version=U32();var listLength=checked((int)U32());var modelCount=checked((int)U32());var objectCount=checked((int)U32());var regionCount=checked((int)U32());var lightCount=checked((int)U32());var list=p;Need(listLength);p+=listLength;
  var modelNames=new List<string>(modelCount);for(var i=0;i<modelCount;i++){var off=checked((int)U32());modelNames.Add(Str(checked(list+off)).Replace(")","_"));}
  var objects=new List<SageEqgPlaceable>(objectCount);const float radToDeg=180f/MathF.PI;float zoneRotation=0,zoneOffsetX=0,zoneOffsetY=0,zoneOffsetZ=0;
  for(var i=0;i<objectCount;i++){var id=I32();var loc=checked((int)U32());var x=F32();var y=F32();var z=F32();var rx=F32();var ry=F32();var rz=F32();var scale=F32();var modelName=Str(checked(list+loc));var modelFile=id>=0&&id<modelNames.Count?modelNames[id]:"";if(modelFile.Contains(".ter",StringComparison.OrdinalIgnoreCase)){zoneRotation=rx*radToDeg;zoneOffsetX=x;zoneOffsetY=y;zoneOffsetZ=z;}if(version>1){var n=checked((int)U32());for(var j=0;j<n;j++)_=U32();}objects.Add(new(modelName,modelFile,x,y,z,rx*radToDeg,ry*radToDeg,rz*radToDeg,scale));}
  var regions=new List<SageEqgRegion>(regionCount);for(var i=0;i<regionCount;i++){var loc=checked((int)U32());var x=F32();var y=F32();var z=F32();var rot=F32();var f1=U32();var f2=U32();var ex=F32();var ey=F32();var ez=F32();regions.Add(new(Str(checked(list+loc)),x,y,z,rot/512f*360f,f1,f2,ex,ey,ez));}
  var lights=new List<SageEqgLight>(lightCount);for(var i=0;i<lightCount;i++){var loc=checked((int)U32());lights.Add(new(Str(checked(list+loc)),F32(),F32(),F32(),F32(),F32(),F32(),F32()));}
  return new(objects,regions,lights,zoneRotation,zoneOffsetX,zoneOffsetY,zoneOffsetZ);
 }
}

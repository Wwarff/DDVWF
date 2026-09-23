using System.Buffers.Binary;
using System.Numerics;

namespace DDVWF.Sage.Eqg;

// Direct parser for EQ Sage v1.8.15 src/lib/eqg/model/model.js.
// Keeps Sage's coordinate transforms so EQG object models enter the same canonical render path as S3D assets.
public sealed record SageEqgMaterialProperty(string Name,uint Type,float FloatValue,uint IntValue,string StringValue);
public sealed record SageEqgMaterial(string Name,string Shader,IReadOnlyList<SageEqgMaterialProperty> Properties);
public sealed record SageEqgWeight(int Bone,float Weight);
public sealed record SageEqgBone(string Name,int Next,uint ChildrenCount,int ChildrenIndex,Vector3 Translation,System.Numerics.Quaternion Rotation,Vector3 Scale);
public sealed record SageEqgVertex(Vector3 Position,Vector3 Normal,Vector2 Uv,uint Color,IReadOnlyList<SageEqgWeight>? Weights=null);
public sealed record SageEqgPolygon(uint A,uint B,uint C,int Material,uint Flags);
public sealed record SageEqgModel(string Name,IReadOnlyList<SageEqgMaterial> Materials,IReadOnlyList<SageEqgVertex> Vertices,IReadOnlyList<SageEqgPolygon> Polygons,IReadOnlyList<SageEqgBone> Bones);

public static class SageEqgModelReader
{
 public static SageEqgModel Read(string name,ReadOnlySpan<byte> source)
 {
  var b=source.ToArray();var p=0;
  void Need(int n){if(n<0||p+n>b.Length)throw new InvalidDataException($"EQG model '{name}' is truncated.");}
  uint U32(){Need(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}
  int I32(){Need(4);var v=BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}
  float F32()=>BitConverter.Int32BitsToSingle(I32());
  string Str(int at){if(at<0||at>=b.Length)throw new InvalidDataException($"EQG model '{name}' string offset is invalid.");var e=at;while(e<b.Length&&b[e]!=0)e++;return System.Text.Encoding.UTF8.GetString(b,at,e-at);}
  Need(4);var magic=System.Text.Encoding.ASCII.GetString(b,p,4);p+=4;if(!magic.StartsWith("EQG",StringComparison.Ordinal))throw new InvalidDataException($"EQG model '{name}' has invalid magic '{magic}'.");
  var version=U32();var listLength=checked((int)U32());var materialCount=checked((int)U32());var vertexCount=checked((int)U32());var triangleCount=checked((int)U32());var boneCount=magic[3]=='M'?checked((int)U32()):0;
  var list=p;Need(listLength);p+=listLength;
  var mats=new List<SageEqgMaterial>(materialCount);
  for(var i=0;i<materialCount;i++){_=U32();var nameOff=checked((int)U32());var shaderOff=checked((int)U32());var propertyCount=checked((int)U32());var props=new List<SageEqgMaterialProperty>(propertyCount);for(var j=0;j<propertyCount;j++){var propOff=checked((int)U32());var type=U32();var raw=U32();var fv=type==0?BitConverter.Int32BitsToSingle(unchecked((int)raw)):0;var sv=type==2?Str(checked(list+(int)raw)):"";props.Add(new(Str(checked(list+propOff)),type,fv,type==0?0:raw,sv));}mats.Add(new(Str(checked(list+nameOff)),Str(checked(list+shaderOff)),props));}
  var verts=new List<SageEqgVertex>(vertexCount);
  for(var i=0;i<vertexCount;i++){if(version<3){var x=F32();var y=F32();var z=F32();var nx=F32();var ny=F32();var nz=F32();var u=F32();var v=F32();verts.Add(new(new(-x,-y,z),new(-nx,-ny,nz),new(-u,-v),0xffffffff));}else{var x=F32();var y=F32();var z=F32();var nx=F32();var ny=F32();var nz=F32();var color=U32();var uv1=F32();var uv2=F32();_=F32();_=F32();verts.Add(new(new(-x,-y,z),new(-nx,-ny,nz),new(uv1,uv2),color));}}
  var polys=new List<SageEqgPolygon>(triangleCount);for(var i=0;i<triangleCount;i++)polys.Add(new(U32(),U32(),U32(),I32(),U32()));
  var bones=new List<SageEqgBone>(boneCount);
  for(var i=0;i<boneCount;i++){var nameIdx=I32();var next=I32();var childrenCount=U32();var childrenIndex=I32();var x=F32();var y=F32();var z=F32();var rx=F32();var ry=F32();var rz=F32();var rw=F32();var sx=F32();var sy=F32();var sz=F32();bones.Add(new(Str(checked(list+nameIdx)),next,childrenCount,childrenIndex,new(-x,-y,z),new(-rx,-ry,rz,rw),new(sx,sy,sz)));}
  if(boneCount>0){for(var i=0;i<verts.Count;i++){var count=checked((int)U32());var weights=new List<SageEqgWeight>(4);for(var j=0;j<4;j++)weights.Add(new(I32(),F32()));verts[i]=verts[i] with{Weights=weights.Take(Math.Min(count,4)).ToArray()};}}
  return new(name,mats,verts,polys,bones);
 }
}

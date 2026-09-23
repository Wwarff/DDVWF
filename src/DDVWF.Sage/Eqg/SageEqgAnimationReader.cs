using System.Buffers.Binary;
using System.Numerics;

namespace DDVWF.Sage.Eqg;

// Direct parser for EQ Sage v1.8.15 src/lib/eqg/model/model.js Animation.
public sealed record SageEqgAnimationFrame(uint Milliseconds,Vector3 Translation,Quaternion Rotation,Vector3 Scale);
public sealed record SageEqgBoneAnimation(string BoneName,IReadOnlyList<SageEqgAnimationFrame> Frames);
public sealed record SageEqgAnimation(string Name,bool StrictBoneNumbering,IReadOnlyList<SageEqgBoneAnimation> Bones);

public static class SageEqgAnimationReader
{
 public static SageEqgAnimation Read(string name,ReadOnlySpan<byte> source)
 {
  var b=source.ToArray();var p=0;
  void Need(int n){if(n<0||p+n>b.Length)throw new InvalidDataException($"EQG animation '{name}' is truncated.");}
  uint U32(){Need(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}
  float F32()=>BitConverter.Int32BitsToSingle(unchecked((int)U32()));
  Need(4);var magic=System.Text.Encoding.ASCII.GetString(b,p,4);p+=4;if(magic!="EQGA")throw new InvalidDataException($"EQG animation '{name}' has invalid magic '{magic}'.");
  var version=U32();var listLength=checked((int)U32());var boneAnimationCount=checked((int)U32());var strict=version>1&&U32()==1;
  var list=p;Need(listLength);p+=listLength;
  string Str(int off){var at=checked(list+off);if(at<list||at>=list+listLength)throw new InvalidDataException($"EQG animation '{name}' string offset is invalid.");var e=at;while(e<b.Length&&b[e]!=0)e++;return System.Text.Encoding.UTF8.GetString(b,at,e-at).Replace(" ","",StringComparison.Ordinal);}
  var animations=new List<SageEqgBoneAnimation>(boneAnimationCount);
  for(var i=0;i<boneAnimationCount;i++)
  {
   var frameCount=checked((int)U32());var boneName=Str(checked((int)U32()));var frames=new List<SageEqgAnimationFrame>(frameCount);
   for(var j=0;j<frameCount;j++){var ms=U32();var x=F32();var y=F32();var z=F32();var rx=F32();var ry=F32();var rz=F32();var rw=F32();var sx=F32();var sy=F32();var sz=F32();frames.Add(new(ms,new(-x,-y,z),new(-rx,-ry,rz,rw),new(sx,sy,sz)));}
   // Sage uses unshift(), so preserve the same reversed bone-animation order.
   animations.Insert(0,new(boneName,frames));
  }
  return new(name,strict,animations);
 }
}

using System.Buffers.Binary;using System.Numerics;
namespace DDVWF.Sage.Wld;
public sealed record SageLightSource(uint Flags,uint FrameCount,IReadOnlyList<float> Levels,IReadOnlyList<Vector3> Colors);
public sealed record SageLightInstance(int ReferenceIndex,Vector3 Position,float Radius);
public static class WldLightReader
{
 public static SageLightSource ReadSource(WldFragment f,ReadOnlySpan<byte> source){var r=new R(source,f.PayloadOffset);var flags=r.U();var count=r.U();if((flags&1)!=0)_=r.U();if((flags&2)!=0)_=r.U();var levels=new List<float>();if((flags&4)!=0)for(var i=0;i<count;i++)levels.Add(r.F());var colors=new List<Vector3>();if((flags&0x10)!=0)for(var i=0;i<count;i++)colors.Add(new(r.F(),r.F(),r.F()));return new(flags,count,levels,colors);}
 public static SageLightInstance ReadInstance(WldFragment f,ReadOnlySpan<byte> source){var r=new R(source,f.PayloadOffset);var reference=checked((int)r.U()-1);_=r.U();return new(reference,new(r.F(),r.F(),r.F()),r.F());}
 sealed class R{readonly byte[] b;int p;public R(ReadOnlySpan<byte>s,int o){b=s.ToArray();p=o;}void N(int n){if(p+n>b.Length)throw new InvalidDataException("Light fragment is truncated.");}public uint U(){N(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}public float F()=>BitConverter.Int32BitsToSingle(unchecked((int)U()));}
}
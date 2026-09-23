using System.Buffers.Binary;using System.Numerics;
namespace DDVWF.Sage.Wld;
public sealed record SageBoneFrame(float Scale,Vector3 Translation,Quaternion Rotation);
public sealed record SageTrackDefinition(IReadOnlyList<SageBoneFrame> Frames);
public sealed record SageTrackReference(int DefinitionIndex,int FrameMs);
public sealed record SageAnimatedVertices(int DelayMs,IReadOnlyList<IReadOnlyList<Vector3>> Frames);
public static class WldAnimationReader
{
 public static SageTrackDefinition ReadTrackDefinition(WldFragment f,ReadOnlySpan<byte> source){var r=new R(source,f.PayloadOffset);_=r.U32();var count=r.U32();var frames=new List<SageBoneFrame>(checked((int)count));for(var i=0;i<count;i++){var rw=r.I16();var rx=r.I16();var ry=r.I16();var rz=r.I16();var sx=r.I16();var sy=r.I16();var sz=r.I16();var sd=r.I16();var scale=sd==0?0:sd/256f;var tr=sd==0?Vector3.Zero:new(sx/256f,-sy/256f,sz/256f);var q=Quaternion.Normalize(new(rx,ry,rz,rw));frames.Add(new(scale,tr,q));}return new(frames);}
 public static SageTrackReference ReadTrackReference(WldFragment f,ReadOnlySpan<byte> source){var r=new R(source,f.PayloadOffset);var idx=unchecked((int)r.U32())-1;var flags=r.U32();var ms=(flags&1)!=0?r.I32():0;return new(idx,ms);}
 public static SageAnimatedVertices ReadAnimatedVertices(WldFragment f,ReadOnlySpan<byte> source){var r=new R(source,f.PayloadOffset);_=r.I32();var vc=r.U16();var fc=r.U16();var delay=r.U16();_=r.U16();var exp=r.U16();var scale=1f/(1<<exp);var frames=new List<IReadOnlyList<Vector3>>(fc);for(var i=0;i<fc;i++){var frame=new List<Vector3>(vc);for(var j=0;j<vc;j++)frame.Add(new(r.I16()*scale,r.I16()*scale,r.I16()*scale));frames.Add(frame);}return new(delay,frames);}
 sealed class R{readonly byte[] b;int p;public R(ReadOnlySpan<byte>s,int o){b=s.ToArray();p=o;}void N(int n){if(p+n>b.Length)throw new InvalidDataException("Animation fragment is truncated.");}public ushort U16(){N(2);var v=BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(p,2));p+=2;return v;}public short I16()=>unchecked((short)U16());public uint U32(){N(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}public int I32()=>unchecked((int)U32());}
}

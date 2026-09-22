using System.Buffers.Binary;
using System.Text;

namespace DDVWF.Sage.Wld;

public sealed record SageBitmapName(string FileName);
public sealed record SageBitmapInfo(int Flags,int CurrentFrame,int AnimationDelayMs,IReadOnlyList<int> BitmapNameIndices)
{
 public bool IsAnimated=>(Flags&0x08)!=0; public bool HasSleep=>(Flags&0x10)!=0; public bool HasCurrentFrame=>(Flags&0x20)!=0;
}
public sealed record SageMaterialRecord(uint Flags,uint Parameters,uint Color,float Brightness,float ScaledAmbient,int BitmapInfoReferenceIndex,SageShaderType Shader);
public sealed record SageMaterialList(uint Flags,IReadOnlyList<int> MaterialIndices);

public static class WldMaterialReader
{
 public static int ReadReference(WldFragment f,ReadOnlySpan<byte> s){var b=s.ToArray();Need(b,f.PayloadOffset,4);return checked((int)BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(f.PayloadOffset,4))-1);}
 public static SageBitmapName ReadBitmapName(WldFragment f,ReadOnlySpan<byte> s)
 {
  var b=s.ToArray();var p=f.PayloadOffset;Need(b,p,6);var count=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;if(count>1){}var len=BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(p,2));p+=2;
  if(len<1){return new("");}Need(b,p,len-1);return new(Encoding.Latin1.GetString(b,p,len-1));
 }
 public static SageBitmapInfo ReadBitmapInfo(WldFragment f,ReadOnlySpan<byte> s)
 {
  var b=s.ToArray();var p=f.PayloadOffset;int I(){Need(b,p,4);var v=BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}uint U()=>unchecked((uint)I());
  var flags=I();var count=I();if(count<0)throw new InvalidDataException("BitmapInfo count is negative.");var current=(flags&0x20)!=0?checked((int)U()):0;var delay=((flags&0x08)!=0&&(flags&0x10)!=0)?I():0;var refs=new List<int>(count);for(var i=0;i<count;i++)refs.Add(I()-1);return new(flags,current,delay,refs);
 }
 public static SageMaterialRecord ReadMaterial(WldFragment f,ReadOnlySpan<byte> s)
 {
  var b=s.ToArray();var p=f.PayloadOffset;uint U(){Need(b,p,4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}float F()=>BitConverter.Int32BitsToSingle(unchecked((int)U()));
  var flags=U();var parameters=U();var color=U();var bright=F();var ambient=F();var bitmap=checked((int)U()-1);return new(flags,parameters,color,bright,ambient,bitmap,SageMaterial.Map(parameters,bitmap));
 }
 public static SageMaterialList ReadMaterialList(WldFragment f,ReadOnlySpan<byte> s)
 {
  var b=s.ToArray();var p=f.PayloadOffset;Need(b,p,8);var flags=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;var count=checked((int)BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4)));p+=4;var list=new List<int>(count);for(var i=0;i<count;i++){Need(b,p,4);list.Add(checked((int)BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4))-1));p+=4;}return new(flags,list);
 }
 private static void Need(byte[] b,int p,int n){if(p<0||n<0||p+n>b.Length)throw new InvalidDataException("Material fragment is truncated.");}
}

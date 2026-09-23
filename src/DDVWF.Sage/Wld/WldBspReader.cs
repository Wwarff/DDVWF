using System.Buffers.Binary;using System.Numerics;using DDVWF.Core.Zone;
namespace DDVWF.Sage.Wld;
public sealed record SageBspNode(Vector3 Normal,float SplitDistance,int RegionId,int Left,int Right,Vector3 Min,Vector3 Max);
public sealed record SageBspRegion(int Index,bool ContainsPolygons,int MeshReference);
public sealed record SageRegionType(IReadOnlyList<int> RegionIndices,string Tag,RegionEntityData Data);
public static class WldBspReader
{
 public static IReadOnlyList<SageBspNode> ReadTree(WldFragment f,ReadOnlySpan<byte>s){var r=new R(s,f.PayloadOffset);var count=checked((int)r.U());var a=new List<SageBspNode>(count);for(var i=0;i<count;i++)a.Add(new(new(r.F(),r.F(),r.F()),r.F(),r.I(),r.I()-1,r.I()-1,Vector3.Zero,Vector3.Zero));return a;}
 public static SageBspRegion ReadRegion(int index,WldFragment f,ReadOnlySpan<byte>s){var r=new R(s,f.PayloadOffset);var flags=r.I();_=r.I();var d1=r.I();var d2=r.I();_=r.I();var d3=r.I();var d4=r.I();_=r.I();var d5=r.I();_=r.I();r.Skip(checked(12*(d1+d2)));for(var i=0;i<d3;i++){_=r.I();var n=r.I();r.Skip(checked(n*4));}if(d4!=0)throw new InvalidDataException("Sage BSP data4 branch is source-unimplemented and cannot be guessed.");r.Skip(checked(d5*28));var pvs=r.I16();if(pvs<0)throw new InvalidDataException("Negative BSP PVS size.");r.Skip(pvs);_=r.U();r.Skip(16);var mesh=flags==0x181?r.I():-1;return new(index,flags==0x181,mesh);}
 public static SageRegionType ReadType(WldDocument doc,WldFragment f,ReadOnlySpan<byte>s){var r=new R(s,f.PayloadOffset);_=r.I();var count=r.I();if(count<0)throw new InvalidDataException("Negative region count.");var ids=new List<int>(count);for(var i=0;i<count;i++)ids.Add(r.I());var n=r.I();if(n<0)throw new InvalidDataException("Negative region tag size.");var bytes=r.Bytes(n);for(var i=0;i<bytes.Length;i++)bytes[i]^=new byte[]{0x95,0x3A,0xC5,0x2A,0x95,0x7A,0x95,0x6A}[i&7];var tag=System.Text.Encoding.Latin1.GetString(bytes).TrimEnd('\0').ToLowerInvariant();tag=string.IsNullOrWhiteSpace(tag)?f.Name.ToLowerInvariant():tag;return new(ids,tag,ClassifyTag(tag));}
 public static RegionEntityData ClassifyTag(string tag)
 {
  tag=(tag??string.Empty).ToLowerInvariant();var kinds=new List<RegionSemantic>();int? reference=null,zone=null,x=null,y=null,z=null,rot=null;
  if(tag.StartsWith("wtn_")||tag.StartsWith("wt_"))kinds.Add(RegionSemantic.Water);
  else if(tag.StartsWith("wtntp")){kinds.Add(RegionSemantic.Water);kinds.Add(RegionSemantic.Zoneline);DecodeZoneLine(tag,ref reference,ref zone,ref x,ref y,ref z,ref rot);}
  else if(tag.StartsWith("lan_")||tag.StartsWith("la_"))kinds.Add(RegionSemantic.Lava);
  else if(tag.StartsWith("lantp")){kinds.Add(RegionSemantic.Lava);kinds.Add(RegionSemantic.Zoneline);DecodeZoneLine(tag,ref reference,ref zone,ref x,ref y,ref z,ref rot);}
  else if(tag.StartsWith("drntp")){kinds.Add(RegionSemantic.Zoneline);DecodeZoneLine(tag,ref reference,ref zone,ref x,ref y,ref z,ref rot);}
  else if(tag.StartsWith("drp_"))kinds.Add(RegionSemantic.Pvp);
  else if(tag.StartsWith("drn_"))kinds.Add(tag.Contains("_s_")?RegionSemantic.Slippery:RegionSemantic.Unknown);
  else if(tag.StartsWith("sln_"))kinds.Add(RegionSemantic.WaterBlockLos);
  else if(tag.StartsWith("vwn_"))kinds.Add(RegionSemantic.FreezingWater);
  else kinds.Add(RegionSemantic.Normal);
  return new(kinds,reference,zone,x,y,z,rot);
 }
 static void DecodeZoneLine(string tag,ref int? reference,ref int? zone,ref int? x,ref int? y,ref int? z,ref int? rot)
 {
  if(tag=="drntp_zone"){reference=0;return;}if(tag.Length<10)return;if(!int.TryParse(tag.AsSpan(5,Math.Min(5,tag.Length-5)),out var zid))return;
  if(zid==255){if(tag.Length>=16&&int.TryParse(tag.AsSpan(10,6),out var ri))reference=ri;return;}zone=zid;
  static int? Slice(string s,int start,int len)=>s.Length>=start+len&&int.TryParse(s.AsSpan(start,len),out var v)?v:null;
  x=Slice(tag,10,6);y=Slice(tag,16,6);z=Slice(tag,22,6);rot=Slice(tag,28,3);
 }
 sealed class R{readonly byte[]b;int p;public R(ReadOnlySpan<byte>s,int o){b=s.ToArray();p=o;}void N(int n){if(n<0||p+n>b.Length)throw new InvalidDataException("BSP fragment truncated.");}public uint U(){N(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}public int I()=>unchecked((int)U());public short I16(){N(2);var v=BinaryPrimitives.ReadInt16LittleEndian(b.AsSpan(p,2));p+=2;return v;}public float F()=>BitConverter.Int32BitsToSingle(I());public void Skip(int n){N(n);p+=n;}public byte[] Bytes(int n){N(n);var x=b.AsSpan(p,n).ToArray();p+=n;return x;}}
}
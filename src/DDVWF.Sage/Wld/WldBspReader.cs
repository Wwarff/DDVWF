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

 public sealed record BoundedRegion(int SourceRegionIndex,string Tag,RegionEntityData Data,Vector3 Min,Vector3 Max,Vector3 Center);
 public static IReadOnlyList<BoundedRegion> BuildBoundedRegions(WldDocument doc,ReadOnlySpan<byte> source)
 {
  var bytes=source.ToArray();
  var treeFragment=doc.Fragments.FirstOrDefault(x=>x.KnownType==WldFragmentType.BspTree);if(treeFragment is null)return Array.Empty<BoundedRegion>();
  var nodes=ReadTree(treeFragment,bytes).ToArray();if(nodes.Length==0)return Array.Empty<BoundedRegion>();
  var regions=doc.Fragments.Where(x=>x.KnownType==WldFragmentType.BspRegion).Select((x,i)=>ReadRegion(i,x,bytes)).ToArray();
  var typeByRegion=new Dictionary<int,SageRegionType>();foreach(var f in doc.Fragments.Where(x=>x.KnownType==WldFragmentType.RegionType)){var t=ReadType(doc,f,bytes);foreach(var ri in t.RegionIndices.Where(i=>i>=0&&i<regions.Length))typeByRegion[ri]=t;}
  var triangles=new List<(Vector3 A,Vector3 B,Vector3 C)>();foreach(var f in doc.Fragments.Where(x=>x.KnownType==WldFragmentType.Mesh)){var m=WldMeshReader.Read(doc,f,bytes);foreach(var p in m.Polygons){if(p.A>=m.Vertices.Count||p.B>=m.Vertices.Count||p.C>=m.Vertices.Count)continue;Vector3 P(int i)=>m.Vertices[i]+m.Center;triangles.Add((P(p.A),P(p.B),P(p.C)));}}
  static (Vector3 Min,Vector3 Max) MinMax(IEnumerable<(Vector3 A,Vector3 B,Vector3 C)> ts){var pts=ts.SelectMany(t=>new[]{t.A,t.B,t.C}).ToArray();if(pts.Length==0)return(Vector3.Zero,Vector3.Zero);var min=pts[0];var max=pts[0];foreach(var p in pts.Skip(1)){min=Vector3.Min(min,p);max=Vector3.Max(max,p);}return(min,max);}
  bool Left(SageBspNode n,Vector3 p)=>(p.X*n.Normal.X)+.01f+(p.Y*n.Normal.Y)+.01f+(p.Z*n.Normal.Z)+.01f+n.SplitDistance>0;
  var leaves=new List<BoundedRegion>();void Walk(int ni,List<(Vector3 A,Vector3 B,Vector3 C)> polys){if(ni<0||ni>=nodes.Length)return;var n=nodes[ni];var mm=MinMax(polys);var regionIndex=n.RegionId-1;if(regionIndex>=0&&typeByRegion.TryGetValue(regionIndex,out var type)){var min=new Vector3(mm.Min.X,mm.Min.Z,mm.Min.Y);var max=new Vector3(mm.Max.X,mm.Max.Z,mm.Max.Y);leaves.Add(new(regionIndex,type.Tag,type.Data,min,max,(min+max)/2));}var left=new List<(Vector3,Vector3,Vector3)>();var right=new List<(Vector3,Vector3,Vector3)>();foreach(var t in polys){if(Left(n,t.A)&&Left(n,t.B)&&Left(n,t.C))left.Add(t);else right.Add(t);}if(n.Left>=0)Walk(n.Left,left);if(n.Right>=0)Walk(n.Right,right);}
  Walk(0,triangles);
  var expanded=leaves.SelectMany(x=>x.Data.Semantics.Count==1?new[]{x}:x.Data.Semantics.Select(s=>{var d=x.Data with{Semantics=new[]{s}};if(s!=RegionSemantic.Zoneline)d=d with{ZoneLineReference=null,TargetZoneIndex=null,TargetX=null,TargetY=null,TargetZ=null,TargetRotation=null};return x with{Data=d};})).ToList();
  string Key(BoundedRegion x)=>$"{x.Data.Semantics[0]}|{x.Data.ZoneLineReference}|{x.Data.TargetZoneIndex}|{x.Data.TargetX}|{x.Data.TargetY}|{x.Data.TargetZ}|{x.Data.TargetRotation}";
  bool Inside(BoundedRegion a,BoundedRegion c)=>a.Min.X+20>=c.Min.X&&a.Min.Y+20>=c.Min.Y&&a.Min.Z+20>=c.Min.Z&&a.Max.X<=c.Max.X+20&&a.Max.Y<=c.Max.Y+20&&a.Max.Z<=c.Max.Z+20;
  bool Adj(BoundedRegion a,BoundedRegion c){var dx=MathF.Min(a.Max.X,c.Max.X)-MathF.Max(a.Min.X,c.Min.X);var dy=MathF.Min(a.Max.Y,c.Max.Y)-MathF.Max(a.Min.Y,c.Min.Y);var dz=MathF.Min(a.Max.Z,c.Max.Z)-MathF.Max(a.Min.Z,c.Min.Z);return Inside(a,c)||(dx==0&&dy>0&&dz>0)||(dy==0&&dx>0&&dz>0)||(dz==0&&dx>0&&dy>0);}
  var output=new List<BoundedRegion>();foreach(var group in expanded.GroupBy(Key)){var g=group.ToList();var parent=Enumerable.Range(0,g.Count).ToArray();int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}void Union(int i,int j){i=Find(i);j=Find(j);if(i!=j)parent[j]=i;}for(var i=0;i<g.Count;i++)for(var j=i+1;j<g.Count;j++)if(Adj(g[i],g[j])||Adj(g[j],g[i]))Union(i,j);foreach(var comp in Enumerable.Range(0,g.Count).GroupBy(Find)){var xs=comp.Select(i=>g[i]).ToArray();var min=xs.Select(x=>x.Min).Aggregate(Vector3.Min);var max=xs.Select(x=>x.Max).Aggregate(Vector3.Max);var first=xs[0];output.Add(first with{Min=min,Max=max,Center=(min+max)/2});}}
  return output.GroupBy(x=>$"{x.Min}|{x.Max}|{x.Center}|{x.Data.Semantics[0]}").Select(x=>x.First()).ToArray();
 }
 sealed class R{readonly byte[]b;int p;public R(ReadOnlySpan<byte>s,int o){b=s.ToArray();p=o;}void N(int n){if(n<0||p+n>b.Length)throw new InvalidDataException("BSP fragment truncated.");}public uint U(){N(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}public int I()=>unchecked((int)U());public short I16(){N(2);var v=BinaryPrimitives.ReadInt16LittleEndian(b.AsSpan(p,2));p+=2;return v;}public float F()=>BitConverter.Int32BitsToSingle(I());public void Skip(int n){N(n);p+=n;}public byte[] Bytes(int n){N(n);var x=b.AsSpan(p,n).ToArray();p+=n;return x;}}
}
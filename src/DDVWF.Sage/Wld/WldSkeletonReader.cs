using System.Buffers.Binary;using System.Numerics;
namespace DDVWF.Sage.Wld;
public sealed record SageSkeletonBone(int Index,string Name,int TrackReferenceIndex,int MeshReferenceIndex,IReadOnlyList<int> Children);
public sealed record SageSkeleton(string ModelBase,uint Flags,Vector3? CenterOffset,float BoundingRadius,IReadOnlyList<SageSkeletonBone> Bones,IReadOnlyList<int> MeshReferences);
public static class WldSkeletonReader
{
 public static SageSkeleton Read(WldDocument doc,WldFragment f,ReadOnlySpan<byte> source){var r=new R(source,f.PayloadOffset);var flags=r.U32();var count=checked((int)r.U32());_=r.U32();Vector3? center=null;if((flags&1)!=0)center=new(r.F(),r.F(),r.F());var radius=(flags&2)!=0?r.F():0;var bones=new List<SageSkeletonBone>(count);for(var i=0;i<count;i++){var name=doc.ResolveString(r.I32());_=r.I32();var track=r.I32()-1;var mesh=r.I32()-1;var cc=r.I32();if(cc<0)throw new InvalidDataException("Skeleton child count is negative.");var children=new List<int>(cc);for(var j=0;j<cc;j++)children.Add(r.I32());bones.Add(new(i,name,track,mesh,children));}var meshes=new List<int>();if((flags&0x200)!=0){var n=checked((int)r.U32());for(var i=0;i<n;i++)meshes.Add(unchecked((int)r.U32())-1);}ValidateTree(bones);var modelBase=f.Name.Replace("_HS_DEF","",StringComparison.OrdinalIgnoreCase).Trim().ToLowerInvariant();return new(modelBase,flags,center,radius,bones,meshes);}
 static void ValidateTree(IReadOnlyList<SageSkeletonBone>b){foreach(var bone in b)foreach(var c in bone.Children)if(c<0||c>=b.Count)throw new InvalidDataException("Skeleton child index is out of range.");}
 sealed class R{readonly byte[]b;int p;public R(ReadOnlySpan<byte>s,int o){b=s.ToArray();p=o;}void N(int n){if(n<0||p+n>b.Length)throw new InvalidDataException("Skeleton fragment is truncated.");}public uint U32(){N(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}public int I32()=>unchecked((int)U32());public float F()=>BitConverter.Int32BitsToSingle(I32());}
}

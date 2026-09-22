using System.Buffers.Binary;
using System.Numerics;

namespace DDVWF.Sage.Wld;

public sealed record SagePolygon(bool IsSolid,ushort A,ushort B,ushort C);
public sealed record SageRenderGroup(ushort PolygonCount,ushort MaterialIndex);
public sealed record SageMobPiece(int BoneIndex,int Start,int Count);
public sealed record SageMesh(
 int MaterialListIndex,int AnimatedVerticesReferenceIndex,Vector3 Center,float MaxDistance,Vector3 Min,Vector3 Max,
 IReadOnlyList<Vector3> Vertices,IReadOnlyList<Vector2> Uvs,IReadOnlyList<Vector3> Normals,
 IReadOnlyList<uint> Colors,IReadOnlyList<SagePolygon> Polygons,IReadOnlyList<SageRenderGroup> MaterialGroups,IReadOnlyList<SageMobPiece> MobPieces);

public static class WldMeshReader
{
 public static SageMesh Read(WldDocument doc,WldFragment fragment,ReadOnlySpan<byte> source)
 {
  var b=source.ToArray();var p=fragment.PayloadOffset;
  void Need(int n){if(n<0||p+n>b.Length)throw new InvalidDataException("Mesh fragment is truncated.");}
  int I16(){Need(2);var v=BinaryPrimitives.ReadInt16LittleEndian(b.AsSpan(p,2));p+=2;return v;}
  ushort U16(){Need(2);var v=BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(p,2));p+=2;return v;}
  uint U32(){Need(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}
  float F32(){Need(4);var v=BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(p,4)));p+=4;return v;}
  Vector3 V3()=>new(F32(),F32(),F32());
  _=U32();var materials=checked((int)U32()-1);var animated=checked((int)U32()-1);Need(8);p+=8;var center=V3();Need(12);p+=12;
  var maxDistance=F32();var min=V3();var max=V3();
  var vertexCount=I16();var uvCount=I16();var normalCount=I16();var colorCount=I16();var polygonCount=I16();var vertexPieceCount=I16();var groupCount=I16();var vertexTextureCount=I16();var size9=I16();var exponent=I16();
  var scale=1f/(1<<exponent);var vertices=new List<Vector3>(Math.Max(0,vertexCount));
  for(var i=0;i<vertexCount;i++)vertices.Add(new(I16()*scale,I16()*scale,I16()*scale));
  var uvs=new List<Vector2>(Math.Max(0,uvCount));
  for(var i=0;i<uvCount;i++)uvs.Add(doc.IsNewFormat?new(F32(),F32()):new(I16()/256f,I16()/256f));
  var normals=new List<Vector3>(Math.Max(0,normalCount));
  for(var i=0;i<normalCount;i++){Need(3);var v=new Vector3(unchecked((sbyte)b[p++])/128f,unchecked((sbyte)b[p++])/128f,unchecked((sbyte)b[p++])/128f);normals.Add(v==Vector3.Zero?v:Vector3.Normalize(v));}
  var colors=new List<uint>(Math.Max(0,colorCount));for(var i=0;i<colorCount;i++){Need(4);colors.Add(BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4)));p+=4;}
  var polygons=new List<SagePolygon>(Math.Max(0,polygonCount));for(var i=0;i<polygonCount;i++)polygons.Add(new(I16()==0,unchecked((ushort)I16()),unchecked((ushort)I16()),unchecked((ushort)I16())));
  var pieces=new List<SageMobPiece>(Math.Max(0,vertexPieceCount));var mobStart=0;for(var i=0;i<vertexPieceCount;i++){var count=I16();var bone=I16();pieces.Add(new(bone,mobStart,count));mobStart+=count;}
  var groups=new List<SageRenderGroup>(Math.Max(0,groupCount));for(var i=0;i<groupCount;i++)groups.Add(new(U16(),U16()));
  for(var n=0;n<vertexTextureCount;n++){Need(4);p+=4;}for(var n=0;n<size9;n++){Need(12);p+=12;}
  while(uvs.Count<vertices.Count)uvs.Add(Vector2.Zero);
  return new(materials,animated,center,maxDistance,min,max,vertices,uvs,normals,colors,polygons,groups,pieces);
 }
}

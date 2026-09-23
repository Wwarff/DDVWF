using System.Numerics;
namespace DDVWF.Core.Zone;
public sealed record RenderVertex(Vector3 Position,Vector3 Normal,Vector2 Uv,int SourceIndex=-1,int BoneIndex=0,uint Color=0xFFFFFFFF);
public sealed record RenderMorphFrame(IReadOnlyList<Vector3> PositionDeltas,int DelayMs);
public sealed record RenderMaterial(string Name,string? TextureName,string Shader,float Alpha=1f,bool AlphaTest=false,bool Additive=false,bool Unlit=false,IReadOnlyList<string>? TextureFrames=null,int AnimationDelayMs=0,int CurrentFrame=0,float Brightness=0,float ScaledAmbient=0);
public sealed record RenderPrimitive(string Name,IReadOnlyList<RenderVertex> Vertices,IReadOnlyList<ushort> Indices,string? TextureName,bool PassThrough,RenderMaterial? Material=null,IReadOnlyList<RenderMorphFrame>? MorphFrames=null);
public sealed record RenderBone(string Name,int ParentIndex,Vector3 Translation,Quaternion Rotation,float Scale);
public sealed record RenderSkeleton(IReadOnlyList<RenderBone> Bones);
public sealed record RenderMesh(string Name,IReadOnlyList<RenderPrimitive> Primitives,bool FlipRootX,RenderSkeleton? Skeleton=null);
public interface IClientRenderAssetProvider:IClientZoneProvider{IReadOnlyDictionary<string,RenderMesh> RenderMeshes{get;} IReadOnlyDictionary<string,byte[]> TextureFiles{get;}}

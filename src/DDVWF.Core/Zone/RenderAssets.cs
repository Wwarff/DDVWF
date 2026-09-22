using System.Numerics;
namespace DDVWF.Core.Zone;
public sealed record RenderVertex(Vector3 Position,Vector3 Normal,Vector2 Uv);
public sealed record RenderMaterial(string Name,string? TextureName,string Shader,float Alpha=1f,bool AlphaTest=false,bool Additive=false,bool Unlit=false,IReadOnlyList<string>? TextureFrames=null,int AnimationDelayMs=0);
public sealed record RenderPrimitive(string Name,IReadOnlyList<RenderVertex> Vertices,IReadOnlyList<ushort> Indices,string? TextureName,bool PassThrough,RenderMaterial? Material=null);
public sealed record RenderMesh(string Name,IReadOnlyList<RenderPrimitive> Primitives,bool FlipRootX);
public interface IClientRenderAssetProvider:IClientZoneProvider{IReadOnlyDictionary<string,RenderMesh> RenderMeshes{get;} IReadOnlyDictionary<string,byte[]> TextureFiles{get;}}

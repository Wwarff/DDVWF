using System.Numerics;
namespace DDVWF.Core.Zone;
public sealed record RenderVertex(Vector3 Position,Vector3 Normal,Vector2 Uv);
public sealed record RenderPrimitive(string Name,IReadOnlyList<RenderVertex> Vertices,IReadOnlyList<ushort> Indices,string? TextureName,bool PassThrough);
public sealed record RenderMesh(string Name,IReadOnlyList<RenderPrimitive> Primitives,bool FlipRootX);
public interface IClientRenderAssetProvider:IClientZoneProvider{IReadOnlyDictionary<string,RenderMesh> RenderMeshes{get;}}

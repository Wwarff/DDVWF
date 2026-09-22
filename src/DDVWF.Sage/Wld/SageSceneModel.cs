using System.Numerics;
namespace DDVWF.Sage.Wld;

public sealed record SageTexture(string FileName);
public sealed record SageSceneMaterial(string Name,SageShaderType Shader,float Brightness,float ScaledAmbient,IReadOnlyList<SageTexture> Frames,int AnimationDelayMs);
public sealed record SageSceneVertex(Vector3 Position,Vector3 Normal,Vector2 Uv);
public sealed record SageScenePrimitive(string Name,int MaterialIndex,bool PassThrough,IReadOnlyList<SageSceneVertex> Vertices,IReadOnlyList<ushort> Indices);
public sealed record SageSceneMesh(string Name,IReadOnlyList<SageScenePrimitive> Primitives);
public sealed record SageSceneDocument(IReadOnlyList<SageSceneMaterial> Materials,IReadOnlyList<SageSceneMesh> Meshes,bool FlipRootX=true);

public static class SageExportTransform
{
 public static Vector3 Position(Vector3 local,Vector3 center)=>new(local.X+center.X,local.Z+center.Z,local.Y+center.Y);
 public static Vector3 Normal(Vector3 n)=>new(-n.X,n.Z,n.Y);
}

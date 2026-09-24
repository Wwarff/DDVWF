using System.Numerics;
namespace DDVWF.Sage.Wld;

public sealed record SageTexture(string FileName);
public sealed record SageSceneMaterial(string Name,SageShaderType Shader,float Brightness,float ScaledAmbient,IReadOnlyList<SageTexture> Frames,int AnimationDelayMs,int CurrentFrame=0,bool SkipFrames=false);
public sealed record SageSceneVertex(Vector3 Position,Vector3 Normal,Vector2 Uv,int SourceIndex=-1,uint Color=0xFFFFFFFF);
public sealed record SageSceneMorphFrame(IReadOnlyList<Vector3> PositionDeltas,int DelayMs);
public sealed record SageScenePrimitive(string Name,int MaterialIndex,bool PassThrough,IReadOnlyList<SageSceneVertex> Vertices,IReadOnlyList<ushort> Indices,IReadOnlyList<SageSceneMorphFrame>? MorphFrames=null);
public sealed record SageSceneMesh(string Name,IReadOnlyList<SageScenePrimitive> Primitives);
public sealed record SageSceneDocument(IReadOnlyList<SageSceneMaterial> Materials,IReadOnlyList<SageSceneMesh> Meshes,bool FlipRootX=true);

public static class SageExportTransform
{
 // EQ Sage exportZone writes Babylon-facing vertex data first, then applies scale(-1,1,1) to the exported zone root.
 // Keeping the root mirror separate is required so positions and normals receive the same final handedness operation.
 public static Vector3 Position(Vector3 local,Vector3 center)=>new(local.X+center.X,local.Z+center.Z,local.Y+center.Y);
 public static Vector3 Normal(Vector3 n)=>new(-n.X,n.Z,n.Y);
}

using System.Numerics;
namespace DDVWF.Sage.Wld;

public sealed record SageTexture(string FileName);
public sealed record SageSceneMaterial(string Name,SageShaderType Shader,float Brightness,float ScaledAmbient,IReadOnlyList<SageTexture> Frames,int AnimationDelayMs);
public sealed record SageScenePrimitive(int MaterialIndex,IReadOnlyList<SagePolygon> Polygons);
public sealed record SageSceneMesh(string Name,Vector3 Center,IReadOnlyList<Vector3> Vertices,IReadOnlyList<Vector2> Uvs,IReadOnlyList<Vector3> Normals,IReadOnlyList<SageScenePrimitive> Primitives);
public sealed record SageSceneDocument(IReadOnlyList<SageSceneMaterial> Materials,IReadOnlyList<SageSceneMesh> Meshes);

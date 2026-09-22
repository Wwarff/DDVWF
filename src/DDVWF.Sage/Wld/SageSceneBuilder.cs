namespace DDVWF.Sage.Wld;

public static class SageSceneBuilder
{
 public static SageSceneDocument Build(WldDocument doc,ReadOnlySpan<byte> source)
 {
  var materials=new List<SageSceneMaterial>();var materialIndex=new Dictionary<int,int>();
  for(var i=0;i<doc.Fragments.Count;i++)
  {
   var f=doc.Fragments[i];if(f.KnownType!=WldFragmentType.Material)continue;var m=WldMaterialReader.ReadMaterial(f,source);var frames=new List<SageTexture>();var delay=0;
   if(m.BitmapInfoReferenceIndex>=0&&m.BitmapInfoReferenceIndex<doc.Fragments.Count){var reference=doc.Fragments[m.BitmapInfoReferenceIndex];if(reference.KnownType==WldFragmentType.FragmentReference){var ii=WldMaterialReader.ReadReference(reference,source);if(ii>=0&&ii<doc.Fragments.Count&&doc.Fragments[ii].KnownType==WldFragmentType.BitmapInfo){var info=WldMaterialReader.ReadBitmapInfo(doc.Fragments[ii],source);delay=info.AnimationDelayMs;foreach(var bi in info.BitmapNameIndices)if(bi>=0&&bi<doc.Fragments.Count&&doc.Fragments[bi].KnownType==WldFragmentType.BitmapName)frames.Add(new(WldMaterialReader.ReadBitmapName(doc.Fragments[bi],source).FileName));}}}
   materialIndex[i]=materials.Count;materials.Add(new(f.Name,m.Shader,m.Brightness,m.ScaledAmbient,frames,delay));
  }
  var meshes=new List<SageSceneMesh>();
  foreach(var f in doc.Fragments.Where(x=>x.KnownType==WldFragmentType.Mesh))
  {
   var m=WldMeshReader.Read(doc,f,source);SageAnimatedVertices? animated=null;if(m.AnimatedVerticesReferenceIndex>=0&&m.AnimatedVerticesReferenceIndex<doc.Fragments.Count){var ar=doc.Fragments[m.AnimatedVerticesReferenceIndex];var ai=ar.KnownType==WldFragmentType.AnimatedVertexReference?WldMaterialReader.ReadReference(ar,source):m.AnimatedVerticesReferenceIndex;if(ai>=0&&ai<doc.Fragments.Count&&doc.Fragments[ai].KnownType==WldFragmentType.AnimatedVertices)animated=WldAnimationReader.ReadAnimatedVertices(doc.Fragments[ai],source);}var prims=new List<SageScenePrimitive>();var po=0;
   if(m.MaterialListIndex>=0&&m.MaterialListIndex<doc.Fragments.Count&&doc.Fragments[m.MaterialListIndex].KnownType==WldFragmentType.MaterialList)
   {
    var list=WldMaterialReader.ReadMaterialList(doc.Fragments[m.MaterialListIndex],source);
    foreach(var group in m.MaterialGroups)
    {
     var polys=m.Polygons.Skip(po).Take(group.PolygonCount).ToArray();po+=group.PolygonCount;if(group.MaterialIndex>=list.MaterialIndices.Count)continue;
     if(!materialIndex.TryGetValue(list.MaterialIndices[group.MaterialIndex],out var mi))continue;
     var pass=polys.Any(x=>!x.IsSolid);var name=materials[mi].Name+(pass?"-passthrough":"");var vertices=new List<SageSceneVertex>();var indices=new List<ushort>();var dedup=new Dictionary<(System.Numerics.Vector3,System.Numerics.Vector3,System.Numerics.Vector2),ushort>();
     foreach(var poly in polys)foreach(var idx in new[]{poly.A,poly.B,poly.C}){if(idx>=m.Vertices.Count||idx>=m.Normals.Count||idx>=m.Uvs.Count)throw new InvalidDataException("Mesh polygon references a missing vertex attribute.");var v=new SageSceneVertex(SageExportTransform.Position(m.Vertices[idx],m.Center),SageExportTransform.Normal(m.Normals[idx]),m.Uvs[idx],idx);var key=(v.Position,v.Normal,v.Uv);if(!dedup.TryGetValue(key,out var di)){if(vertices.Count>=ushort.MaxValue)throw new InvalidDataException("Sage primitive exceeds UInt16 index range.");di=(ushort)vertices.Count;dedup[key]=di;vertices.Add(v);}indices.Add(di);}
     IReadOnlyList<SageSceneMorphFrame>? morph=null;if(animated is not null&&animated.Frames.Count>0){morph=animated.Frames.Select(frame=>new SageSceneMorphFrame(vertices.Select(v=>{if(v.SourceIndex<0||v.SourceIndex>=frame.Count)throw new InvalidDataException("Animated vertex frame is missing a source vertex.");var p=SageExportTransform.Position(frame[v.SourceIndex],m.Center);return p-v.Position;}).ToArray(),animated.DelayMs)).ToArray();}prims.Add(new(name,mi,pass,vertices,indices,morph));
    }
   }
   meshes.Add(new(f.Name,prims));
  }
  return new(materials,meshes,false);
 }
}

namespace DDVWF.Sage.Wld;

public static class SageSceneBuilder
{
 public static SageSceneDocument Build(WldDocument doc,ReadOnlySpan<byte> source)
 {
  var materials=new List<SageSceneMaterial>();var materialIndex=new Dictionary<int,int>();
  for(var i=0;i<doc.Fragments.Count;i++)
  {
   var f=doc.Fragments[i]; if(f.KnownType!=WldFragmentType.Material)continue;
   var m=WldMaterialReader.ReadMaterial(f,source);var frames=new List<SageTexture>();var delay=0;
   if(m.BitmapInfoReferenceIndex>=0 && m.BitmapInfoReferenceIndex<doc.Fragments.Count)
   {
    var reference=doc.Fragments[m.BitmapInfoReferenceIndex];
    if(reference.KnownType==WldFragmentType.FragmentReference)
    {
     var infoIndex=WldMaterialReader.ReadReference(reference,source);
     if(infoIndex>=0 && infoIndex<doc.Fragments.Count && doc.Fragments[infoIndex].KnownType==WldFragmentType.BitmapInfo)
     {
      var info=WldMaterialReader.ReadBitmapInfo(doc.Fragments[infoIndex],source);delay=info.AnimationDelayMs;
      foreach(var bitmapIndex in info.BitmapNameIndices)
       if(bitmapIndex>=0 && bitmapIndex<doc.Fragments.Count && doc.Fragments[bitmapIndex].KnownType==WldFragmentType.BitmapName)
        frames.Add(new(WldMaterialReader.ReadBitmapName(doc.Fragments[bitmapIndex],source).FileName));
     }
    }
   }
   materialIndex[i]=materials.Count;materials.Add(new(f.Name,m.Shader,m.Brightness,m.ScaledAmbient,frames,delay));
  }

  var meshes=new List<SageSceneMesh>();
  foreach(var f in doc.Fragments.Where(x=>x.KnownType==WldFragmentType.Mesh))
  {
   var m=WldMeshReader.Read(doc,f,source);var primitives=new List<SageScenePrimitive>();var polygonOffset=0;
   if(m.MaterialListIndex>=0 && m.MaterialListIndex<doc.Fragments.Count && doc.Fragments[m.MaterialListIndex].KnownType==WldFragmentType.MaterialList)
   {
    var list=WldMaterialReader.ReadMaterialList(doc.Fragments[m.MaterialListIndex],source);
    foreach(var group in m.MaterialGroups)
    {
     var polys=m.Polygons.Skip(polygonOffset).Take(group.PolygonCount).ToArray();polygonOffset+=group.PolygonCount;
     var global=-1;if(group.MaterialIndex<list.MaterialIndices.Count)materialIndex.TryGetValue(list.MaterialIndices[group.MaterialIndex],out global);
     primitives.Add(new(global,polys));
    }
   }
   meshes.Add(new(f.Name,m.Center,m.Vertices,m.Uvs,m.Normals,primitives));
  }
  return new(materials,meshes);
 }
}

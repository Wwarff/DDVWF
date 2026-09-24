namespace DDVWF.Sage.Wld;

public static class SageSceneBuilder
{
 public static SageSceneDocument Build(WldDocument doc,ReadOnlySpan<byte> source)
 {
  var materials=new List<SageSceneMaterial>();var materialIndex=new Dictionary<int,int>();
  for(var i=0;i<doc.Fragments.Count;i++)
  {
   var f=doc.Fragments[i];if(f.KnownType!=WldFragmentType.Material)continue;SageMaterialRecord m;try{m=WldMaterialReader.ReadMaterial(f,source);}catch(Exception ex){throw FragmentFailure(doc,f,ex);}var frames=new List<SageTexture>();var delay=0;var currentFrame=0;var skipFrames=false;
   if(m.BitmapInfoReferenceIndex>=0&&m.BitmapInfoReferenceIndex<doc.Fragments.Count){var reference=doc.Fragments[m.BitmapInfoReferenceIndex];if(reference.KnownType==WldFragmentType.FragmentReference){var ii=WldMaterialReader.ReadReference(reference,source);if(ii>=0&&ii<doc.Fragments.Count&&doc.Fragments[ii].KnownType==WldFragmentType.BitmapInfo){var info=WldMaterialReader.ReadBitmapInfo(doc.Fragments[ii],source);delay=info.AnimationDelayMs;currentFrame=info.CurrentFrame;skipFrames=info.SkipFrames;foreach(var bi in info.BitmapNameIndices)if(bi>=0&&bi<doc.Fragments.Count&&doc.Fragments[bi].KnownType==WldFragmentType.BitmapName)frames.Add(new(WldMaterialReader.ReadBitmapName(doc.Fragments[bi],source).FileName));}}}
   materialIndex[i]=materials.Count;materials.Add(new(f.Name,m.Shader,m.Brightness,m.ScaledAmbient,frames,delay,currentFrame,skipFrames));
  }
  var meshes=new List<SageSceneMesh>();
  foreach(var f in doc.Fragments.Where(x=>x.KnownType==WldFragmentType.Mesh))
  {
   SageMesh m;try{m=WldMeshReader.Read(doc,f,source);}catch(Exception ex){throw FragmentFailure(doc,f,ex);}SageAnimatedVertices? animated=null;if(m.AnimatedVerticesReferenceIndex>=0&&m.AnimatedVerticesReferenceIndex<doc.Fragments.Count){var ar=doc.Fragments[m.AnimatedVerticesReferenceIndex];var ai=ar.KnownType==WldFragmentType.AnimatedVertexReference?WldMaterialReader.ReadReference(ar,source):m.AnimatedVerticesReferenceIndex;if(ai>=0&&ai<doc.Fragments.Count&&doc.Fragments[ai].KnownType==WldFragmentType.AnimatedVertices)animated=WldAnimationReader.ReadAnimatedVertices(doc.Fragments[ai],source);}var prims=new List<SageScenePrimitive>();var po=0;var primitiveByName=new Dictionary<string,(int MaterialIndex,bool PassThrough,List<SageSceneVertex> Vertices,List<ushort> Indices,Dictionary<(System.Numerics.Vector3,System.Numerics.Vector3,System.Numerics.Vector2),ushort> Dedup)>(StringComparer.OrdinalIgnoreCase);
   if(m.MaterialListIndex>=0&&m.MaterialListIndex<doc.Fragments.Count&&doc.Fragments[m.MaterialListIndex].KnownType==WldFragmentType.MaterialList)
   {
    var list=WldMaterialReader.ReadMaterialList(doc.Fragments[m.MaterialListIndex],source);
    foreach(var group in m.MaterialGroups)
    {
     if(group.MaterialIndex>=list.MaterialIndices.Count)continue;
     if(!materialIndex.TryGetValue(list.MaterialIndices[group.MaterialIndex],out var mi))continue;
     var polys=m.Polygons.Skip(po).Take(group.PolygonCount).ToArray();
     var pass=polys.Any(x=>!x.IsSolid);var baseName=materials[mi].Name+(pass?"-passthrough":"");var name=baseName;while(primitiveByName.TryGetValue(name,out var existing)&&existing.Vertices.Count*3>20000){var match=System.Text.RegularExpressions.Regex.Match(name,@"-(\d+)$");name=match.Success?System.Text.RegularExpressions.Regex.Replace(name,@"-\d+$",$"-{int.Parse(match.Groups[1].Value)+1}"):name+"-0";}if(!primitiveByName.TryGetValue(name,out var shared)){shared=(mi,pass,new List<SageSceneVertex>(),new List<ushort>(),new Dictionary<(System.Numerics.Vector3,System.Numerics.Vector3,System.Numerics.Vector2),ushort>());primitiveByName[name]=shared;}
     foreach(var poly in polys)foreach(var idx in new[]{poly.A,poly.B,poly.C}){if(idx>=m.Vertices.Count)throw new InvalidDataException("Mesh polygon references a missing vertex.");var normal=idx<m.Normals.Count?m.Normals[idx]:System.Numerics.Vector3.Zero;var uv=idx<m.Uvs.Count?m.Uvs[idx]:System.Numerics.Vector2.Zero;var v=new SageSceneVertex(SageExportTransform.Position(m.Vertices[idx],m.Center),SageExportTransform.Normal(normal),uv,idx,0xFFFFFFFF);var key=(v.Position,v.Normal,v.Uv);if(!shared.Dedup.TryGetValue(key,out var di)){if(shared.Vertices.Count>=ushort.MaxValue)throw new InvalidDataException("Sage primitive exceeds UInt16 index range.");di=(ushort)shared.Vertices.Count;shared.Dedup[key]=di;shared.Vertices.Add(v);}shared.Indices.Add(di);}po+=group.PolygonCount;
    }
   }
   foreach(var pair in primitiveByName){var shared=pair.Value;IReadOnlyList<SageSceneMorphFrame>? morph=null;if(animated is not null&&animated.Frames.Count>0){morph=animated.Frames.Select(frame=>new SageSceneMorphFrame(shared.Vertices.Select(v=>{if(v.SourceIndex<0||v.SourceIndex>=frame.Count)throw new InvalidDataException("Animated vertex frame is missing a source vertex.");var p=SageExportTransform.Position(frame[v.SourceIndex],m.Center);return p-v.Position;}).ToArray(),animated.DelayMs)).ToArray();}prims.Add(new(pair.Key,shared.MaterialIndex,shared.PassThrough,shared.Vertices,shared.Indices,morph));}
   meshes.Add(new(f.Name,prims));
  }
  return new(materials,meshes,false);
 }
 static InvalidDataException FragmentFailure(WldDocument doc,WldFragment f,Exception ex)=>new($"WLD fragment {f.Index} type 0x{f.RawType:X} ({f.KnownType?.ToString()??"Unknown"}) name '{f.Name}' in {doc.Name}: {ex.Message}",ex);
}

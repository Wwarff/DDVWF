using System.Buffers.Binary;
namespace DDVWF.Sage.Wld;
public enum SageActorType{Camera=0,Skeletal=1,Static=2,Particle=3,Sprite=4}
public sealed record SageActorDefinition(string Name,string NameReference,uint Flags,IReadOnlyList<int> FragmentReferences,SageActorType Type,int? ResolvedModelFragment);
public static class WldActorDefinitionReader
{
 public static SageActorDefinition Read(WldDocument doc,WldFragment f,ReadOnlySpan<byte> source)
 {
  var b=source.ToArray();var p=f.PayloadOffset;void N(int n){if(p+n>b.Length)throw new InvalidDataException("ActorDef is truncated.");}uint U(){N(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}int I()=>unchecked((int)U());float F(){var v=I();return BitConverter.Int32BitsToSingle(v);}
  var flags=U();var nameRef=doc.ResolveString(I());var actionCount=U();var refCount=checked((int)U());_=U();if((flags&1)!=0)_=U();if((flags&2)!=0){for(var i=0;i<6;i++)_=F();_=U();}var lod=checked((int)U());_=U();for(var i=0;i<lod;i++)_=F();var refs=new List<int>(refCount);for(var i=0;i<refCount;i++)refs.Add(I()-1);
  var type=SageActorType.Camera;int? model=null;
  foreach(var ri in refs){if(ri<0||ri>=doc.Fragments.Count)continue;var rf=doc.Fragments[ri];if(rf.KnownType==WldFragmentType.MeshReference){var target=WldMaterialReader.ReadReference(rf,source);if(target>=0&&target<doc.Fragments.Count){type=SageActorType.Static;model=target;break;}}if(rf.KnownType==WldFragmentType.SkeletonHierarchyReference){type=SageActorType.Skeletal;model=WldMaterialReader.ReadReference(rf,source);break;}}
  return new(f.Name,nameRef,flags,refs,type,model);
 }
}

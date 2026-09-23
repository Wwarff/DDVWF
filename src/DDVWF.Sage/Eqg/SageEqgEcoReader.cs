using System.Globalization;
using System.Text;
namespace DDVWF.Sage.Eqg;
// EQ Sage v1.8.15 src/lib/eqg/eco/eco.js fields consumed by V4 export.
public sealed record SageEqgEcoTexture(string Name,string CoverMap,string BlendMap,float BlendSoftness,string LayeringMap,float LayeringArea,string DetailMap,string NormalMap,float DetailRepeat,float NormalRepeat,float MinHeight,float MaxHeight,float HeightTol,float MinSlope,float MaxSlope,float SlopeTol);
public static class SageEqgEcoReader
{
 public static IReadOnlyList<SageEqgEcoTexture> Read(ReadOnlySpan<byte> source)
 {
  var result=new List<SageEqgEcoTexture>();var type="";string name="",cover="",blend="",layering="",detail="",normal="";float blendSoft=0,layerArea=0,detailRepeat=0,normalRepeat=0,minH=0,maxH=0,hTol=0,minS=0,maxS=0,sTol=0;
  static float F(string s)=>float.Parse(s,CultureInfo.InvariantCulture);
  foreach(var raw in Encoding.UTF8.GetString(source).Split('*')){var p=raw.Trim().Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries);if(p.Length==0)continue;switch(p[0]){
   case "TEXTUREPART":type="texture";break;case "OBJECTPART":type="object";break;case "FLORAPART":type="flora";break;
   case "LAYER":name=p[1];cover=blend=layering=detail=normal="";blendSoft=layerArea=detailRepeat=normalRepeat=minH=maxH=hTol=minS=maxS=sTol=0;break;
   case "MINHEIGHT":minH=F(p[1]);break;case "MAXHEIGHT":maxH=F(p[1]);break;case "HEIGHTTOL":hTol=F(p[1]);break;case "MINSLOPE":minS=F(p[1]);break;case "MAXSLOPE":maxS=F(p[1]);break;case "SLOPETOL":sTol=F(p[1]);break;
   case "COVERMAP":cover=p[1];break;case "BLENDMAP":blend=p[1];break;case "BLENDSOFTNESS":blendSoft=F(p[1]);break;case "LAYERINGMAP":layering=p[1];break;case "LAYERINGAREA":layerArea=F(p[1]);break;case "DETAILMAP":detail=p[1];break;case "NORMALMAP":normal=p[1];break;case "DETAILREPEAT":detailRepeat=F(p[1]);break;case "NORMALREPEAT":normalRepeat=F(p[1]);break;
   case "END_LAYER" when type=="texture":result.Add(new(name,cover,blend,blendSoft,layering,layerArea,detail,normal,detailRepeat,normalRepeat,minH,maxH,hTol,minS,maxS,sTol));break;
  }}
  return result;
 }
}

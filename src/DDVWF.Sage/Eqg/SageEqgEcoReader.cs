using System.Globalization;
using System.Text;
namespace DDVWF.Sage.Eqg;
// EQ Sage v1.8.15 src/lib/eqg/eco/eco.js fields consumed by V4 export.
public sealed record SageEqgEcoObject(string Name,string Object,string TextureLayer,float Density,int Iterations,float MinScale,float MaxScale,float MinHeight,float MaxHeight,float HeightTol,float MinSlope,float MaxSlope,float SlopeTol);
public sealed record SageEqgEcoFlora(string Name,string Flora,string TextureLayer,float Density,int Iterations,float MinScale,float MaxScale,float MinAlpha,float MinHeight,float MaxHeight,float HeightTol,float MinSlope,float MaxSlope,float SlopeTol);
public sealed record SageEqgEcoData(IReadOnlyList<SageEqgEcoTexture> TextureLayers,IReadOnlyList<SageEqgEcoObject> ObjectLayers,IReadOnlyList<SageEqgEcoFlora> FloraLayers);
public sealed record SageEqgEcoTexture(string Name,string CoverMap,string BlendMap,float BlendSoftness,string LayeringMap,float LayeringArea,string DetailMap,string NormalMap,float DetailRepeat,float NormalRepeat,float MinHeight,float MaxHeight,float HeightTol,float MinSlope,float MaxSlope,float SlopeTol);
public static class SageEqgEcoReader
{
 public static SageEqgEcoData Read(ReadOnlySpan<byte> source)
 {
  var result=new List<SageEqgEcoTexture>();var objects=new List<SageEqgEcoObject>();var flora=new List<SageEqgEcoFlora>();var type="";string name="",cover="",blend="",layering="",detail="",normal="",objectName="",textureLayer="",floraName="";float blendSoft=0,layerArea=0,detailRepeat=0,normalRepeat=0,minH=0,maxH=0,hTol=0,minS=0,maxS=0,sTol=0,density=0,minScale=0,maxScale=0,minAlpha=0;int iterations=0;
  static float F(string s)=>float.Parse(s,CultureInfo.InvariantCulture);
  foreach(var raw in Encoding.UTF8.GetString(source).Split('*')){var p=raw.Trim().Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries);if(p.Length==0)continue;switch(p[0]){
   case "TEXTUREPART":type="texture";break;case "OBJECTPART":type="object";break;case "FLORAPART":type="flora";break;
   case "LAYER":name=p[1];cover=blend=layering=detail=normal=objectName=textureLayer=floraName="";blendSoft=layerArea=detailRepeat=normalRepeat=minH=maxH=hTol=minS=maxS=sTol=density=minScale=maxScale=minAlpha=0;iterations=0;break;
   case "MINHEIGHT":minH=F(p[1]);break;case "MAXHEIGHT":maxH=F(p[1]);break;case "HEIGHTTOL":hTol=F(p[1]);break;case "MINSLOPE":minS=F(p[1]);break;case "MAXSLOPE":maxS=F(p[1]);break;case "SLOPETOL":sTol=F(p[1]);break;
   case "COVERMAP":cover=p[1];break;case "BLENDMAP":blend=p[1];break;case "BLENDSOFTNESS":blendSoft=F(p[1]);break;case "LAYERINGMAP":layering=p[1];break;case "LAYERINGAREA":layerArea=F(p[1]);break;case "DETAILMAP":detail=p[1];break;case "NORMALMAP":normal=p[1];break;case "DETAILREPEAT":detailRepeat=F(p[1]);break;case "NORMALREPEAT":normalRepeat=F(p[1]);break;case "OBJECT":objectName=p[1];break;case "FLORA":floraName=p[1];break;case "TEXTURELAYER":textureLayer=p[1];break;case "DENSITY":density=F(p[1]);break;case "ITERATIONS":iterations=(int)F(p[1]);break;case "MINSCALE":minScale=F(p[1]);break;case "MAXSCALE":maxScale=F(p[1]);break;case "MINALPHA":minAlpha=F(p[1]);break;
   case "END_LAYER" when type=="texture":result.Add(new(name,cover,blend,blendSoft,layering,layerArea,detail,normal,detailRepeat,normalRepeat,minH,maxH,hTol,minS,maxS,sTol));break;case "END_LAYER" when type=="object":objects.Add(new(name,objectName,textureLayer,density,iterations,minScale,maxScale,minH,maxH,hTol,minS,maxS,sTol));break;case "END_LAYER" when type=="flora":flora.Add(new(name,floraName,textureLayer,density,iterations,minScale,maxScale,minAlpha,minH,maxH,hTol,minS,maxS,sTol));break;
  }}
  return new(result,objects,flora);
 }
}

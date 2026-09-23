using System.Globalization;
using System.Text;
namespace DDVWF.Sage.Eqg;

// Exact Sage v1.8.15 src/lib/eqg/zone/v4-zone.js text header semantics.
public sealed record SageEqgV4Header(int Version,int MinLng,int MaxLng,int MinLat,int MaxLat,float UnitsPerVert,int QuadsPerTile,int CoverMapInputSize,int LayeringMapInputSize,IReadOnlyList<float> MinExtents,IReadOnlyList<float> MaxExtents);
public static class SageEqgV4HeaderReader
{
 public static SageEqgV4Header Read(string name,ReadOnlySpan<byte> source)
 {
  var text=Encoding.UTF8.GetString(source);if(!text.TrimStart().StartsWith("EQTZP",StringComparison.Ordinal))throw new InvalidDataException($"EQG V4 zone '{name}' does not start with EQTZP.");
  int version=0,minLng=0,maxLng=0,minLat=0,maxLat=0,quads=0,cover=0,layering=0;float units=0;var minExt=Array.Empty<float>();var maxExt=Array.Empty<float>();
  static float F(string s)=>float.Parse(s,CultureInfo.InvariantCulture);static int I(string s)=>int.Parse(s,CultureInfo.InvariantCulture);
  foreach(var raw in text.Split('*')){var parts=raw.Trim().Split(' ',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);if(parts.Length==0)continue;switch(parts[0]){
   case "EQTZP":break;case "NAME":break;case "MINLNG":minLng=I(parts[1]);break;case "MAXLNG":maxLng=I(parts[1]);break;case "MINLAT":minLat=I(parts[1]);break;case "MAXLAT":maxLat=I(parts[1]);break;
   case "MIN_EXTENTS":minExt=parts.Skip(1).Select(F).ToArray();break;case "MAX_EXTENTS":maxExt=parts.Skip(1).Select(F).ToArray();break;case "UNITSPERVERT":units=F(parts[1]);break;case "QUADSPERTILE":quads=I(parts[1]);break;case "COVERMAPINPUTSIZE":cover=I(parts[1]);break;case "LAYERINGMAPINPUTSIZE":layering=I(parts[1]);break;case "VERSION":version=I(parts[1]);break;
   default:break; // Sage warns and continues for unknown V4 header attributes.
  }}
  if(units<=0||quads<=0)throw new InvalidDataException($"EQG V4 zone '{name}' has invalid UNITSPERVERT/QUADSPERTILE.");
  return new(version,minLng,maxLng,minLat,maxLat,units,quads,cover,layering,minExt,maxExt);
 }
}

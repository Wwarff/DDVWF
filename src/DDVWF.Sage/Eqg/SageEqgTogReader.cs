using System.Globalization;

namespace DDVWF.Sage.Eqg;

// Exact parser for Sage v1.8.15 src/lib/eqg/tog/tog.js consumed fields.
public sealed record SageEqgTogPlaceable(string ModelName,float X,float Y,float Z,float RotateX,float RotateY,float RotateZ,float Scale);
public static class SageEqgTogReader
{
 public static IReadOnlyList<SageEqgTogPlaceable> Read(string name,ReadOnlySpan<byte> source)
 {
  var text=System.Text.Encoding.UTF8.GetString(source);
  var result=new List<SageEqgTogPlaceable>();
  string model="";float x=0,y=0,z=0,rx=0,ry=0,rz=0,scale=1;var active=false;
  static float F(string s)=>float.Parse(s,CultureInfo.InvariantCulture);
  foreach(var raw in text.Split('*'))
  {
   var parts=raw.Trim().Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries);if(parts.Length==0)continue;
   switch(parts[0])
   {
    case "BEGIN_OBJECT": active=true;model="";x=y=z=rx=ry=rz=0;scale=1;break;
    case "NAME" when active&&parts.Length>1:model=parts[1];break;
    case "POSITION" when active&&parts.Length>3:x=F(parts[1]);y=-F(parts[2]);z=F(parts[3]);break;
    case "ROTATION" when active&&parts.Length>3:rx=F(parts[1]);ry=F(parts[2]);rz=F(parts[3]);break;
    case "SCALE" when active&&parts.Length>1:scale=F(parts[1]);break;
    case "END_OBJECT" when active:result.Add(new(model,x,y,z,rx,ry,rz,scale));active=false;break;
   }
  }
  return result;
 }
}

using System.Buffers.Binary;
using System.Text;
namespace DDVWF.Sage.Eqg;

// Direct implementation of EQ Sage v1.8.15 ZoneData in src/lib/eqg/zone/v4-zone.js.
public sealed record SageEqgV4Tile(float X,float Y,IReadOnlyList<float> Heights,IReadOnlyList<uint> Colors,IReadOnlyList<uint> Colors2,IReadOnlyList<byte> Flags,float BaseWaterLevel,string BaseMaterial,string Material);
public sealed record SageEqgV4Placeable(string ModelName,float X,float Y,float Z,float RotateX,float RotateY,float RotateZ,float ScaleX,float ScaleY,float ScaleZ);
public sealed record SageEqgV4Region(string Name,string AltName,int Type,float X,float Y,float Z,float RotateX,float RotateY,float RotateZ,float ScaleX,float ScaleY,float ScaleZ,float ExtX,float ExtY,float ExtZ);
public sealed record SageEqgV4TogReference(string Name,float X,float Y,float Z,float RotateX,float RotateY,float RotateZ,float ScaleX,float ScaleY,float ScaleZ,float ZAdjust);
public sealed record SageEqgV4Data(IReadOnlyList<SageEqgV4Tile> Tiles,IReadOnlyList<SageEqgV4Placeable> Placeables,IReadOnlyList<SageEqgV4Region> Regions,IReadOnlyList<SageEqgV4TogReference> TogReferences);

public static class SageEqgV4DataReader
{
 public static SageEqgV4Data Read(string name,ReadOnlySpan<byte> source,SageEqgV4Header h)
 {
  var b=source.ToArray();var p=0;void Need(int n){if(n<0||p+n>b.Length)throw new InvalidDataException($"EQG V4 data '{name}' is truncated.");}
  uint U(){Need(4);var v=BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p,4));p+=4;return v;}int I()=>unchecked((int)U());float F()=>BitConverter.Int32BitsToSingle(I());byte B(){Need(1);return b[p++];}
  string S(){var s=p;while(p<b.Length&&b[p]!=0)p++;Need(1);var v=Encoding.UTF8.GetString(b,s,p-s);p++;return v;}
  var unk000=U();_=U();_=U();_=S();var tileCount=checked((int)U());var q=h.QuadsPerTile;var units=h.UnitsPerVert;var quadCount=checked(q*q);var vertCount=checked((q+1)*(q+1));var zoneMinX=h.MinLat*q*units;var zoneMinY=h.MinLng*q*units;
  var tiles=new List<SageEqgV4Tile>(tileCount);var placeables=new List<SageEqgV4Placeable>();var regions=new List<SageEqgV4Region>();var togs=new List<SageEqgV4TogReference>();
  for(var ti=0;ti<tileCount;ti++){
   var tileLng=U();var tileLat=U();_=U();var tileStartY=zoneMinY+((long)tileLng-100000-h.MinLng)*units*q;var tileStartX=zoneMinX+((long)tileLat-100000-h.MinLat)*units*q;
   var heights=new float[vertCount];for(var i=0;i<vertCount;i++)heights[i]=F();var colors=new uint[vertCount];for(var i=0;i<vertCount;i++)colors[i]=U();var colors2=new uint[vertCount];for(var i=0;i<vertCount;i++)colors2[i]=U();var flags=new byte[quadCount];for(var i=0;i<quadCount;i++)flags[i]=B();var water=F();var unk1=I();if(unk1>0){var ub=B();if(ub>0)for(var i=0;i<4;i++)_=F();_=F();}
   var layerCount=checked((int)U());var baseMaterial=S();var material="";for(var layer=1;layer<layerCount;layer++){material=S();var dim=checked((int)U());Need(checked(dim*dim));p+=dim*dim;}
   tiles.Add(new(tileStartX,tileStartY,heights,colors,colors2,flags,water,baseMaterial,material));
   var single=checked((int)U());for(var i=0;i<single;i++){var model=S().ToLowerInvariant();_=S();_=U();_=U();var x=F();var y=F();var z=F();var rx=F();var ry=F();var rz=F();var sx=F();var sy=F();var sz=F();_=B();if((unk000&2)!=0)_=U();var th=Height(heights,q,units,x,y);placeables.Add(new(model,x+tileStartY,y+tileStartX,z+th,rx,ry,rz,sx,sy,sz));}
   var areas=checked((int)U());for(var i=0;i<areas;i++){var n=S();var type=I();var alt=S();_=U();_=U();var x=F();var y=F();var z=F();var rx=F();var ry=F();var rz=F();var sx=F();var sy=F();var sz=F();var sizeX=F();var sizeY=F();var sizeZ=F();var th=Height(heights,q,units,x,y);regions.Add(new(n,alt,type,x+tileStartY,y+tileStartX,z+th,rx,ry,rz,sx,sy,sz,sizeX/2,sizeY/2,sizeZ/2));}
   var lightFx=checked((int)U());for(var i=0;i<lightFx;i++){_=S();_=S();_=B();_=U();_=U();for(var j=0;j<10;j++)_=F();}
   var tc=checked((int)U());for(var i=0;i<tc;i++){var n=S();_=U();_=U();var x=F();var y=F();var z=F();var rx=F();var ry=F();var rz=F();var sx=F();var sy=F();var sz=F();var za=F();togs.Add(new(n,x+tileStartY,y+tileStartX,z+(sz+za),rx,ry,rz,sx,sy,sz,za));}
  }
  return new(tiles,placeables,regions,togs);
 }
 static float Height(IReadOnlyList<float> heights,int q,float units,float x,float y){var grid=units*q;static float Mod(float a,float b)=>a-MathF.Floor(a/b)*b;var ax=x<0?x+(-x/grid+1)*grid:Mod(x,grid);var ay=y<0?y+(-y/grid+1)*grid:Mod(y,grid);var row=(int)MathF.Floor(ay/units);var col=(int)MathF.Floor(ax/units);row=Math.Clamp(row,0,q-1);col=Math.Clamp(col,0,q-1);var quad=row*q+col;var z1=heights[quad+row];var z2=heights[quad+row+q+1];var z3=heights[quad+row+q+2];var z4=heights[quad+row+1];var lx=row*units;var ly=(quad%q)*units;return PlaneHeight(lx,ly,z1,lx+units,ly,z2,lx+units,ly+units,z3,lx,ly+units,z4,ay,ax);}
 static float PlaneHeight(float x1,float y1,float z1,float x2,float y2,float z2,float x3,float y3,float z3,float x4,float y4,float z4,float x,float y){var first=Inside(x1,y1,x2,y2,x3,y3,x,y);return first?Tri(x1,y1,z1,x2,y2,z2,x3,y3,z3,x,y):Tri(x1,y1,z1,x3,y3,z3,x4,y4,z4,x,y);}
 static bool Inside(float ax,float ay,float bx,float by,float cx,float cy,float x,float y){var ab=(y-ay)*(bx-ax)-(x-ax)*(by-ay);var bc=(y-by)*(cx-bx)-(x-bx)*(cy-by);var ca=(y-cy)*(ax-cx)-(x-cx)*(ay-cy);return ab*bc>=0&&bc*ca>=0;}
 static float Tri(float ax,float ay,float az,float bx,float by,float bz,float cx,float cy,float cz,float x,float y){var nx=(by-ay)*(cz-az)-(bz-az)*(cy-ay);var ny=(bz-az)*(cx-ax)-(bx-ax)*(cz-az);var nz=(bx-ax)*(cy-ay)-(by-ay)*(cx-ax);var len=MathF.Sqrt(nx*nx+ny*ny+nz*nz);nx/=len;ny/=len;nz/=len;return (nx*(x-ax)+ny*(y-ay))/-nz+az;}
}

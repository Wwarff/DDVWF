using DDVWF.Core.Zone;
namespace DDVWF.Core.Tests;
public class EqEmuCollisionMapTests
{
 [Fact]
 public void V1_FindBestZ_matches_EQEmu_down_then_up_filters()
 {
  var path=Path.GetTempFileName();
  try
  {
   using(var f=File.Create(path))using(var w=new BinaryWriter(f))
   {
    w.Write(0x01000000u);w.Write(2u);w.Write((ushort)0);w.Write(0u);
    Tri(w,0,0,5,10,0,5,0,10,5);Tri(w,0,0,15,0,10,15,10,0,15);
   }
   var map=EqEmuCollisionMap.Load(path);
   Assert.Equal(5f,map.FindBestZ(2,2,10));
   Assert.Equal(15f,map.FindBestZ(2,2,4));
   Assert.Equal(EqEmuCollisionMap.BestZInvalid,map.FindBestZ(2,2,10,underworld:6,maxZ:14));
   Assert.Equal(15f,map.FindBestZ(2,2,4,maxZ:15));
  }
  finally{File.Delete(path);}
 }
 [Fact]
 public void V2_inflation_and_base_collision_match_EQEmu()
 {
  using var raw=new MemoryStream();using(var w=new BinaryWriter(raw,System.Text.Encoding.UTF8,true))
  {
   w.Write(3u);w.Write(3u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0f);
   V(w,0,0,7);V(w,10,0,7);V(w,0,10,7);w.Write(0u);w.Write(1u);w.Write(2u);
  }
  raw.Position=0;using var compressed=new MemoryStream();using(var z=new System.IO.Compression.ZLibStream(compressed,System.IO.Compression.CompressionLevel.SmallestSize,true))raw.CopyTo(z);
  var path=Path.GetTempFileName();try{using(var f=File.Create(path))using(var w=new BinaryWriter(f)){w.Write(0x02000000u);w.Write((uint)compressed.Length);w.Write((uint)raw.Length);w.Write(compressed.ToArray());}var map=EqEmuCollisionMap.Load(path);Assert.Equal(7f,map.FindBestZ(2,2,20));}finally{File.Delete(path);}
 }
 static void Tri(BinaryWriter w,float ax,float ay,float az,float bx,float by,float bz,float cx,float cy,float cz){V(w,ax,ay,az);V(w,bx,by,bz);V(w,cx,cy,cz);for(int i=0;i<4;i++)w.Write(0f);}
 static void V(BinaryWriter w,float x,float y,float z){w.Write(x);w.Write(y);w.Write(z);}
}

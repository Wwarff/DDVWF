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
 public void FindBestZ_does_not_hit_beyond_EQEmu_segment_endpoint()
 {
  var path=Path.GetTempFileName();try{using(var f=File.Create(path))using(var w=new BinaryWriter(f)){w.Write(0x01000000u);w.Write(1u);w.Write((ushort)0);w.Write(0u);Tri(w,0,0,-100500,10,0,-100500,0,10,-100500);}var map=EqEmuCollisionMap.Load(path);Assert.Equal(EqEmuCollisionMap.BestZInvalid,map.FindBestZ(2,2,0));}finally{File.Delete(path);}
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
 [Fact]
 public void V2_flat_terrain_participates_in_collision()
 {
  using var raw=new MemoryStream();using(var w=new BinaryWriter(raw,System.Text.Encoding.UTF8,true)){w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(1u);w.Write(1u);w.Write(10f);w.Write(true);w.Write(0f);w.Write(0f);w.Write(9f);}raw.Position=0;using var compressed=new MemoryStream();using(var z=new System.IO.Compression.ZLibStream(compressed,System.IO.Compression.CompressionLevel.SmallestSize,true))raw.CopyTo(z);var path=Path.GetTempFileName();try{using(var f=File.Create(path))using(var w=new BinaryWriter(f)){w.Write(0x02000000u);w.Write((uint)compressed.Length);w.Write((uint)raw.Length);w.Write(compressed.ToArray());}Assert.Equal(9f,EqEmuCollisionMap.Load(path).FindBestZ(5,5,20));}finally{File.Delete(path);}
 }
 [Fact]
 public void V2_model_placement_uses_EQEmu_transform_and_xy_swap()
 {
  using var raw=new MemoryStream();using(var w=new BinaryWriter(raw,System.Text.Encoding.UTF8,true)){w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(1u);w.Write(1u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0f);w.Write((byte)'m');w.Write((byte)0);w.Write(3u);w.Write(1u);V(w,0,0,4);V(w,10,0,4);V(w,0,10,4);w.Write(0u);w.Write(1u);w.Write(2u);w.Write((byte)1);w.Write((byte)'m');w.Write((byte)0);V(w,20,30,0);V(w,0,0,0);V(w,1,1,1);}raw.Position=0;using var compressed=new MemoryStream();using(var z=new System.IO.Compression.ZLibStream(compressed,System.IO.Compression.CompressionLevel.SmallestSize,true))raw.CopyTo(z);var path=Path.GetTempFileName();try{using(var f=File.Create(path))using(var w=new BinaryWriter(f)){w.Write(0x02000000u);w.Write((uint)compressed.Length);w.Write((uint)raw.Length);w.Write(compressed.ToArray());}Assert.Equal(4f,EqEmuCollisionMap.Load(path).FindBestZ(31,21,20));}finally{File.Delete(path);}
 }
 [Fact]
 public void V2_placement_group_rotation_matches_EQEmu_sequence()
 {
  using var raw=new MemoryStream();using(var w=new BinaryWriter(raw,System.Text.Encoding.UTF8,true))
  {
   w.Write(0u);w.Write(0u);w.Write(0u);w.Write(0u);w.Write(1u);w.Write(0u);w.Write(1u);w.Write(0u);w.Write(0u);w.Write(0f);
   w.Write((byte)'m');w.Write((byte)0);w.Write(3u);w.Write(1u);
   V(w,0,0,0);V(w,10,0,0);V(w,0,10,0);w.Write(0u);w.Write(1u);w.Write(2u);w.Write((byte)1);
   V(w,0,0,0);V(w,0,45,0);V(w,1,1,1);V(w,0,0,0);w.Write(1u);
   w.Write((byte)'m');w.Write((byte)0);V(w,20,0,4);V(w,0,0,0);V(w,1,1,1);
  }
  raw.Position=0;using var compressed=new MemoryStream();using(var z=new System.IO.Compression.ZLibStream(compressed,System.IO.Compression.CompressionLevel.SmallestSize,true))raw.CopyTo(z);
  var path=Path.GetTempFileName();
  try
  {
   using(var f2=File.Create(path))using(var w2=new BinaryWriter(f2)){w2.Write(0x02000000u);w2.Write((uint)compressed.Length);w2.Write((uint)raw.Length);w2.Write(compressed.ToArray());}
   var map=EqEmuCollisionMap.Load(path);
   Assert.InRange(map.FindBestZ(1,18,20),-12.344f,-12.342f);
  }
  finally{File.Delete(path);}
 }
 static void Tri(BinaryWriter w,float ax,float ay,float az,float bx,float by,float bz,float cx,float cy,float cz){V(w,ax,ay,az);V(w,bx,by,bz);V(w,cx,cy,cz);for(int i=0;i<4;i++)w.Write(0f);}
 static void V(BinaryWriter w,float x,float y,float z){w.Write(x);w.Write(y);w.Write(z);}
}

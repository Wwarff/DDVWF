using DDVWF.Sage.Wld;
namespace DDVWF.Core.Tests;
public sealed class SageTextureProcessorTests
{
 [Fact] public void Pfim_Rgba16_A4R4G4B4_Dds_is_processed()
 {
  if(!OperatingSystem.IsWindows())return;
  var dds=BuildA4R4G4B4(0xF123);
  var png=SageTextureProcessor.Process("rgba16.dds",dds,SageShaderType.Diffuse);
  Assert.True(png.Length>8);
  Assert.Equal(new byte[]{0x89,0x50,0x4e,0x47},png[..4]);
 }
 static byte[] BuildA4R4G4B4(ushort pixel)
 {
  var b=new byte[130];
  void U(int o,uint v)=>System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(o,4),v);
  U(0,0x20534444);U(4,124);U(8,0x100F);U(12,1);U(16,1);U(20,2);
  U(76,32);U(80,0x41);U(88,16);U(92,0x0F00);U(96,0x00F0);U(100,0x000F);U(104,0xF000);U(108,0x1000);
  System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(128,2),pixel);return b;
 }
}

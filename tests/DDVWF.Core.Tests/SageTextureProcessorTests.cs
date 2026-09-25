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
 [Fact] public void TransparentMasked_Bmp_preserves_non_mask_pixel_alpha()
 {
  if(!OperatingSystem.IsWindows())return;
  using var source=new System.Drawing.Bitmap(2,1,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
  source.SetPixel(0,0,System.Drawing.Color.FromArgb(255,255,0,0));source.SetPixel(1,0,System.Drawing.Color.FromArgb(255,0,20,0));
  using var input=new MemoryStream();source.Save(input,System.Drawing.Imaging.ImageFormat.Bmp);
  var png=SageTextureProcessor.Process("mask.bmp",input.ToArray(),SageShaderType.TransparentMasked);
  using var image=new System.Drawing.Bitmap(new MemoryStream(png,false));
  Assert.Equal(0,image.GetPixel(0,0).A);Assert.Equal(255,image.GetPixel(1,0).A);
 }
 [Fact] public void TransparentMasked_Dds_samples_mask_before_Sage_vertical_flip()
 {
  if(!OperatingSystem.IsWindows())return;
  var dds=BuildA4R4G4B4(1,2,0xFF00,0xF0F0);
  var png=SageTextureProcessor.Process("mask.dds",dds,SageShaderType.TransparentMasked);
  using var image=new System.Drawing.Bitmap(new MemoryStream(png,false));
  Assert.Equal(255,image.GetPixel(0,0).A);
  Assert.Equal(0,image.GetPixel(0,1).A);
 }
 static byte[] BuildA4R4G4B4(ushort pixel)=>BuildA4R4G4B4(1,1,pixel);
 static byte[] BuildA4R4G4B4(int width,int height,params ushort[] pixels)
 {
  var b=new byte[128+pixels.Length*2];
  void U(int o,uint v)=>System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(o,4),v);
  U(0,0x20534444);U(4,124);U(8,0x100F);U(12,(uint)height);U(16,(uint)width);U(20,(uint)(width*2));
  U(76,32);U(80,0x41);U(88,16);U(92,0x0F00);U(96,0x00F0);U(100,0x000F);U(104,0xF000);U(108,0x1000);
  for(var i=0;i<pixels.Length;i++)System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(128+i*2,2),pixels[i]);return b;
 }
}

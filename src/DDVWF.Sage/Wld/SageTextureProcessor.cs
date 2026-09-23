using System.Drawing;using System.Drawing.Imaging;using System.Runtime.InteropServices;
namespace DDVWF.Sage.Wld;
public static class SageTextureProcessor
{
 public static byte[] Process(string name,byte[] data,SageShaderType shader)
 {
  if(!OperatingSystem.IsWindowsVersionAtLeast(6,1))throw new PlatformNotSupportedException("DDVWF native texture processing requires Windows.");
  if(data.Length<2)throw new InvalidDataException("Texture is truncated.");
  var bmp=data[0]==0x42&&data[1]==0x4d;var png=data.Length>=8&&data[0]==0x89&&data[1]==0x50&&data[2]==0x4e&&data[3]==0x47;
  using var image=bmp||png?new Bitmap(new MemoryStream(data,false)):DecodeDds(data);
  if(!bmp&&!png)image.RotateFlip(RotateFlipType.RotateNoneFlipY);
  Color? mask=shader==SageShaderType.TransparentMasked?image.GetPixel(0,0):null;
  using var output=new Bitmap(image.Width,image.Height,PixelFormat.Format32bppArgb);
  for(var y=0;y<image.Height;y++)for(var x=0;x<image.Width;x++){var p=image.GetPixel(x,y);var a=Alpha(shader,p,mask,bmp||png);output.SetPixel(x,y,Color.FromArgb(a,p.R,p.G,p.B));}
  using var ms=new MemoryStream();output.Save(ms,ImageFormat.Png);return ms.ToArray();
 }
 static int Alpha(SageShaderType shader,Color p,Color? mask,bool bmp)
 {
  if(shader==SageShaderType.TransparentMasked&&mask is Color m&&p.R==m.R&&p.G==m.G&&p.B==m.B&&p.A==m.A)return 0;
  var mapped=shader switch{SageShaderType.Transparent25=>64,SageShaderType.Transparent50 or SageShaderType.TransparentSkydome=>128,SageShaderType.Transparent75 or SageShaderType.TransparentAdditive or SageShaderType.TransparentAdditiveUnlit=>192,_=>-1};
  if(mapped>=0)return mapped;
  if(!bmp)return p.A;
  if(shader==SageShaderType.Diffuse)return 255;
  var max=Math.Max(p.R,Math.Max(p.G,p.B));return max<=64?max:Math.Min(max+(max-64)*2,255);
 }
 static Bitmap DecodeDds(byte[] data)
 {
  if(!OperatingSystem.IsWindowsVersionAtLeast(6,1))throw new PlatformNotSupportedException("DDVWF native DDS processing requires Windows.");
  using var ms=new MemoryStream(data,false);using var image=Pfim.Pfimage.FromStream(ms);var format=image.Format switch{Pfim.ImageFormat.Rgba32=>PixelFormat.Format32bppArgb,Pfim.ImageFormat.Rgb24=>PixelFormat.Format24bppRgb,Pfim.ImageFormat.R5g5b5=>PixelFormat.Format16bppRgb555,Pfim.ImageFormat.R5g6b5=>PixelFormat.Format16bppRgb565,Pfim.ImageFormat.R5g5b5a1=>PixelFormat.Format16bppArgb1555,_=>throw new InvalidDataException($"Unsupported DDS pixel format {image.Format}.")};var handle=GCHandle.Alloc(image.Data,GCHandleType.Pinned);try{using var view=new Bitmap(image.Width,image.Height,image.Stride,format,handle.AddrOfPinnedObject());return new Bitmap(view);}finally{handle.Free();}}
}
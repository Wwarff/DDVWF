using DDVWF.Sage.Eqg;
namespace DDVWF.Core.Tests;
public sealed class EqgV4Tests
{
 [Fact] public void HeaderMatchesPinnedSageTextSemantics(){var h=SageEqgV4HeaderReader.Read("x.zon",System.Text.Encoding.UTF8.GetBytes("EQTZP*MINLNG 1*MAXLNG 2*MINLAT 3*MAXLAT 4*MIN_EXTENTS -1 -2 -3*MAX_EXTENTS 1 2 3*UNITSPERVERT 2.5*QUADSPERTILE 8*COVERMAPINPUTSIZE 16*LAYERINGMAPINPUTSIZE 32*VERSION 4*"));Assert.Equal(4,h.Version);Assert.Equal(2.5f,h.UnitsPerVert);Assert.Equal(8,h.QuadsPerTile);Assert.Equal(new float[]{-1,-2,-3},h.MinExtents);}
 [Fact] public void EcoTextureLayerMatchesPinnedSageFields(){var e=SageEqgEcoReader.Read(System.Text.Encoding.UTF8.GetBytes("TEXTUREPART*LAYER grass*MINHEIGHT 1*MAXHEIGHT 2*DETAILMAP grass.dds*NORMALMAP grassn.dds*DETAILREPEAT 4*NORMALREPEAT 5*END_LAYER*END_TEXTUREPART*"));var x=Assert.Single(e);Assert.Equal("grass",x.Name);Assert.Equal("grass.dds",x.DetailMap);Assert.Equal(4,x.DetailRepeat);}
}

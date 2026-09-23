using DDVWF.Sage.Eqg;
namespace DDVWF.Core.Tests;
public sealed class EqgV4Tests
{
 [Fact] public void HeaderMatchesPinnedSageTextSemantics(){var h=SageEqgV4HeaderReader.Read("x.zon",System.Text.Encoding.UTF8.GetBytes("EQTZP*MINLNG 1*MAXLNG 2*MINLAT 3*MAXLAT 4*MIN_EXTENTS -1 -2 -3*MAX_EXTENTS 1 2 3*UNITSPERVERT 2.5*QUADSPERTILE 8*COVERMAPINPUTSIZE 16*LAYERINGMAPINPUTSIZE 32*VERSION 4*"));Assert.Equal(4,h.Version);Assert.Equal(2.5f,h.UnitsPerVert);Assert.Equal(8,h.QuadsPerTile);Assert.Equal(new float[]{-1,-2,-3},h.MinExtents);}
 [Fact] public void EcoTextureLayerMatchesPinnedSageFields(){var e=SageEqgEcoReader.Read(System.Text.Encoding.UTF8.GetBytes("TEXTUREPART*LAYER grass*MINHEIGHT 1*MAXHEIGHT 2*DETAILMAP grass.dds*NORMALMAP grassn.dds*DETAILREPEAT 4*NORMALREPEAT 5*END_LAYER*END_TEXTUREPART*"));var x=Assert.Single(e.TextureLayers);Assert.Equal("grass",x.Name);Assert.Equal("grass.dds",x.DetailMap);Assert.Equal(4,x.DetailRepeat);}
 [Fact] public void EcoObjectAndFloraLayersMatchPinnedSageFields(){var e=SageEqgEcoReader.Read(System.Text.Encoding.UTF8.GetBytes("OBJECTPART*LAYER rocks*OBJECT rock.mod*TEXTURELAYER stone*DENSITY 2.5*ITERATIONS 3*MINSCALE .5*MAXSCALE 1.5*END_LAYER*END_OBJECTPART*FLORAPART*LAYER grass*FLORA blade.mod*TEXTURELAYER green*DENSITY 4*ITERATIONS 2*MINSCALE .25*MAXSCALE 1*MINALPHA .6*END_LAYER*END_FLORAPART*"));var o=Assert.Single(e.ObjectLayers);Assert.Equal("rock.mod",o.Object);Assert.Equal(3,o.Iterations);var fl=Assert.Single(e.FloraLayers);Assert.Equal("blade.mod",fl.Flora);Assert.Equal(.6f,fl.MinAlpha);}
}

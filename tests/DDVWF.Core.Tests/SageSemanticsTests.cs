using DDVWF.Sage.Wld;
using DDVWF.Core.Zone;
namespace DDVWF.Core.Tests;

public sealed class SageSemanticsTests
{
    [Fact] public void Location_matches_Sage_axis_rotation_semantics()
    {
        var l=SageLocation.FromSource(1,2,3,128,64,999);
        Assert.Equal(1,l.X); Assert.Equal(2,l.Y); Assert.Equal(3,l.Z);
        Assert.Equal(0,l.RotateX); Assert.Equal(-90,l.RotateY); Assert.Equal(45,l.RotateZ);
    }

    [Theory]
    [InlineData(0x0,SageShaderType.Boundary)]
    [InlineData(0x01,SageShaderType.Diffuse)]
    [InlineData(0x05,SageShaderType.Transparent50)]
    [InlineData(0x09,SageShaderType.Transparent25)]
    [InlineData(0x0A,SageShaderType.Transparent75)]
    [InlineData(0x13,SageShaderType.TransparentMasked)]
    [InlineData(0x17,SageShaderType.TransparentAdditive)]
    [InlineData(0x53,SageShaderType.Invisible)]
    public void Material_map_matches_Sage(uint value,SageShaderType expected)=>Assert.Equal(expected,SageMaterial.Map(value,1));
    [Theory]
    [InlineData("wt_water",RegionSemantic.Water)]
    [InlineData("lan_lava",RegionSemantic.Lava)]
    [InlineData("drp_pvp",RegionSemantic.Pvp)]
    [InlineData("sln_block",RegionSemantic.WaterBlockLos)]
    [InlineData("vwn_cold",RegionSemantic.FreezingWater)]
    public void Region_tags_match_Sage_semantics(string tag,RegionSemantic expected)
        => Assert.Contains(expected,WldBspReader.ClassifyTag(tag).Semantics);

    [Fact] public void Referenced_zoneline_matches_Sage_encoding()
    {
        var data=WldBspReader.ClassifyTag("drntp00255000042");
        Assert.Contains(RegionSemantic.Zoneline,data.Semantics);
        Assert.Equal(42,data.ZoneLineReference);
    }
}


using DDVWF.Sage.Wld;
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
}

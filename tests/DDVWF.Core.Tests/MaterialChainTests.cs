using DDVWF.Sage.Wld;
namespace DDVWF.Core.Tests;
public sealed class MaterialChainTests
{
 [Fact] public void Unknown_material_without_bitmap_matches_Sage_invisible_fallback()=>Assert.Equal(SageShaderType.Invisible,SageMaterial.Map(0xDEAD,0));
 [Fact] public void Unknown_material_with_bitmap_matches_Sage_diffuse_fallback()=>Assert.Equal(SageShaderType.Diffuse,SageMaterial.Map(0xDEAD,2));
}

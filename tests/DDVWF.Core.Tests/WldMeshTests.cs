using DDVWF.Sage.Wld;
namespace DDVWF.Core.Tests;
public sealed class WldMeshTests
{
 [Fact] public void Fragment_type_36_is_mesh()=>Assert.Equal(WldFragmentType.Mesh,(WldFragmentType)0x36u);
 [Fact] public void Old_and_new_wld_versions_are_distinct(){Assert.NotEqual(0x00015500u,0x1000C800u);}
}

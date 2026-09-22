using System.Numerics;using DDVWF.Sage.Wld;
namespace DDVWF.Core.Tests;
public sealed class SageExportTransformTests
{
 [Fact] public void Position_adds_center_and_swaps_yz()=>Assert.Equal(new Vector3(11,33,22),SageExportTransform.Position(new(1,2,3),new(10,20,30)));
 [Fact] public void Normal_negates_x_and_swaps_yz()=>Assert.Equal(new Vector3(-1,3,2),SageExportTransform.Normal(new(1,2,3)));
 [Fact] public void Sage_static_export_does_not_apply_unconditional_root_x_flip()=>Assert.False(new SageSceneDocument([],[],false).FlipRootX);
}

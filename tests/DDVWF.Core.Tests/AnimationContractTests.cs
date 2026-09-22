using DDVWF.Sage.Wld;namespace DDVWF.Core.Tests;
public sealed class AnimationContractTests
{
 [Fact] public void Animation_fragment_ids_match_Sage(){Assert.Equal(0x12u,(uint)WldFragmentType.TrackDefinition);Assert.Equal(0x13u,(uint)WldFragmentType.TrackReference);Assert.Equal(0x37u,(uint)WldFragmentType.AnimatedVertices);}
}

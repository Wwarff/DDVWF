using DDVWF.Sage.Wld;namespace DDVWF.Core.Tests;
public sealed class SkeletonContractTests{[Fact]public void Skeleton_fragment_ids_match_Sage(){Assert.Equal(0x10u,(uint)WldFragmentType.SkeletonHierarchy);Assert.Equal(0x11u,(uint)WldFragmentType.SkeletonHierarchyReference);}}

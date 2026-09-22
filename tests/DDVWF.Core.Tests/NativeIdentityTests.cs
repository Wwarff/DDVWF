using DDVWF.Sage;namespace DDVWF.Core.Tests;
public sealed class NativeIdentityTests
{
 [Fact]public void Native_identity_is_stable(){var a=SageClientZoneProvider.Stable("cobaltscar","objects.wld","actor","tree",7);var b=SageClientZoneProvider.Stable("cobaltscar","objects.wld","actor","tree",7);Assert.Equal(a,b);}
 [Fact]public void Native_identity_distinguishes_ordinal(){Assert.NotEqual(SageClientZoneProvider.Stable("z","objects.wld","actor","tree",1),SageClientZoneProvider.Stable("z","objects.wld","actor","tree",2));}
}

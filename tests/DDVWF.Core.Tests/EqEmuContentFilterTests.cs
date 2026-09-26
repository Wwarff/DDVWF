using DDVWF.Core.Zone;

namespace DDVWF.Core.Tests;

public sealed class EqEmuContentFilterTests
{
    private static readonly HashSet<string> Enabled=new(StringComparer.Ordinal){"era","holiday"};
    private static readonly HashSet<string> Disabled=new(StringComparer.Ordinal){"retired","beta"};

    [Fact] public void All_expansions_bypasses_min_and_max(){Assert.True(EqEmuContentFilter.Passes(-1,Enabled,Disabled,20,20,"",""));}
    [Fact] public void Enforces_expansion_bounds(){Assert.False(EqEmuContentFilter.Passes(5,Enabled,Disabled,6,-1,"",""));Assert.False(EqEmuContentFilter.Passes(5,Enabled,Disabled,-1,4,"",""));Assert.True(EqEmuContentFilter.Passes(5,Enabled,Disabled,5,5,"",""));}
    [Fact] public void Requires_every_enabled_content_flag(){Assert.True(EqEmuContentFilter.Passes(5,Enabled,Disabled,-1,-1,"era,holiday",""));Assert.False(EqEmuContentFilter.Passes(5,Enabled,Disabled,-1,-1,"era,missing",""));}
    [Fact] public void Requires_every_disabled_content_flag(){Assert.True(EqEmuContentFilter.Passes(5,Enabled,Disabled,-1,-1,"","retired,beta"));Assert.False(EqEmuContentFilter.Passes(5,Enabled,Disabled,-1,-1,"","retired,missing"));}
    [Fact] public void Empty_flag_fields_are_unrestricted(){Assert.True(EqEmuContentFilter.Passes(5,Enabled,Disabled,-1,-1,"",""));}
}

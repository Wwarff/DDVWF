using DDVWF.Sage.Pfs;
namespace DDVWF.Core.Tests;
public sealed class PfsCrcTests
{
    [Fact] public void Crc_is_case_sensitive_at_primitive_level_but_provider_uses_lower_names()
    {
        Assert.Equal(PfsCrc.Get("test.wld"),PfsCrc.Get("test.wld"));
        Assert.NotEqual(PfsCrc.Get("test.wld"),PfsCrc.Get("TEST.WLD"));
    }
    [Fact] public void Empty_crc_matches_sage()=>Assert.Equal(0,PfsCrc.Get(""));
}

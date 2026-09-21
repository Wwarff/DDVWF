using DDVWF.Sage.Wld;

namespace DDVWF.Core.Tests;

public sealed class WldClassificationTests
{
    [Theory]
    [InlineData("lights.wld", WldKind.Lights)]
    [InlineData("objects.wld", WldKind.ZoneObjects)]
    [InlineData("sky.wld", WldKind.Sky)]
    [InlineData("poknowledge_obj.wld", WldKind.Objects)]
    [InlineData("global_chr.wld", WldKind.Characters)]
    [InlineData("gequip.wld", WldKind.Equipment)]
    [InlineData("poknowledge.wld", WldKind.Zone)]
    public void Matches_sage_classification(string name, WldKind expected) => Assert.Equal(expected, WldClassification.Classify(name));
}

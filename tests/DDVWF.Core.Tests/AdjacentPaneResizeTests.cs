using DDVWF.Core.Workspace;

namespace DDVWF.Core.Tests;

public sealed class AdjacentPaneResizeTests
{
    [Fact]
    public void Drag_changes_only_pair_and_preserves_pair_total()
    {
        var widths=new[]{100d,110d,120d,130d,140d,150d,160d};
        var before=(double[])widths.Clone();
        var pair=AdjacentPaneResize.Apply(widths[2],widths[3],25,60,60);
        widths[2]=pair.Left; widths[3]=pair.Right;
        Assert.Equal(145,widths[2]);
        Assert.Equal(105,widths[3]);
        Assert.Equal(before[2]+before[3],widths[2]+widths[3]);
        Assert.Equal(before[0],widths[0]); Assert.Equal(before[1],widths[1]);
        Assert.Equal(before[4],widths[4]); Assert.Equal(before[5],widths[5]); Assert.Equal(before[6],widths[6]);
    }

    [Fact]
    public void Drag_clamps_at_neighbor_minimum_without_moving_other_panes()
    {
        var pair=AdjacentPaneResize.Apply(100,100,500,60,60);
        Assert.Equal(140,pair.Left);
        Assert.Equal(60,pair.Right);
    }
}

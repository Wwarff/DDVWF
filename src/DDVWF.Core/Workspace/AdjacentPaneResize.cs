namespace DDVWF.Core.Workspace;

public static class AdjacentPaneResize
{
    public static (double Left,double Right) Apply(double left,double right,double delta,double minLeft,double minRight)
    {
        var total=left+right;
        if(total<minLeft+minRight) return (left,right);
        var nextLeft=Math.Clamp(left+delta,minLeft,total-minRight);
        return (nextLeft,total-nextLeft);
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DDVWF.Core.Workspace;

namespace DDVWF.App;

public sealed class CompassWindow : Window
{
    readonly RotateTransform rotation=new();
    readonly Polygon needle=new(){Points=new PointCollection([new(0,-42),new(9,18),new(0,10),new(-9,18)]),Stretch=Stretch.None};
    readonly Border frame=new(){BorderThickness=new Thickness(1),Background=new SolidColorBrush(Color.FromRgb(17,19,24))};

    public CompassWindow(ThemeSettings theme)
    {
        Title="Compass";Width=190;Height=190;MinWidth=130;MinHeight=130;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.CanResizeWithGrip;ShowInTaskbar=false;
        var grid=new Grid();frame.Child=grid;Content=frame;
        var north=new TextBlock{Text="N",HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,12,0,0),FontFamily=new FontFamily("Verdana"),FontSize=12};
        needle.HorizontalAlignment=HorizontalAlignment.Center;needle.VerticalAlignment=VerticalAlignment.Center;needle.RenderTransformOrigin=new Point(.5,.5);needle.RenderTransform=rotation;
        grid.Children.Add(needle);grid.Children.Add(north);
        ApplyTheme(theme);
        MouseLeftButtonDown+=(_,e)=>{if(e.ButtonState==MouseButtonState.Pressed)DragMove();};
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape){e.Handled=true;Close();}};
    }

    public void SetHeading(float yawRadians)=>rotation.Angle=-yawRadians*180/Math.PI;
    public void ApplyTheme(ThemeSettings theme)
    {
        try
        {
            var accent=(Color)ColorConverter.ConvertFromString(theme.Compass);
            var separator=(Color)ColorConverter.ConvertFromString(theme.Separator);
            needle.Fill=new SolidColorBrush(accent);
            frame.BorderBrush=new SolidColorBrush(separator);
            Foreground=new SolidColorBrush(accent);
        }
        catch { }
    }
}

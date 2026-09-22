using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DDVWF.Core.Workspace;

namespace DDVWF.App;

public sealed class CompassWindow : Window
{
    readonly RotateTransform ringRotation=new();
    readonly Grid ring=new();
    readonly Polygon pointer=new(){Points=new PointCollection([new(0,-38),new(7,-18),new(0,-23),new(-7,-18)]),Stretch=Stretch.None};
    readonly Border frame=new(){BorderThickness=new Thickness(1),Background=new SolidColorBrush(Color.FromRgb(17,19,24))};
    readonly List<TextBlock> labels=[];

    public CompassWindow(ThemeSettings theme)
    {
        Title="Compass";Width=190;Height=190;MinWidth=130;MinHeight=130;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.CanResizeWithGrip;ShowInTaskbar=false;
        var grid=new Grid();frame.Child=grid;Content=frame;
        ring.RenderTransformOrigin=new Point(.5,.5);ring.RenderTransform=ringRotation;
        AddCardinal("N",HorizontalAlignment.Center,VerticalAlignment.Top,new Thickness(0,12,0,0));
        AddCardinal("E",HorizontalAlignment.Right,VerticalAlignment.Center,new Thickness(0,0,12,0));
        AddCardinal("S",HorizontalAlignment.Center,VerticalAlignment.Bottom,new Thickness(0,0,0,12));
        AddCardinal("W",HorizontalAlignment.Left,VerticalAlignment.Center,new Thickness(12,0,0,0));
        grid.Children.Add(ring);
        pointer.HorizontalAlignment=HorizontalAlignment.Center;pointer.VerticalAlignment=VerticalAlignment.Center;grid.Children.Add(pointer);
        ApplyTheme(theme);
        MouseLeftButtonDown+=(_,e)=>{if(e.ButtonState==MouseButtonState.Pressed)DragMove();};
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape){e.Handled=true;Close();}};
    }

    void AddCardinal(string text,HorizontalAlignment horizontal,VerticalAlignment vertical,Thickness margin)
    {
        var label=new TextBlock{Text=text,HorizontalAlignment=horizontal,VerticalAlignment=vertical,Margin=margin,FontFamily=new FontFamily("Verdana"),FontSize=12};
        labels.Add(label);ring.Children.Add(label);
    }

    public void SetHeading(float yawRadians)=>ringRotation.Angle=-yawRadians*180/Math.PI;
    public void ApplyTheme(ThemeSettings theme)
    {
        try
        {
            var accent=(Color)ColorConverter.ConvertFromString(theme.Compass);
            var separator=(Color)ColorConverter.ConvertFromString(theme.Separator);
            var brush=new SolidColorBrush(accent);
            pointer.Fill=brush;foreach(var label in labels)label.Foreground=brush;
            frame.BorderBrush=new SolidColorBrush(separator);Foreground=brush;
        }
        catch { }
    }
}

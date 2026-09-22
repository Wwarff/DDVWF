using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Shell;
using DDVWF.Core.Workspace;

namespace DDVWF.App;

public sealed class CompassWindow:Window
{
    readonly Canvas face=new(){Width=600,Height=600};
    readonly Canvas bearingRing=new(){Width=600,Height=600,RenderTransformOrigin=new Point(.5,.5)};
    readonly RotateTransform bearingRotation=new();
    ThemeSettings theme;
    public CompassWindow(ThemeSettings value)
    {
        theme=value;Title="Compass";Width=520;Height=520;MinWidth=180;MinHeight=180;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.CanResize;ShowInTaskbar=false;Background=Brushes.Transparent;AllowsTransparency=false;
        WindowChrome.SetWindowChrome(this,new WindowChrome{CaptionHeight=0,ResizeBorderThickness=new Thickness(6),CornerRadius=new CornerRadius(0),GlassFrameThickness=new Thickness(0)});
        bearingRing.RenderTransform=bearingRotation;var view=new Viewbox{Stretch=Stretch.Uniform,Child=face};Content=view;Build();
        MouseLeftButtonDown+=(_,e)=>{if(e.LeftButton==MouseButtonState.Pressed)DragMove();};
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape){e.Handled=true;Close();}};
    }
    static SolidColorBrush B(string s)=>new((Color)ColorConverter.ConvertFromString(s));
    string C(string k,string fallback)=>theme.Palette.TryGetValue(k,out var v)?v:fallback;
    public void SetHeading(float yawRadians)=>bearingRotation.Angle=-yawRadians*180/Math.PI;
    public void ApplyTheme(ThemeSettings value){theme=value;Build();}
    void Build()
    {
        face.Children.Clear();face.Background=B(C("CompassBackgroundBrush","#000000"));
        bearingRing.Children.Clear();face.Children.Add(bearingRing);
        var primary=B(C("CompassPrimaryBrush","#A68A6A"));var secondary=B(C("CompassSecondaryBrush","#6E5947"));var star=B(C("CompassStarBrush","#E8D9B5"));var accent=B(C("CompassAccentBrush","#D7FF00"));var degrees=B(C("CompassDegreeTextBrush","#A68A6A"));var ticks=B(C("CompassTickBrush","#A68A6A"));var card=B(C("CompassCardinalBrush","#A68A6A"));
        Ring(bearingRing,300,300,266,primary,2);Ring(bearingRing,300,300,244,primary,2);
        for(int d=0;d<360;d+=2){var major=d%10==0;var r1=major?232:238;var r2=246;bearingRing.Children.Add(LineAt(d,r1,r2,ticks,major?3:1));}
        for(int d=0;d<360;d+=10){var p=Polar(d,270);var t=new TextBlock{Text=d.ToString(),Foreground=degrees,FontFamily=new FontFamily("Georgia"),FontSize=22,FontWeight=FontWeights.SemiBold,RenderTransformOrigin=new Point(.5,.5),RenderTransform=new RotateTransform(d)};Place(t,p.X,p.Y,38,28,bearingRing);}
        Ring(face,300,300,198,primary,2);Ring(face,300,300,150,secondary,2);Ring(face,300,300,112,secondary,2);
        for(int d=0;d<360;d+=45){face.Children.Add(LineAt(d,112,150,secondary,2));}
        var cardinals=new[]{("N",0),("E",90),("S",180),("W",270)};foreach(var (s,d) in cardinals){var p=Polar(d,202);var t=new TextBlock{Text=s,Foreground=card,FontFamily=new FontFamily("Georgia"),FontSize=42,FontWeight=FontWeights.Bold};Place(t,p.X,p.Y,48,50,face);}
        var inner=new[]{("nne",22.5),("ne",45),("nee",67.5),("see",112.5),("se",135),("sse",157.5),("ssw",202.5),("sw",225),("sww",247.5),("nww",292.5),("nw",315),("nnw",337.5)};
        foreach(var (s,d) in inner){var p=Polar(d,132);var t=new TextBlock{Text=s,Foreground=secondary,FontFamily=new FontFamily("Georgia"),FontStyle=FontStyles.Italic,FontSize=18,FontWeight=FontWeights.Bold,RenderTransformOrigin=new Point(.5,.5),RenderTransform=new RotateTransform(d)};Place(t,p.X,p.Y,40,24,face);}
        for(int d=0;d<360;d+=45)AddStarPoint(d,d%90==0?178:132,star,primary);
        var hub=new Ellipse{Width=12,Height=12,Fill=accent};Canvas.SetLeft(hub,294);Canvas.SetTop(hub,294);face.Children.Add(hub);
    }
    void AddStarPoint(double deg,double length,Brush light,Brush dark)
    {
        var tip=Polar(deg,length),left=Polar(deg-90,22),right=Polar(deg+90,22);var center=new Point(300,300);
        var a=new Polygon{Points=new PointCollection{center,left,tip},Fill=light,Stroke=dark,StrokeThickness=1};var b=new Polygon{Points=new PointCollection{center,tip,right},Fill=dark,Stroke=dark,StrokeThickness=1};face.Children.Add(a);face.Children.Add(b);
    }
    static Line LineAt(double deg,double r1,double r2,Brush brush,double width){var a=Polar(deg,r1),b=Polar(deg,r2);return new Line{X1=a.X,Y1=a.Y,X2=b.X,Y2=b.Y,Stroke=brush,StrokeThickness=width};}
    static Point Polar(double deg,double radius){var a=(deg-90)*Math.PI/180;return new Point(300+Math.Cos(a)*radius,300+Math.Sin(a)*radius);}
    static void Ring(Canvas c,double x,double y,double r,Brush stroke,double width){var e=new Ellipse{Width=r*2,Height=r*2,Stroke=stroke,StrokeThickness=width};Canvas.SetLeft(e,x-r);Canvas.SetTop(e,y-r);c.Children.Add(e);}
    static void Place(FrameworkElement e,double x,double y,double w,double h,Canvas c){e.Width=w;e.Height=h;if(e is TextBlock t)t.TextAlignment=TextAlignment.Center;Canvas.SetLeft(e,x-w/2);Canvas.SetTop(e,y-h/2);c.Children.Add(e);}
}
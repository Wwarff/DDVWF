using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DDVWF.App;

public sealed class VwfColorPicker : Window
{
    readonly Canvas sv=new(), hue=new(); readonly Border sample=new(); readonly Rectangle svWhite=new(){IsHitTestVisible=false},svBlack=new(){IsHitTestVisible=false}; readonly TextBox hex=new();
    readonly Slider r=new(){Minimum=0,Maximum=255},g=new(){Minimum=0,Maximum=255},b=new(){Minimum=0,Maximum=255},i=new(){Minimum=0,Maximum=100,Value=100},a=new(){Minimum=0,Maximum=255,Value=255};
    readonly TextBox rn=new(),gn=new(),bn=new(),inn=new(),an=new(); readonly Ellipse svCursor=new(){Width=12,Height=12,Stroke=Brushes.White,StrokeThickness=2,IsHitTestVisible=false}; readonly Rectangle hueCursor=new(){Height=4,Stroke=Brushes.White,StrokeThickness=1,Fill=new SolidColorBrush(Color.FromArgb(128,0,0,0)),IsHitTestVisible=false};
    readonly Action<Color> preview; readonly DDVWF.Core.Workspace.ThemeSettings? activeTheme; bool updating; double h,s,v=1; public Color SelectedColor{get;private set;}
    public VwfColorPicker(Color initial,Action<Color> live,DDVWF.Core.Workspace.ThemeSettings? theme=null)
    {
        SelectedColor=initial;preview=live;activeTheme=theme;Title="Color";Width=300;Height=610;MinWidth=300;ResizeMode=ResizeMode.CanResize;WindowStartupLocation=WindowStartupLocation.CenterOwner;FontFamily=new FontFamily("Verdana");FontSize=12;Background=B("#030706");Foreground=B("#70B93B");
        var root=new DockPanel{Margin=new Thickness(7)};Content=theme is null?root:DdvwfChrome.Wrap(this,"COLOR",root,theme);DdvwfChrome.AttachPersistentGeometry(this,"color-picker");
        var close=new Button{Content="X",Width=25,Height=22,HorizontalAlignment=HorizontalAlignment.Right};close.Click+=(_,_)=>{DialogResult=true;Close();};DockPanel.SetDock(close,Dock.Top);root.Children.Add(close);
        var stack=new StackPanel();root.Children.Add(stack);
        var top=new Grid{Height=260};top.ColumnDefinitions.Add(new ColumnDefinition());top.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(27)});stack.Children.Add(top);
        sv.Background=new SolidColorBrush(Colors.Red);svWhite.Fill=new LinearGradientBrush(Colors.White,Colors.Transparent,0);svBlack.Fill=new LinearGradientBrush(Colors.Transparent,Colors.Black,90);sv.Children.Add(svWhite);sv.Children.Add(svBlack);sv.Children.Add(svCursor);sv.SizeChanged+=(_,_)=>{svWhite.Width=svBlack.Width=sv.ActualWidth;svWhite.Height=svBlack.Height=sv.ActualHeight;Refresh();};sv.MouseDown+=SvMouse;sv.MouseMove+=SvMouse;top.Children.Add(sv);
        hue.Background=new LinearGradientBrush(new GradientStopCollection{new(Colors.Red,0),new(Colors.Yellow,1d/6),new(Colors.Lime,2d/6),new(Colors.Cyan,3d/6),new(Colors.Blue,4d/6),new(Colors.Magenta,5d/6),new(Colors.Red,1)},90);hue.Margin=new Thickness(3,0,0,0);hue.MouseDown+=HueMouse;hue.MouseMove+=HueMouse;hue.Children.Add(hueCursor);Grid.SetColumn(hue,1);top.Children.Add(hue);
        sample.Height=25;sample.Margin=new Thickness(0,4,0,4);stack.Children.Add(sample);
        var modes=new UniformGrid{Columns=3};foreach(var x in new[]{"RGB","HSV","Linear"})modes.Children.Add(new Button{Content=x,Height=31});stack.Children.Add(modes);
        AddChannel(stack,"R",r,rn);AddChannel(stack,"G",g,gn);AddChannel(stack,"B",b,bn);AddChannel(stack,"I",i,inn);AddChannel(stack,"A",a,an);
        var hr=new Grid{Height=32};hr.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(39)});hr.ColumnDefinitions.Add(new ColumnDefinition());hr.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(28)});hr.Children.Add(new TextBlock{Text="Hex",VerticalAlignment=VerticalAlignment.Center});hex.Margin=new Thickness(2);Grid.SetColumn(hex,1);hr.Children.Add(hex);var copy=new Button{Content="▣",Margin=new Thickness(2)};copy.Click+=(_,_)=>Clipboard.SetText(hex.Text);Grid.SetColumn(copy,2);hr.Children.Add(copy);stack.Children.Add(hr);
        stack.Children.Add(new Expander{Header="● Swatches",Height=28});stack.Children.Add(new Expander{Header="● Recent Colors",Height=28});
        var save=new Button{Content="SAVE COLOR",Height=26,Margin=new Thickness(0,4,0,0)};save.Click+=(_,_)=>{DialogResult=true;Close();};stack.Children.Add(save);
        foreach(var sl in new[]{r,g,b,i,a})sl.ValueChanged+=ChannelChanged;hex.LostKeyboardFocus+=(_,_)=>HexChanged();hex.KeyDown+=(_,e)=>{if(e.Key==Key.Enter)HexChanged();};
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape){DialogResult=false;Close();}};
        SetFromColor(initial);RefreshChrome();
    }
    static SolidColorBrush B(string s)=>new((Color)ColorConverter.ConvertFromString(s));
    void AddChannel(Panel p,string label,Slider sl,TextBox n){var q=new Grid{Height=33};q.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(20)});q.ColumnDefinitions.Add(new ColumnDefinition());q.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(53)});q.Children.Add(new TextBlock{Text=label,VerticalAlignment=VerticalAlignment.Center});sl.Margin=new Thickness(3);Grid.SetColumn(sl,1);q.Children.Add(sl);n.Margin=new Thickness(3);Grid.SetColumn(n,2);q.Children.Add(n);p.Children.Add(q);}
    void SetFromColor(Color c){updating=true;SelectedColor=c;r.Value=c.R;g.Value=c.G;b.Value=c.B;a.Value=c.A;i.Value=100;RgbToHsv(c.R,c.G,c.B,out h,out s,out v);updating=false;Refresh();}
    void ChannelChanged(object? s0,RoutedPropertyChangedEventArgs<double> e){if(updating)return;var intensity=i.Value/100d;SetColor(Color.FromArgb((byte)a.Value,(byte)Math.Clamp(r.Value*intensity,0,255),(byte)Math.Clamp(g.Value*intensity,0,255),(byte)Math.Clamp(b.Value*intensity,0,255)),false);}
    void SetColor(Color c,bool sync){SelectedColor=c;if(sync){updating=true;r.Value=c.R;g.Value=c.G;b.Value=c.B;a.Value=c.A;updating=false;}Refresh();preview(c);}
    void Refresh(){sample.Background=new SolidColorBrush(SelectedColor);hex.Text=$"{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";rn.Text=((int)r.Value).ToString();gn.Text=((int)g.Value).ToString();bn.Text=((int)b.Value).ToString();inn.Text=((int)i.Value).ToString();an.Text=((int)a.Value).ToString();sv.Background=new SolidColorBrush(Hsv(h,1,1,255));Canvas.SetLeft(svCursor,s*Math.Max(1,sv.ActualWidth)-6);Canvas.SetTop(svCursor,(1-v)*Math.Max(1,sv.ActualHeight)-6);Canvas.SetTop(hueCursor,h*Math.Max(1,hue.ActualHeight)-2);hueCursor.Width=Math.Max(1,hue.ActualWidth);}
    public void RefreshChrome(){if(activeTheme is not null){string P(string k,string f)=>activeTheme.Palette.TryGetValue(k,out var v)?v:f;Background=B(P("PickerBackground","#1F2121"));Foreground=B(P("PickerFont","#EBEBEB"));var control=B(P("PickerControlBackground","#282B2B"));var font=B(P("PickerFont","#EBEBEB"));var border=B(P("PickerBorder","#464A4A"));foreach(var box in new[]{hex,rn,gn,bn,inn,an}){box.Background=control;box.Foreground=font;box.BorderBrush=border;}svCursor.Stroke=B(P("PickerSelectorArrow","#EBEBEB"));hueCursor.Stroke=B(P("PickerSelectorArrowOutline","#000000"));}Refresh();}
    void SvMouse(object sender,MouseEventArgs e){if(e.LeftButton!=MouseButtonState.Pressed)return;var p=e.GetPosition(sv);s=Math.Clamp(p.X/Math.Max(1,sv.ActualWidth),0,1);v=1-Math.Clamp(p.Y/Math.Max(1,sv.ActualHeight),0,1);var c=Hsv(h,s,v,(byte)a.Value);SetColor(c,true);}
    void HueMouse(object sender,MouseEventArgs e){if(e.LeftButton!=MouseButtonState.Pressed)return;var p=e.GetPosition(hue);h=Math.Clamp(p.Y/Math.Max(1,hue.ActualHeight),0,1);SetColor(Hsv(h,s,v,(byte)a.Value),true);}
    void HexChanged(){var t=hex.Text.Trim().TrimStart('#');if(t.Length!=6||!uint.TryParse(t,NumberStyles.HexNumber,CultureInfo.InvariantCulture,out var x))return;SetFromColor(Color.FromArgb((byte)a.Value,(byte)(x>>16),(byte)(x>>8),(byte)x));preview(SelectedColor);}
    static Color Hsv(double h,double s,double v,byte alpha){double x=h*6;int k=(int)Math.Floor(x)%6;double f=x-Math.Floor(x),p=v*(1-s),q=v*(1-f*s),t=v*(1-(1-f)*s);(double R,double G,double B) z=k switch{0=>(v,t,p),1=>(q,v,p),2=>(p,v,t),3=>(p,q,v),4=>(t,p,v),_=>(v,p,q)};return Color.FromArgb(alpha,(byte)(z.R*255),(byte)(z.G*255),(byte)(z.B*255));}
    static void RgbToHsv(byte rr,byte gg,byte bb,out double hh,out double ss,out double vv){double R=rr/255d,G=gg/255d,B=bb/255d,max=Math.Max(R,Math.Max(G,B)),min=Math.Min(R,Math.Min(G,B)),d=max-min;vv=max;ss=max==0?0:d/max;if(d==0)hh=0;else if(max==R)hh=((G-B)/d%6)/6;else if(max==G)hh=((B-R)/d+2)/6;else hh=((R-G)/d+4)/6;if(hh<0)hh+=1;}
}
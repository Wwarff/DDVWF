using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Shell;
using DDVWF.Core.Workspace;

namespace DDVWF.App;

internal static class DdvwfChrome
{
    static SolidColorBrush B(string s)=>new((Color)ColorConverter.ConvertFromString(s));
    static string C(ThemeSettings t,string key,string fallback)=>t.Palette.TryGetValue(key,out var v)?v:fallback;


    public static void AttachPersistentGeometry(Window w,string key)
    {
        var path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DragonDenWorldForge",key+"-window.txt");
        void Save(){try{Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,string.Join("|",new[]{w.Left,w.Top,w.Width,w.Height}.Select(v=>v.ToString("R",System.Globalization.CultureInfo.InvariantCulture))));}catch(Exception ex){System.Diagnostics.Debug.WriteLine("DDVWF window geometry save failed: "+ex.Message);}}
        try{if(File.Exists(path)){var a=File.ReadAllText(path).Split('|');if(a.Length==4){var v=a.Select(s=>double.Parse(s,System.Globalization.CultureInfo.InvariantCulture)).ToArray();w.WindowStartupLocation=WindowStartupLocation.Manual;w.Width=Math.Max(w.MinWidth,v[2]);w.Height=Math.Max(w.MinHeight,v[3]);var l=SystemParameters.VirtualScreenLeft;var t=SystemParameters.VirtualScreenTop;var r=l+SystemParameters.VirtualScreenWidth;var b=t+SystemParameters.VirtualScreenHeight;w.Left=Math.Min(Math.Max(l,v[0]),Math.Max(l,r-80));w.Top=Math.Min(Math.Max(t,v[1]),Math.Max(t,b-40));}}}catch(Exception ex){System.Diagnostics.Debug.WriteLine("DDVWF window geometry restore failed: "+ex.Message);}
        w.LocationChanged+=(_,_)=>Save();w.SizeChanged+=(_,_)=>Save();w.StateChanged+=(_,_)=>Save();w.Closed+=(_,_)=>Save();
    }

    public static DockPanel Wrap(Window w,string title,UIElement body,ThemeSettings theme,bool allowMaximize=false)
    {
        w.WindowStyle=WindowStyle.None;w.ResizeMode=ResizeMode.CanResize;w.Background=B(C(theme,"PanelBrush","#100D18"));w.Foreground=B(C(theme,"TextBrush","#D7FF00"));
        WindowChrome.SetWindowChrome(w,new WindowChrome{CaptionHeight=0,ResizeBorderThickness=new Thickness(6),CornerRadius=new CornerRadius(0),GlassFrameThickness=new Thickness(0)});
        ApplyResources(w,theme);
        var root=new DockPanel();
        var bar=new Grid{Height=24,Background=B(C(theme,"TitleBarBrush","#08070C"))};
        bar.ColumnDefinitions.Add(new ColumnDefinition());bar.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
        var caption=new TextBlock{Text=title,Margin=new Thickness(6,0,6,0),VerticalAlignment=VerticalAlignment.Center,Foreground=B(C(theme,"TitleBarTextBrush","#D7FF00")),FontFamily=new FontFamily("Verdana"),FontSize=12};
        bar.Children.Add(caption);
        var controls=new StackPanel{Orientation=Orientation.Horizontal};Grid.SetColumn(controls,1);bar.Children.Add(controls);
        if(allowMaximize)controls.Children.Add(CaptionButton(w,"□",()=>w.WindowState=w.WindowState==System.Windows.WindowState.Maximized?System.Windows.WindowState.Normal:System.Windows.WindowState.Maximized,theme));
        controls.Children.Add(CaptionButton(w,"X",w.Close,theme));
        bar.MouseLeftButtonDown+=(_,e)=>{if(e.OriginalSource is TextBlock tb&&tb.Tag as string=="captionButton")return;if(allowMaximize&&e.ClickCount==2)w.WindowState=w.WindowState==System.Windows.WindowState.Maximized?System.Windows.WindowState.Normal:System.Windows.WindowState.Maximized;else if(e.LeftButton==MouseButtonState.Pressed)w.DragMove();};
        DockPanel.SetDock(bar,Dock.Top);root.Children.Add(bar);root.Children.Add(body);return root;
    }

    static Border CaptionButton(Window w,string text,Action action,ThemeSettings theme)
    {
        var normal=B(C(theme,"TitleBarBrush","#08070C"));var hover=B(C(theme,"TitleBarButtonHoverBrush","#282038"));
        var label=new TextBlock{Text=text,Tag="captionButton",HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,Foreground=B(C(theme,"TitleBarTextBrush","#D7FF00"))};
        var b=new Border{Width=32,Height=22,Background=normal,Child=label};b.MouseEnter+=(_,_)=>b.Background=hover;b.MouseLeave+=(_,_)=>b.Background=normal;b.MouseLeftButtonUp+=(_,e)=>{e.Handled=true;action();};return b;
    }

    public static void ApplyResources(FrameworkElement root,ThemeSettings theme)
    {
        foreach(var p in ThemeSettings.DefaultPalette()){var v=theme.Palette.TryGetValue(p.Key,out var x)?x:p.Value;root.Resources[p.Key]=B(v);}
        var scroll=new Style(typeof(ScrollBar));scroll.Setters.Add(new Setter(ScrollBar.WidthProperty,5d));scroll.Setters.Add(new Setter(Control.BackgroundProperty,root.Resources["ScrollTrackBrush"]));scroll.Setters.Add(new Setter(Control.ForegroundProperty,root.Resources["ScrollThumbBrush"]));
        var scrollTemplate=(ControlTemplate)XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='{x:Type ScrollBar}'><Grid Background='{TemplateBinding Background}'><Track x:Name='PART_Track' IsDirectionReversed='True'><Track.DecreaseRepeatButton><RepeatButton Command='{x:Static ScrollBar.PageUpCommand}' Background='Transparent' BorderThickness='0' Opacity='0'/></Track.DecreaseRepeatButton><Track.Thumb><Thumb><Thumb.Template><ControlTemplate TargetType='{x:Type Thumb}'><Border Background='{DynamicResource ScrollThumbBrush}' BorderThickness='0'/></ControlTemplate></Thumb.Template></Thumb></Track.Thumb><Track.IncreaseRepeatButton><RepeatButton Command='{x:Static ScrollBar.PageDownCommand}' Background='Transparent' BorderThickness='0' Opacity='0'/></Track.IncreaseRepeatButton></Track></Grid></ControlTemplate>");
        scroll.Setters.Add(new Setter(Control.TemplateProperty,scrollTemplate));
        var horizontal=new Trigger{Property=ScrollBar.OrientationProperty,Value=Orientation.Horizontal};horizontal.Setters.Add(new Setter(ScrollBar.WidthProperty,double.NaN));horizontal.Setters.Add(new Setter(ScrollBar.HeightProperty,5d));scroll.Triggers.Add(horizontal);root.Resources[typeof(ScrollBar)]=scroll;
        var text=new Style(typeof(TextBox));text.Setters.Add(new Setter(Control.BackgroundProperty,root.Resources["InputBrush"]));text.Setters.Add(new Setter(Control.ForegroundProperty,root.Resources["TextBrush"]));text.Setters.Add(new Setter(Control.BorderBrushProperty,root.Resources["ControlBorderBrush"]));text.Setters.Add(new Setter(Control.BorderThicknessProperty,new Thickness(1)));text.Setters.Add(new Setter(TextBox.CaretBrushProperty,root.Resources["AccentBrush"]));text.Setters.Add(new Setter(TextBox.SelectionBrushProperty,root.Resources["SelectionBrush"]));text.Setters.Add(new Setter(Control.TemplateProperty,(ControlTemplate)XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='{x:Type TextBox}'><Border Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}'><ScrollViewer x:Name='PART_ContentHost' Margin='3,0'/></Border></ControlTemplate>")));root.Resources[typeof(TextBox)]=text;
        var button=new Style(typeof(Button));button.Setters.Add(new Setter(Control.BackgroundProperty,root.Resources["ButtonBrush"]));button.Setters.Add(new Setter(Control.ForegroundProperty,root.Resources["TextBrush"]));button.Setters.Add(new Setter(Control.BorderBrushProperty,root.Resources["ControlBorderBrush"]));button.Setters.Add(new Setter(Control.BorderThicknessProperty,new Thickness(1)));button.Setters.Add(new Setter(Control.TemplateProperty,(ControlTemplate)XamlReader.Parse("<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='{x:Type Button}'><Border x:Name='Chrome' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' Padding='{TemplateBinding Padding}'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border><ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'><Setter TargetName='Chrome' Property='Background' Value='{DynamicResource ButtonHoverBrush}'/></Trigger><Trigger Property='IsPressed' Value='True'><Setter TargetName='Chrome' Property='Background' Value='{DynamicResource SelectionBrush}'/></Trigger></ControlTemplate.Triggers></ControlTemplate>")));root.Resources[typeof(Button)]=button;
    }
}
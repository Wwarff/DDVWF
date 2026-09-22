using System.IO;
using System.Windows;
using DDVWF.Core.Commands;
using DDVWF.Core.Workspace;
using DDVWF.Core.Zone;
using DDVWF.Sage;
using DDVWF.Server;
using DDVWF.Renderer.Babylon;

namespace DDVWF.App;
public partial class MainWindow:Window
{
 readonly CommandHistory _history=new();readonly JsonWorkspaceStore _workspace;WorkspaceSettings _settings=new();ZoneSession? _session; SageClientZoneProvider? _client;Guid? _selectedId;
 public MainWindow(){InitializeComponent();Viewport.CoreWebView2InitializationCompleted+=(_,e)=>{if(!e.IsSuccess)StatusText.Text=$"Renderer failed: {e.InitializationException?.Message}";};var root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DragonsDen","DDVWF");_workspace=new(Path.Combine(root,"workspace.json"));Loaded+=async(_,_)=>await RestoreAsync();Closing+=async(_,_)=>await PersistAsync();}
 async Task RestoreAsync(){await Viewport.EnsureCoreWebView2Async();var html=Path.Combine(AppContext.BaseDirectory,"Renderer","babylon.html");if(File.Exists(html))Viewport.Source=new Uri(html);Viewport.WebMessageReceived+=Viewport_WebMessageReceived;_settings=await _workspace.LoadAsync();StatusText.Text=string.IsNullOrWhiteSpace(_settings.EqRoot)?"Choose EverQuest directory":_settings.EqRoot;if(!string.IsNullOrWhiteSpace(_settings.EqRoot)&&Directory.Exists(_settings.EqRoot)){DiscoverZones();if(!string.IsNullOrWhiteSpace(_settings.LastZone))await OpenZoneAsync(_settings.LastZone);}}
 void DiscoverZones(){AssetTree.Items.Clear();foreach(var p in Directory.EnumerateFiles(_settings.EqRoot!,"*.s3d",SearchOption.TopDirectoryOnly).Where(x=>!Path.GetFileNameWithoutExtension(x).EndsWith("_obj",StringComparison.OrdinalIgnoreCase)&&!Path.GetFileNameWithoutExtension(x).EndsWith("_obj2",StringComparison.OrdinalIgnoreCase)).OrderBy(Path.GetFileName)){var z=Path.GetFileNameWithoutExtension(p);var item=new System.Windows.Controls.TreeViewItem{Header=z,Tag=z};item.MouseDoubleClick+=async(_,_)=>await OpenZoneAsync(z);AssetTree.Items.Add(item);}}
 async Task OpenZoneAsync(string zone){try{StatusText.Text=$"Loading {zone}...";_client=new SageClientZoneProvider();IServerDataProvider server=new NoServerDataProvider();if(_settings.Server.Enabled&&!string.IsNullOrWhiteSpace(_settings.Server.User)&&!string.IsNullOrWhiteSpace(_settings.Server.Password))server=EqEmuMySqlConnection.CreateReadProvider(new(_settings.Server.Host,_settings.Server.Port,_settings.Server.Database,_settings.Server.User,_settings.Server.Password));_session=new(_client,server,new SceneTreeViewport(SceneTree));var loaded=await _session.OpenAsync(_settings.EqRoot!,zone);var payload=BabylonSceneSerializer.Serialize(loaded,_client.RenderMeshes);await Viewport.CoreWebView2.ExecuteScriptAsync($"window.ddvwf.loadScene({System.Text.Json.JsonSerializer.Serialize(payload)})");_settings=_settings with{LastZone=zone};ZoneText.Text=$"{zone}  |  {loaded.Entities.Count} entities";StatusText.Text=$"Loaded {zone}";await PersistAsync();}catch(Exception ex){StatusText.Text=$"Load failed: {ex.Message}";}}
 void Viewport_WebMessageReceived(object? sender,Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e){try{using var d=System.Text.Json.JsonDocument.Parse(e.TryGetWebMessageAsString());var root=d.RootElement;var type=root.GetProperty("type").GetString();if(type=="camera"){CoordText.Text=$"EQ X {root.GetProperty("x").GetSingle():0.00}  Y {root.GetProperty("y").GetSingle():0.00}  Z {root.GetProperty("z").GetSingle():0.00}";return;}if(!Guid.TryParse(root.GetProperty("id").GetString(),out var id)||_session?.ActiveZone is not{} zone||!zone.Contains(id))return;if(type=="transform"){var before=zone.Get(id);var pos=new EqPosition(root.GetProperty("x").GetSingle(),root.GetProperty("y").GetSingle(),root.GetProperty("z").GetSingle(),root.GetProperty("heading").GetSingle());_history.Execute(new TransformCommand(zone,id,pos,root.GetProperty("scale").GetSingle()));StatusText.Text=$"Transformed {before.Name}";ShowInspector(zone.Get(id));return;}if(type!="select")return;_selectedId=id;ShowInspector(zone.Get(id));}catch(Exception ex){StatusText.Text=$"Viewport message failed: {ex.Message}";}}\n void ShowInspector(ZoneEntity entity){InspectorPanel.Children.Clear();InspectorPanel.Children.Add(new System.Windows.Controls.TextBlock{Text=$"{entity.Kind}: {entity.Name}"});InspectorPanel.Children.Add(new System.Windows.Controls.TextBlock{Text=$"X {entity.Position.X:0.00}  Y {entity.Position.Y:0.00}  Z {entity.Position.Z:0.00}  H {entity.Position.Heading:0.00}  Scale {entity.Scale:0.000}"});StatusText.Text=$"Selected {entity.Name}";}
 Task PersistAsync()=>_workspace.SaveAsync(_settings);
 async void ChooseEqRoot_Click(object sender,RoutedEventArgs e){var dialog=new Microsoft.Win32.OpenFolderDialog{Title="Select EverQuest client directory",Multiselect=false};if(dialog.ShowDialog(this)!=true)return;_settings=_settings with{EqRoot=dialog.FolderName,LastZone=null};DiscoverZones();ZoneText.Text="Choose a zone from EQ Asset Browser";StatusText.Text=dialog.FolderName;await PersistAsync();}
 void OpenZone_Click(object sender,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(_settings.EqRoot)||!Directory.Exists(_settings.EqRoot)){StatusText.Text="Configure a valid EverQuest directory before opening a zone.";return;}DiscoverZones();StatusText.Text="Double-click a zone in EQ Asset Browser.";}
 void CloseZone_Click(object sender,RoutedEventArgs e){_session?.Close();SceneTree.Items.Clear();ZoneText.Text="No zone loaded";}
 async Task ScriptAsync(string script){if(Viewport.CoreWebView2 is not null)await Viewport.CoreWebView2.ExecuteScriptAsync(script);}
 async void SelectMode_Click(object sender,RoutedEventArgs e)=>await ScriptAsync("window.ddvwf?.setMode?.('select')");
 async void MoveMode_Click(object sender,RoutedEventArgs e)=>await ScriptAsync("window.ddvwf?.setMode?.('move')");
 async void RotateMode_Click(object sender,RoutedEventArgs e)=>await ScriptAsync("window.ddvwf?.setMode?.('rotate')");
 async void ScaleMode_Click(object sender,RoutedEventArgs e)=>await ScriptAsync("window.ddvwf?.setMode?.('scale')");
 async void Focus_Click(object sender,RoutedEventArgs e)=>await ScriptAsync("window.ddvwf?.focus?.()");
 async void Delete_Click(object sender,RoutedEventArgs e){if(_selectedId is not Guid id||_session?.ActiveZone is not{} z)return;_history.Execute(new DeleteEntityCommand(z,id));await ScriptAsync($"window.ddvwf?.remove?.('{id}')");_selectedId=null;StatusText.Text="Deleted entity";}
 void Duplicate_Click(object sender,RoutedEventArgs e){StatusText.Text="Duplicate requires renderer synchronization before activation.";}
 void Stamp_Click(object sender,RoutedEventArgs e){StatusText.Text="Stamp requires entity-specific persistence before activation.";}
 void Undo_Click(object sender,RoutedEventArgs e){if(_history.Undo())StatusText.Text="Undo";}
 void Redo_Click(object sender,RoutedEventArgs e){if(_history.Redo())StatusText.Text="Redo";}
 void Exit_Click(object sender,RoutedEventArgs e)=>Close();
}

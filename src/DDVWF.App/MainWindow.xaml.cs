using System.IO;
using System.Windows;
using DDVWF.Core.Commands;
using DDVWF.Core.Workspace;
using DDVWF.Core.Zone;
using DDVWF.Sage;
using DDVWF.Server;

namespace DDVWF.App;
public partial class MainWindow:Window
{
 readonly CommandHistory _history=new();readonly JsonWorkspaceStore _workspace;WorkspaceSettings _settings=new();ZoneSession? _session;
 public MainWindow(){InitializeComponent();var root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DragonsDen","DDVWF");_workspace=new(Path.Combine(root,"workspace.json"));Loaded+=async(_,_)=>await RestoreAsync();Closing+=async(_,_)=>await PersistAsync();}
 async Task RestoreAsync(){_settings=await _workspace.LoadAsync();StatusText.Text=string.IsNullOrWhiteSpace(_settings.EqRoot)?"Choose EverQuest directory":_settings.EqRoot;if(!string.IsNullOrWhiteSpace(_settings.EqRoot)&&Directory.Exists(_settings.EqRoot)){DiscoverZones();if(!string.IsNullOrWhiteSpace(_settings.LastZone))await OpenZoneAsync(_settings.LastZone);}}
 void DiscoverZones(){AssetTree.Items.Clear();foreach(var p in Directory.EnumerateFiles(_settings.EqRoot!,"*.s3d",SearchOption.TopDirectoryOnly).Where(x=>!Path.GetFileNameWithoutExtension(x).EndsWith("_obj",StringComparison.OrdinalIgnoreCase)&&!Path.GetFileNameWithoutExtension(x).EndsWith("_obj2",StringComparison.OrdinalIgnoreCase)).OrderBy(Path.GetFileName)){var z=Path.GetFileNameWithoutExtension(p);var item=new System.Windows.Controls.TreeViewItem{Header=z,Tag=z};item.MouseDoubleClick+=async(_,_)=>await OpenZoneAsync(z);AssetTree.Items.Add(item);}}
 async Task OpenZoneAsync(string zone){try{StatusText.Text=$"Loading {zone}...";_session=new(new SageClientZoneProvider(),new NoServerDataProvider(),new SceneTreeViewport(SceneTree));var loaded=await _session.OpenAsync(_settings.EqRoot!,zone);_settings=_settings with{LastZone=zone};ZoneText.Text=$"{zone}  |  {loaded.Entities.Count} entities";StatusText.Text=$"Loaded {zone}";await PersistAsync();}catch(Exception ex){StatusText.Text=$"Load failed: {ex.Message}";}}
 Task PersistAsync()=>_workspace.SaveAsync(_settings);
 void OpenZone_Click(object sender,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(_settings.EqRoot)||!Directory.Exists(_settings.EqRoot)){StatusText.Text="Configure a valid EverQuest directory before opening a zone.";return;}DiscoverZones();StatusText.Text="Double-click a zone in EQ Asset Browser.";}
 void CloseZone_Click(object sender,RoutedEventArgs e){_session?.Close();SceneTree.Items.Clear();ZoneText.Text="No zone loaded";}
 void Undo_Click(object sender,RoutedEventArgs e){if(_history.Undo())StatusText.Text="Undo";}
 void Redo_Click(object sender,RoutedEventArgs e){if(_history.Redo())StatusText.Text="Redo";}
 void Exit_Click(object sender,RoutedEventArgs e)=>Close();
}

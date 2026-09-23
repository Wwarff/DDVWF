using DDVWF.Core.Zone;
namespace DDVWF.App;
public sealed class SceneTreeViewport : IRenderViewport
{
 private readonly System.Windows.Controls.TreeView _tree;
 private CompleteZone? _zone;
 public SceneTreeViewport(System.Windows.Controls.TreeView tree)=>_tree=tree;
 public Task LoadAsync(CompleteZone zone,CancellationToken cancellationToken){cancellationToken.ThrowIfCancellationRequested();_zone=zone;Rebuild();return Task.CompletedTask;}
 public void Upsert(ZoneEntity entity){if(_zone is null)return;Rebuild();}
 public void Remove(Guid id){if(_zone is null)return;Rebuild();}
 private void Rebuild(){if(_zone is null){_tree.Items.Clear();return;}_tree.Items.Clear();foreach(var g in _zone.Entities.GroupBy(x=>x.Kind).OrderBy(x=>x.Key)){var root=new System.Windows.Controls.TreeViewItem{Header=$"{g.Key} ({g.Count()})"};foreach(var e in g.OrderBy(x=>x.Name,StringComparer.OrdinalIgnoreCase).ThenBy(x=>x.Id))root.Items.Add(new System.Windows.Controls.TreeViewItem{Header=e.Name,Tag=e.Id});_tree.Items.Add(root);}}
}

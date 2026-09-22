using DDVWF.Core.Zone;
namespace DDVWF.App;
public sealed class SceneTreeViewport : IRenderViewport
{
 private readonly System.Windows.Controls.TreeView _tree;
 public SceneTreeViewport(System.Windows.Controls.TreeView tree)=>_tree=tree;
 public Task LoadAsync(CompleteZone zone,CancellationToken cancellationToken){_tree.Items.Clear();foreach(var g in zone.Entities.GroupBy(x=>x.Kind).OrderBy(x=>x.Key)){var root=new System.Windows.Controls.TreeViewItem{Header=$"{g.Key} ({g.Count()})"};foreach(var e in g.OrderBy(x=>x.Name))root.Items.Add(new System.Windows.Controls.TreeViewItem{Header=e.Name,Tag=e.Id});_tree.Items.Add(root);}return Task.CompletedTask;}
 public void Upsert(ZoneEntity entity){}
 public void Remove(Guid id){}
}

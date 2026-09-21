using System.Windows;
using DDVWF.Core.Commands;
using DDVWF.Core.Workspace;

namespace DDVWF.App;

public partial class MainWindow : Window
{
    private readonly CommandHistory _history = new();
    private readonly JsonWorkspaceStore _workspace;
    private WorkspaceSettings _settings = new();

    public MainWindow()
    {
        InitializeComponent();
        var root = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DragonsDen", "DDVWF");
        _workspace = new JsonWorkspaceStore(System.IO.Path.Combine(root, "workspace.json"));
        Loaded += async (_, _) => await RestoreAsync();
        Closing += async (_, _) => await PersistAsync();
    }

    private async Task RestoreAsync()
    {
        _settings = await _workspace.LoadAsync();
        ZoneText.Text = string.IsNullOrWhiteSpace(_settings.LastZone) ? "No zone loaded" : $"Last zone: {_settings.LastZone}";
        StatusText.Text = string.IsNullOrWhiteSpace(_settings.EqRoot) ? "Choose EverQuest directory" : _settings.EqRoot;
    }

    private Task PersistAsync() => _workspace.SaveAsync(_settings);

    private void OpenZone_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Zone loader requires configured EQ root; no guessed path will be used.";
    }

    private void CloseZone_Click(object sender, RoutedEventArgs e) { ZoneText.Text = "No zone loaded"; }
    private void Undo_Click(object sender, RoutedEventArgs e) { if (_history.Undo()) StatusText.Text = "Undo"; }
    private void Redo_Click(object sender, RoutedEventArgs e) { if (_history.Redo()) StatusText.Text = "Redo"; }
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
}

using System.Text.Json;

namespace DDVWF.Core.Workspace;

public sealed class JsonWorkspaceStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public string Path { get; }

    public JsonWorkspaceStore(string path) => Path = path;

    public async Task<WorkspaceSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(Path)) return new WorkspaceSettings();
        await using var stream = File.OpenRead(Path);
        return await JsonSerializer.DeserializeAsync<WorkspaceSettings>(stream, JsonOptions, cancellationToken)
            ?? new WorkspaceSettings();
    }

    public async Task SaveAsync(WorkspaceSettings settings, CancellationToken cancellationToken = default)
    {
        var directory = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var temporary = Path + ".tmp";
        await using (var stream = File.Create(temporary))
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
        File.Move(temporary, Path, true);
    }
}

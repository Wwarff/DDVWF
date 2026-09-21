namespace DDVWF.Core.Workspace;

public sealed record CameraState(float X, float Y, float Z, float Yaw, float Pitch, float Speed = 20f);

public sealed record WorkspaceSettings
{
    public string? EqRoot { get; init; }
    public string? LastZone { get; init; }
    public CameraState Camera { get; init; } = new(0, 0, 0, 0, 0);
    public Dictionary<string, double> Dividers { get; init; } = new();
    public Dictionary<string, WindowState> Windows { get; init; } = new();
    public Dictionary<string, string> ActiveTabs { get; init; } = new();
}

public sealed record WindowState(double Left, double Top, double Width, double Height, bool Minimized = false);

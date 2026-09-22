namespace DDVWF.Core.Workspace;

public sealed record CameraState(float X, float Y, float Z, float Yaw, float Pitch, float Speed = 20f);

public sealed record ThemeSettings(string Accent="#9CFF80",string Separator="#6930A8",string Compass="#9CFF80",string ListText="#9CFF80");

public sealed record ServerConnectionSettings(string Host="127.0.0.1",int Port=3306,string Database="peq",string User="trinity",string Password="trinity",bool Enabled=true);

public sealed record WorkspaceSettings
{
    public string? EqRoot { get; init; }
    public string? LastZone { get; init; }
    public ServerConnectionSettings Server { get; init; } = new();
    public ThemeSettings Theme { get; init; } = new();
    public Dictionary<string,string> Shortcuts { get; init; } = new(){{"Select","1"},{"Move","2"},{"Rotate","3"},{"Scale","4"},{"Focus","F"},{"Delete","Delete"},{"Duplicate","Ctrl+D"},{"Stamp","Ctrl+S"}};
    public CameraState Camera { get; init; } = new(0, 0, 0, 0, 0);
    public Dictionary<string, double> Dividers { get; init; } = new();
    public Dictionary<string, WindowState> Windows { get; init; } = new();
    public Dictionary<string, string> ActiveTabs { get; init; } = new();
}

public sealed record WindowState(double Left, double Top, double Width, double Height, bool Minimized = false);

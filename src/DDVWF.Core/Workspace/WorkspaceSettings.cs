namespace DDVWF.Core.Workspace;

public sealed record CameraState(float X, float Y, float Z, float Yaw, float Pitch, float Speed = 20f);

public sealed record ThemeSettings(string Accent="#D7FF00",string Separator="#403050",string Compass="#D7FF00",string ListText="#D8D8DE",string WindowBackground="#020204",string PanelBackground="#100D18",string ControlBackground="#151020",string MutedText="#817A8A")
{
    public Dictionary<string,string> Palette { get; init; } = DefaultPalette();
    public static Dictionary<string,string> DefaultPalette()=>new(StringComparer.OrdinalIgnoreCase)
    {
        ["ViewportBrush"]="#020204",["TextBrush"]="#D7FF00",["MutedTextBrush"]="#817A8A",["PanelBrush"]="#100D18",["Panel2Brush"]="#151020",["MenuBrush"]="#08070C",["MenuTextBrush"]="#D7FF00",["ButtonBrush"]="#151020",["ButtonHoverBrush"]="#282038",["InputBrush"]="#08070C",["AccentBrush"]="#D7FF00",["DividerBrush"]="#403050",["ControlBorderBrush"]="#403050",["SelectionBrush"]="#5871B8",["SelectionTextBrush"]="#F0F0F0",["TabBrush"]="#151020",["TabSelectedBrush"]="#282038",["PopupBrush"]="#08070C",["PopupHoverBrush"]="#282038",["ScrollTrackBrush"]="#08070C",["ScrollThumbBrush"]="#403050",["ListTextBrush"]="#D8D8DE",["ListSelectedTextBrush"]="#FFFFFF",["ListSelectedBackgroundBrush"]="#604A78",["ListInactiveSelectedTextBrush"]="#D8D8DE",["ListInactiveSelectedBackgroundBrush"]="#342A40",["StatusBrush"]="#08070C",["ViewportHintBrush"]="#817A8A",["TitleBarBrush"]="#08070C",["TitleBarTextBrush"]="#D7FF00",["TitleBarButtonHoverBrush"]="#282038",["PickerBackground"]="#1F2121",["PickerControlBackground"]="#282B2B",["PickerFont"]="#EBEBEB",["PickerMutedFont"]="#B4B8B8",["PickerBorder"]="#464A4A",["PickerSelectorArrow"]="#EBEBEB",["PickerSelectorArrowOutline"]="#000000",["ModelLibraryBackgroundBrush"]="#100D18",["ModelLibraryTextBrush"]="#D7FF00",["ModelLibrarySelectionBrush"]="#282038",["ModelPreviewBackgroundBrush"]="#020204",["ModelPreviewTextBrush"]="#D7FF00",["ModelCategoryBackgroundBrush"]="#100D18",["ModelCategoryTextBrush"]="#D7FF00",["ModelCategoryBorderBrush"]="#403050",["ModelCategoryDropBackgroundBrush"]="#100D18",["ModelCategoryDropTextBrush"]="#D7FF00",["ModelCategoryHoverBrush"]="#282038",["ModelCategorySelectedBrush"]="#403050",["CompassStarBrush"]="#E8D9B5",["CompassPrimaryBrush"]="#A68A6A",["CompassSecondaryBrush"]="#6E5947",["CompassAccentBrush"]="#D7FF00"
    };
}

public sealed record ServerConnectionSettings(string Host="127.0.0.1",int Port=3306,string Database="eqemu_peq",string User="trinity",string Password="trinity",bool Enabled=true);

public sealed record WorkspaceSettings
{
    public string? EqRoot { get; init; }
    public string? LastZone { get; init; }
    public ServerConnectionSettings Server { get; init; } = new();
    public ThemeSettings Theme { get; init; } = new();
    public Dictionary<string,string> Shortcuts { get; init; } = ShortcutDefaults();\n    public static Dictionary<string,string> ShortcutDefaults()=>new(StringComparer.OrdinalIgnoreCase){{"Select","Escape"},{"Move","NumPad1"},{"Rotate","NumPad2"},{"Scale","NumPad3"},{"Snap","NumPad4"},{"Focus","NumPad5"},{"ResetSelection","NumPad6"},{"Undo","NumPad7"},{"Redo","NumPad8"},{"ResetAll","NumPad9"},{"Release","NumPad0"},{"CameraUp","PageUp"},{"CameraDown","PageDown"},{"Duplicate","Ctrl+D"},{"Delete","Delete"},{"Lock","Ctrl+L"},{"Hide","H"},{"Commit","Ctrl+Enter"},{"Cancel","Ctrl+Escape"},{"Stamp","Ctrl+S"},{"Continuous","Ctrl+T"},{"Randomize","Ctrl+R"},{"Fullbright","Ctrl+F"},{"Lit","Ctrl+I"},{"ObjectLibrary","Ctrl+O"},{"EditScene","Alt+S"},{"EditTerrain","Alt+T"},{"EditWater","Alt+W"},{"EditRegions","Alt+R"},{"EditLights","Alt+L"},{"EditSounds","Alt+U"},{"EditAll","Alt+A"}};
    public CameraState Camera { get; init; } = new(0, 0, 0, 0, 0);
    public Dictionary<string, CameraState> ModelCameras { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double> Dividers { get; init; } = new();
    public Dictionary<string, WindowState> Windows { get; init; } = new();
    public Dictionary<string, string> ActiveTabs { get; init; } = new();
}

public sealed record WindowState(double Left, double Top, double Width, double Height, bool Minimized = false);

using MTS.Core.Enums;

namespace MTS.Core.Models.Settings;

public class UiPreferences
{
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public string FormFillBrowser { get; set; } = "Default";
}

using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Settings;

/// <summary>
/// Observable wrapper for a FailReason lookup item.
/// Preserves IsOther, AppliesToCalls, and AppliesToSupTransfers flags
/// so the editor can control how fail reasons appear during grading.
/// </summary>
public partial class FailReasonRowViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private bool _isOther;

    [ObservableProperty]
    private bool _appliesToCalls = true;

    [ObservableProperty]
    private bool _appliesToSupTransfers;

    [ObservableProperty]
    private bool _isEnabled = true;

    public string DisplayLabel => string.IsNullOrWhiteSpace(Label) ? "(new fail reason)" : Label;

    partial void OnLabelChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    // ---- Factory ----

    public static FailReasonRowViewModel FromDomain(FailReason fr) => new()
    {
        Id                    = fr.Id,
        Label                 = fr.Label,
        IsOther               = fr.IsOther,
        AppliesToCalls        = fr.AppliesToCalls,
        AppliesToSupTransfers = fr.AppliesToSupTransfers,
        IsEnabled             = fr.IsEnabled
    };

    public FailReason ToDomain() => new()
    {
        Id                    = Id,
        Label                 = Label,
        IsOther               = IsOther,
        AppliesToCalls        = AppliesToCalls,
        AppliesToSupTransfers = AppliesToSupTransfers,
        IsEnabled             = IsEnabled
    };
}

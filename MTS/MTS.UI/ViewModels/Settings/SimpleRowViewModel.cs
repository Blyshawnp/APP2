using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Settings;

/// <summary>
/// Observable wrapper for any simple label-only lookup item (e.g. SupervisorReason).
/// Carries IsEnabled so the user can disable without deleting.
/// </summary>
public partial class SimpleRowViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private bool _isEnabled = true;

    public string DisplayLabel => string.IsNullOrWhiteSpace(Label) ? "(new item)" : Label;

    partial void OnLabelChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    // ---- Factory ----

    public static SimpleRowViewModel FromSupervisorReason(SupervisorReason r) => new()
    {
        Id        = r.Id,
        Label     = r.Label,
        IsEnabled = r.IsEnabled
    };

    public SupervisorReason ToSupervisorReason() => new()
    {
        Id        = Id,
        Label     = Label,
        IsEnabled = IsEnabled
    };
}

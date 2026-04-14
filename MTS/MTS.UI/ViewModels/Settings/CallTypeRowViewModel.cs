using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Enums;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Settings;

/// <summary>
/// Observable wrapper for a CallType lookup item.
/// Exposes CategoryOptions so the editor can bind a ComboBox without putting enum logic in XAML.
/// </summary>
public partial class CallTypeRowViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private CallTypeCategory _category;

    [ObservableProperty]
    private bool _isEnabled = true;

    public string DisplayLabel => string.IsNullOrWhiteSpace(Label) ? "(new call type)" : Label;

    /// <summary>Provides the full enum value list for a ComboBox in the detail form.</summary>
    public IReadOnlyList<CallTypeCategory> CategoryOptions { get; } =
        Enum.GetValues<CallTypeCategory>().ToArray();

    partial void OnLabelChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    // ---- Factory ----

    public static CallTypeRowViewModel FromDomain(CallType ct) => new()
    {
        Id        = ct.Id,
        Label     = ct.Label,
        Category  = ct.Category,
        IsEnabled = ct.IsEnabled
    };

    public CallType ToDomain() => new()
    {
        Id        = Id,
        Label     = Label,
        Category  = Category,
        IsEnabled = IsEnabled
    };
}

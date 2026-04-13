using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Settings;

/// <summary>
/// Observable wrapper for a Show lookup item.
/// Preserves the full Show structure (amounts, gift) rather than collapsing it
/// to a single label — shows have richer data that must be editable.
/// </summary>
public partial class ShowRowViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _oneTimeAmount;

    [ObservableProperty]
    private decimal _monthlyAmount;

    [ObservableProperty]
    private string _giftDescription = string.Empty;

    [ObservableProperty]
    private bool _isEnabled = true;

    public string DisplayLabel => string.IsNullOrWhiteSpace(Name) ? "(new show)" : Name;

    partial void OnNameChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    // ---- Factory ----

    public static ShowRowViewModel FromDomain(Show s) => new()
    {
        Id              = s.Id,
        Name            = s.Name,
        OneTimeAmount   = s.OneTimeAmount,
        MonthlyAmount   = s.MonthlyAmount,
        GiftDescription = s.GiftDescription,
        IsEnabled       = s.IsEnabled
    };

    public Show ToDomain() => new()
    {
        Id              = Id,
        Name            = Name,
        OneTimeAmount   = OneTimeAmount,
        MonthlyAmount   = MonthlyAmount,
        GiftDescription = GiftDescription,
        IsEnabled       = IsEnabled
    };
}

using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Calls;

/// <summary>
/// Wraps one FailReason from settings as a live, bindable form row.
/// BR-04: when IsOther and IsSelected, Notes becomes required.
/// BR-05: at least one FailItem must be selected when Result = Fail.
/// Both rules are enforced in ValidationService; this VM surfaces the data.
/// </summary>
public partial class FailItemViewModel : ObservableObject
{
    public FailReason Reason { get; }

    public Guid ReasonId => Reason.Id;
    public string Label  => Reason.Label;
    public bool IsOther  => Reason.IsOther;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOtherInput))]
    private bool _isSelected;

    [ObservableProperty]
    private string _notes = string.Empty;

    public bool ShowOtherInput => IsSelected && IsOther;

    public FailItemViewModel(FailReason reason)
        => Reason = reason;

    public Core.Models.Session.FailSelection ToSelection() => new()
    {
        ReasonId    = Reason.Id,
        ReasonLabel = Reason.Label,
        IsOther     = Reason.IsOther,
        Notes       = Notes
    };
}

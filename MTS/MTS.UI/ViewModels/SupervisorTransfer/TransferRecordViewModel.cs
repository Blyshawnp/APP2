using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Enums;
using MTS.Core.Models.Session;
using MTS.Core.Models.Settings;
using MTS.Core.Services;
using MTS.UI.Services;
using MTS.UI.ViewModels.Calls;

namespace MTS.UI.ViewModels.SupervisorTransfer;

/// <summary>
/// Manages the form state for one supervisor transfer attempt (1 or 2).
/// Mirrors CallRecordViewModel's pattern but for sup-transfer specific
/// coaching categories and fail reasons.
/// Supports three result states: Pass, Fail, and null (not yet attempted).
/// </summary>
public partial class TransferRecordViewModel : ObservableObject
{
    private readonly ValidationService _validation;
    private readonly ISoundService _sound;

    // -------------------------------------------------------------------------
    // Identity
    // -------------------------------------------------------------------------
    public int TransferNumber { get; }

    // -------------------------------------------------------------------------
    // Setup fields
    // -------------------------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScenarioText))]
    private Show? _selectedShow;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScenarioText))]
    private Donor? _selectedCaller;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailableCallers))]
    private CallType? _selectedCallType;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScenarioText))]
    private string _selectedReason = string.Empty;

    public string ScenarioText
    {
        get
        {
            string caller = SelectedCaller?.DisplayName ?? "[Caller]";
            string fname  = caller.Split(' ')[0];
            string reason = !string.IsNullOrEmpty(SelectedReason) ? SelectedReason.ToLowerInvariant() : "[reason]";
            return $"For this call you will portray {caller}. {fname} would like to speak with a supervisor. " +
                   $"The caller was {reason} during a previous call.";
        }
    }

    public ObservableCollection<Show> AvailableShows { get; } = new();
    public ObservableCollection<string> AvailableReasons { get; } = new();
    public ObservableCollection<Donor> AllDonors { get; } = new();

    public IEnumerable<Donor> AvailableCallers => SelectedCallType?.Category switch
    {
        CallTypeCategory.ExistingMember    => AllDonors.Where(_ => true), // all for transfer
        CallTypeCategory.IncreaseSustaining => AllDonors.Where(_ => true),
        _                                  => AllDonors
    };

    // -------------------------------------------------------------------------
    // Result (null = not yet attempted)
    // -------------------------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPassSelected))]
    [NotifyPropertyChangedFor(nameof(IsFailSelected))]
    [NotifyPropertyChangedFor(nameof(ShowFollowUpSection))]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    private CallResult? _result;

    public bool IsPassSelected     => Result == CallResult.Pass;
    public bool IsFailSelected     => Result == CallResult.Fail;
    public bool HasResult          => Result.HasValue;

    /// <summary>Follow-up fields (coaching + fail reasons) appear after a Fail result.</summary>
    public bool ShowFollowUpSection => Result == CallResult.Fail;

    // -------------------------------------------------------------------------
    // Coaching (sup-transfer specific categories)
    // -------------------------------------------------------------------------
    public ObservableCollection<CoachingItemViewModel> CoachingItems { get; } = new();
    [ObservableProperty] private string _coachingNotes = string.Empty;

    public bool HasAnyCoachingSelected => CoachingItems.Any(c => c.IsSelected);
    public bool HasOtherCoaching       => CoachingItems.Any(c => c.IsOther && c.IsSelected);

    // -------------------------------------------------------------------------
    // Fail reasons (sup-transfer specific)
    // -------------------------------------------------------------------------
    public ObservableCollection<FailItemViewModel> FailItems { get; } = new();
    [ObservableProperty] private string _failNotes = string.Empty;

    public bool HasOtherFail => FailItems.Any(f => f.IsOther && f.IsSelected);

    // -------------------------------------------------------------------------
    // Validation / state
    // -------------------------------------------------------------------------
    [ObservableProperty] private ObservableCollection<string> _validationErrors = new();
    [ObservableProperty] private bool _hasCoachingSkippedWarning;
    [ObservableProperty] private bool _isCompleted;
    [ObservableProperty] private bool _isVisible = true;

    public bool IsValid => ValidationErrors.Count == 0;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------
    public TransferRecordViewModel(
        int transferNumber,
        AppSettings settings,
        ValidationService validation,
        ISoundService sound)
    {
        TransferNumber = transferNumber;
        _validation    = validation;
        _sound         = sound;

        // Enabled items only surface in the UI
        foreach (var s in settings.Shows.Where(x => x.IsEnabled))
            AvailableShows.Add(s);
        foreach (var r in settings.SupervisorReasons.Where(x => x.IsEnabled))
            AvailableReasons.Add(r.Label);

        // All enabled donors available for sup transfers (all three categories)
        foreach (var d in settings.Donors.NewDonors.Where(x => x.IsEnabled))
            AllDonors.Add(d);
        foreach (var d in settings.Donors.ExistingMembers.Where(x => x.IsEnabled))
            AllDonors.Add(d);
        foreach (var d in settings.Donors.IncreaseSustaining.Where(x => x.IsEnabled))
            AllDonors.Add(d);

        // Only enabled sup-transfer coaching categories
        foreach (var c in settings.CoachingCategories.Where(c => c.AppliesToSupTransfers && c.IsEnabled))
            CoachingItems.Add(new CoachingItemViewModel(c));

        // Only enabled sup-transfer fail reasons
        foreach (var r in settings.FailReasons.Where(r => r.AppliesToSupTransfers && r.IsEnabled))
            FailItems.Add(new FailItemViewModel(r));
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void SelectPass()
    {
        Result = CallResult.Pass;
        Validate();
        _sound.PlaySuccess();
    }

    [RelayCommand]
    private void SelectFail()
    {
        Result = CallResult.Fail;
        Validate();
        _sound.PlayError();
    }

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------
    public bool Validate()
    {
        var record = BuildDomainRecord();
        var result = _validation.ValidateSupTransferRecord(record);

        ValidationErrors.Clear();
        foreach (var e in result.Errors)
            ValidationErrors.Add(e);

        HasCoachingSkippedWarning = Result.HasValue && !HasAnyCoachingSelected;
        OnPropertyChanged(nameof(IsValid));
        return result.IsValid;
    }

    // -------------------------------------------------------------------------
    // Domain conversion
    // -------------------------------------------------------------------------
    public SupTransferRecord BuildDomainRecord() => new()
    {
        TransferNumber    = TransferNumber,
        ShowId            = SelectedShow?.Id ?? Guid.Empty,
        ShowName          = SelectedShow?.Name ?? string.Empty,
        CallerId          = SelectedCaller?.Id ?? Guid.Empty,
        CallerDisplayName = SelectedCaller?.DisplayName ?? string.Empty,
        Reason            = SelectedReason,
        Result            = Result,
        CoachingItems     = CoachingItems
            .Where(c => c.IsSelected)
            .Select(c => c.ToSelection())
            .ToList(),
        CoachingNotes     = CoachingNotes,
        FailItems         = FailItems
            .Where(f => f.IsSelected)
            .Select(f => f.ToSelection())
            .ToList(),
        FailNotes         = FailNotes
    };
}
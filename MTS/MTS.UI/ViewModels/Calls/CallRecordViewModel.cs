using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Enums;
using MTS.Core.Models.Session;
using MTS.Core.Models.Settings;
using MTS.Core.Services;
using MTS.UI.Services;

namespace MTS.UI.ViewModels.Calls;

/// <summary>Randomly-generated payment simulation data shown on the call card.</summary>
public record PaymentSimData(
    string CardNumber,
    string CardExpiry,
    string CardCvv,
    string BankRouting,
    string BankAccount);

/// <summary>
/// Manages the form state for one mock call (1, 2, or 3).
/// Holds all editable fields, coaching/fail item collections,
/// validation errors, and the submission logic for that call.
/// Business rules are delegated to ValidationService.
/// No persistence logic — the parent CallsViewModel handles saves.
/// </summary>
public partial class CallRecordViewModel : ObservableObject
{
    private readonly ValidationService _validation;
    private readonly ISoundService _sound;

    // -------------------------------------------------------------------------
    // Identity
    // -------------------------------------------------------------------------
    public int CallNumber { get; }

    // -------------------------------------------------------------------------
    // Setup fields
    // -------------------------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailableCallers))]
    private CallType? _selectedCallType;

    [ObservableProperty]
    private Show? _selectedShow;

    [ObservableProperty]
    private Donor? _selectedCaller;

    [ObservableProperty]
    private decimal _donationAmount;

    [ObservableProperty]
    private ScenarioFlags _scenarioFlags = new();

    // -------------------------------------------------------------------------
    // Available options (populated from Settings by parent ViewModel)
    // -------------------------------------------------------------------------
    public ObservableCollection<CallType> AvailableCallTypes { get; } = new();
    public ObservableCollection<Show> AvailableShows { get; } = new();
    public ObservableCollection<Donor> NewDonors { get; } = new();
    public ObservableCollection<Donor> ExistingMembers { get; } = new();
    public ObservableCollection<Donor> IncreaseSustaining { get; } = new();

    /// <summary>Callers filtered to the current call type category.</summary>
    public IEnumerable<Donor> AvailableCallers => SelectedCallType?.Category switch
    {
        CallTypeCategory.NewDonor         => NewDonors,
        CallTypeCategory.ExistingMember   => ExistingMembers,
        CallTypeCategory.IncreaseSustaining => IncreaseSustaining,
        _                                 => NewDonors
    };

    // -------------------------------------------------------------------------
    // Result
    // -------------------------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPassSelected))]
    [NotifyPropertyChangedFor(nameof(IsFailSelected))]
    [NotifyPropertyChangedFor(nameof(ShowFailSection))]
    private CallResult? _result;

    public bool IsPassSelected => Result == CallResult.Pass;
    public bool IsFailSelected => Result == CallResult.Fail;
    public bool ShowFailSection => Result == CallResult.Fail;

    // -------------------------------------------------------------------------
    // Coaching
    // -------------------------------------------------------------------------
    public ObservableCollection<CoachingItemViewModel> CoachingItems { get; } = new();

    [ObservableProperty]
    private string _coachingNotes = string.Empty;

    public bool HasAnyCoachingSelected => CoachingItems.Any(c => c.IsSelected);
    public bool HasOtherCoaching => CoachingItems.Any(c => c.IsOther && c.IsSelected);

    // -------------------------------------------------------------------------
    // Fail reasons
    // -------------------------------------------------------------------------
    public ObservableCollection<FailItemViewModel> FailItems { get; } = new();

    [ObservableProperty]
    private string _failNotes = string.Empty;

    public bool HasOtherFail => FailItems.Any(f => f.IsOther && f.IsSelected);

    // -------------------------------------------------------------------------
    // Payment simulation (random per call, read-only display)
    // -------------------------------------------------------------------------
    public PaymentSimData PaymentSim { get; }

    // -------------------------------------------------------------------------
    // Scenario text (computed from caller + show + call type + flags)
    // -------------------------------------------------------------------------
    public string ScenarioText => GenerateScenarioText();

    // -------------------------------------------------------------------------
    // Validation / state
    // -------------------------------------------------------------------------
    [ObservableProperty]
    private ObservableCollection<string> _validationErrors = new();

    [ObservableProperty]
    private bool _hasCoachingSkippedWarning;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _isVisible = true;

    public bool IsValid => ValidationErrors.Count == 0;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------
    public CallRecordViewModel(
        int callNumber,
        AppSettings settings,
        ValidationService validation,
        ISoundService sound)
    {
        CallNumber  = callNumber;
        _validation = validation;
        _sound      = sound;

        // Populate lookup collections from settings — only enabled items surface in the UI
        foreach (var ct in settings.CallTypes.Where(x => x.IsEnabled))
            AvailableCallTypes.Add(ct);
        foreach (var s in settings.Shows.Where(x => x.IsEnabled))
            AvailableShows.Add(s);
        foreach (var d in settings.Donors.NewDonors.Where(x => x.IsEnabled))
            NewDonors.Add(d);
        foreach (var d in settings.Donors.ExistingMembers.Where(x => x.IsEnabled))
            ExistingMembers.Add(d);
        foreach (var d in settings.Donors.IncreaseSustaining.Where(x => x.IsEnabled))
            IncreaseSustaining.Add(d);

        // Coaching categories applicable to calls (enabled only)
        foreach (var c in settings.CoachingCategories.Where(c => c.AppliesToCalls && c.IsEnabled))
            CoachingItems.Add(new CoachingItemViewModel(c));

        // Fail reasons applicable to calls (enabled only)
        foreach (var r in settings.FailReasons.Where(r => r.AppliesToCalls && r.IsEnabled))
            FailItems.Add(new FailItemViewModel(r));

        ScenarioFlags = ScenarioFlags.GenerateRandom();
        PaymentSim    = GeneratePaymentSim();
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void SelectPass()
    {
        Result = CallResult.Pass;
        ValidateAndRefresh();
        _sound.PlaySuccess();
    }

    [RelayCommand]
    private void SelectFail()
    {
        Result = CallResult.Fail;
        ValidateAndRefresh();
        _sound.PlayError();
    }

    [RelayCommand]
    private void RegenerateScenario()
        => ScenarioFlags = ScenarioFlags.GenerateRandom();

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs full validation and returns true when the call is ready to submit.
    /// Populates ValidationErrors and HasCoachingSkippedWarning.
    /// </summary>
    public bool Validate()
    {
        var record  = BuildDomainRecord();
        var result  = _validation.ValidateCallRecord(record);

        ValidationErrors.Clear();
        foreach (var e in result.Errors)
            ValidationErrors.Add(e);

        HasCoachingSkippedWarning = Result.HasValue && !HasAnyCoachingSelected;
        OnPropertyChanged(nameof(IsValid));
        return result.IsValid;
    }

    private void ValidateAndRefresh()
    {
        if (IsCompleted) return;  // don't re-validate already-submitted calls
        Validate();
    }

    // -------------------------------------------------------------------------
    // Domain conversion
    // -------------------------------------------------------------------------

    /// <summary>Builds the immutable domain record for persistence.</summary>
    public CallRecord BuildDomainRecord()
    {
        return new CallRecord
        {
            CallNumber      = CallNumber,
            CallTypeCategory = SelectedCallType?.Category ?? CallTypeCategory.NewDonor,
            CallTypeLabel   = SelectedCallType?.Label ?? string.Empty,
            ShowId          = SelectedShow?.Id ?? Guid.Empty,
            ShowName        = SelectedShow?.Name ?? string.Empty,
            CallerId        = SelectedCaller?.Id ?? Guid.Empty,
            CallerDisplayName = SelectedCaller?.DisplayName ?? string.Empty,
            DonationAmount  = DonationAmount,
            ScenarioFlags   = ScenarioFlags,
            Result          = Result,
            CoachingItems   = CoachingItems
                .Where(c => c.IsSelected)
                .Select(c => c.ToSelection())
                .ToList(),
            CoachingNotes   = CoachingNotes,
            FailItems       = FailItems
                .Where(f => f.IsSelected)
                .Select(f => f.ToSelection())
                .ToList(),
            FailNotes       = FailNotes
        };
    }

    // -------------------------------------------------------------------------
    // Property change side-effects
    // -------------------------------------------------------------------------

    partial void OnSelectedCallTypeChanged(CallType? value)
    {
        // Clear caller selection when type changes so stale data isn't submitted
        SelectedCaller = null;
        OnPropertyChanged(nameof(AvailableCallers));
    }

    partial void OnSelectedShowChanged(Show? value)
    {
        if (value != null)
            DonationAmount = value.OneTimeAmount;
    }

    // -------------------------------------------------------------------------
    // Payment simulation generator
    // -------------------------------------------------------------------------

    private static PaymentSimData GeneratePaymentSim()
    {
        var rng = Random.Shared;

        // AmEx: 15 digits, 4-6-5 grouping
        static string AmexBlock(int len) =>
            string.Concat(Enumerable.Range(0, len).Select(_ => rng.Next(0, 10).ToString()));

        string card   = $"3{rng.Next(4, 8)} {AmexBlock(6)} {AmexBlock(5)}";
        string expiry = $"{rng.Next(1, 13):D2}/{rng.Next(26, 30)}";
        string cvv    = $"{rng.Next(1000, 9999)}";

        // Bank draft
        string routing = string.Concat(Enumerable.Range(0, 9).Select(_ => rng.Next(0, 10).ToString()));
        string account = string.Concat(Enumerable.Range(0, rng.Next(8, 13)).Select(_ => rng.Next(0, 10).ToString()));

        return new PaymentSimData(card, expiry, cvv, routing, account);
    }

    // -------------------------------------------------------------------------
    // Scenario text generator
    // -------------------------------------------------------------------------

    private string GenerateScenarioText()
    {
        var parts = new List<string>();

        string caller   = SelectedCaller?.DisplayName ?? "[Caller]";
        string show     = SelectedShow?.Name ?? "[Show]";
        string callType = SelectedCallType?.Label ?? "[Call Type]";

        parts.Add($"{caller} is calling in about {show}.");
        parts.Add($"Call type: {callType}.");

        if (ScenarioFlags.HasPhone)    parts.Add("Caller prefers phone contact.");
        if (ScenarioFlags.HasSms)      parts.Add("Caller has opted in to SMS.");
        if (ScenarioFlags.HasEnews)    parts.Add("Caller wants to receive e-news.");
        if (ScenarioFlags.HasShipping) parts.Add("Shipping address needed.");
        if (ScenarioFlags.HasCcFee)    parts.Add("Credit card processing fee applies.");

        return string.Join(" ", parts);
    }
}

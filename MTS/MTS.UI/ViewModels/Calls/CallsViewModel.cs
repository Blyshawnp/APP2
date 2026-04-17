using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Enums;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Settings;
using MTS.Core.Rules;
using MTS.Core.Services;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;
using MTS.UI.ViewModels;

namespace MTS.UI.ViewModels.Calls;

/// <summary>
/// Orchestrates the mock-call grading workflow (calls 1–3).
///
/// Business rules enforced here via EvaluationRulesService:
///   BR-01 – Needs 1 NewDonor pass + 1 ExistingMember pass to qualify
///   BR-02 – 2 fails = auto-fail session
///   BR-03 – If call 1 AND call 2 both pass, call 3 is hidden
///
/// No validation logic lives in this class — ValidationService owns that.
/// No persistence logic lives in CallRecordViewModel — ISessionService owns that.
/// </summary>
public partial class CallsViewModel : ViewModelBase
{
    private readonly ISessionService _sessionService;
    private readonly ISessionStateService _sessionState;
    private readonly ISettingsService _settingsService;
    private readonly EvaluationRulesService _rules;
    private readonly ValidationService _validation;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialog;
    private readonly ISoundService _sound;

    // -------------------------------------------------------------------------
    // Per-call sub-ViewModels
    // -------------------------------------------------------------------------
    [ObservableProperty]
    private CallRecordViewModel? _call1;

    [ObservableProperty]
    private CallRecordViewModel? _call2;

    [ObservableProperty]
    private CallRecordViewModel? _call3;

    // -------------------------------------------------------------------------
    // Active call (one-at-a-time display)
    // -------------------------------------------------------------------------
    [ObservableProperty]
    private CallRecordViewModel? _activeCall;

    [ObservableProperty]
    private int _activeCallNumber = 1;

    /// <summary>True when Call 1 is submitted (used by step indicator).</summary>
    public bool IsCall1Done => Call1?.IsCompleted == true;
    /// <summary>True when Call 2 is submitted.</summary>
    public bool IsCall2Done => Call2?.IsCompleted == true;
    /// <summary>True when Call 3 is submitted.</summary>
    public bool IsCall3Done => Call3?.IsCompleted == true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercent))]
    private bool _isCall2Visible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercent))]
    private bool _isCall3Visible;

    [ObservableProperty]
    private bool _canProceed;

    [ObservableProperty]
    private int _progressPercent;

    [ObservableProperty]
    private ObservableCollection<string> _pageWarnings = new();

    [ObservableProperty]
    private string? _passTypeWarning;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------
    public CallsViewModel(
        ISessionService sessionService,
        ISessionStateService sessionState,
        ISettingsService settingsService,
        EvaluationRulesService rules,
        ValidationService validation,
        INavigationService nav,
        IDialogService dialog,
        ISoundService sound)
    {
        _sessionService  = sessionService;
        _sessionState    = sessionState;
        _settingsService = settingsService;
        _rules           = rules;
        _validation      = validation;
        _nav             = nav;
        _dialog          = dialog;
        _sound           = sound;
    }

    // -------------------------------------------------------------------------
    // Navigation lifecycle
    // -------------------------------------------------------------------------
    public override async Task OnNavigatedToAsync(object? parameter)
    {
        await ExecuteBusyAsync(async () =>
        {
            var settings = await _settingsService.LoadAsync();
            InitializeCalls(settings);
        }, "Loading call setup...");
    }

    private void InitializeCalls(AppSettings settings)
    {
        Call1 = new CallRecordViewModel(1, settings, _validation, _sound);
        Call2 = new CallRecordViewModel(2, settings, _validation, _sound);
        Call3 = new CallRecordViewModel(3, settings, _validation, _sound);

        Call2.IsVisible = false;
        Call3.IsVisible = false;

        IsCall2Visible = false;
        IsCall3Visible = false;

        ActiveCall       = Call1;
        ActiveCallNumber = 1;

        RefreshProgress();
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    /// <summary>Submits a completed call, applies rules, and advances the workflow.</summary>
    [RelayCommand]
    private async Task SubmitCall(CallRecordViewModel callVm)
    {
        if (callVm.IsCompleted) return;
        if (!callVm.Validate()) return;

        callVm.IsCompleted = true;
        var record = callVm.BuildDomainRecord();
        await _sessionService.SaveCallAsync(record);

        await EvaluateAfterCall();
    }

    [RelayCommand]
    private void Back() => _nav.NavigateTo<DashboardViewModel>();

    [RelayCommand]
    private async Task StoppedResponding()
    {
        bool confirmed = await _dialog.ShowDangerConfirmAsync(
            "Stopped Responding",
            "Mark candidate as Stopped Responding? This will auto-fail the session.",
            "Confirm");

        if (!confirmed) return;

        await _sessionService.SetAutoFailAsync(AutoFailReason.StoppedResponding);
        _nav.NavigateTo<ReviewViewModel>();
    }

    [RelayCommand]
    private async Task TechIssue()
    {
        await _dialog.ShowAlertAsync("Tech Issue", "Tech Issue wizard coming soon.");
    }

    [RelayCommand]
    private async Task Proceed()
    {
        if (!CanProceed) return;

        var session = _sessionState.CurrentSession;
        if (session == null) return;

        if (session.HasAutoFail)
        {
            NavigateToReview();
            return;
        }

        var completedCalls = session.Calls;

        if (_rules.IsAutoFailByCallCount(completedCalls))
        {
            await _sessionService.SetAutoFailAsync(AutoFailReason.NcNs);
            NavigateToReview();
            return;
        }

        bool canGoToSup = _rules.CanProceedToSupervisorTransfer(completedCalls);
        if (canGoToSup)
        {
            bool timeForSup = await _dialog.ShowConfirmAsync(
                "Proceed to Supervisor Transfer",
                "The candidate has enough call passes. Is there time to run supervisor transfers?",
                "Yes, continue");

            _sessionState.UpdateSession(s => s.TimeForSup = timeForSup);

            if (timeForSup)
                _nav.NavigateTo<SupervisorTransfer.SupervisorTransferViewModel>();
            else
                NavigateToReview();
        }
        else
        {
            NavigateToReview();
        }
    }

    // -------------------------------------------------------------------------
    // Rule evaluation
    // -------------------------------------------------------------------------

    private async Task EvaluateAfterCall()
    {
        var session = _sessionState.CurrentSession;
        if (session == null) return;

        var completedCalls = session.Calls.OrderBy(c => c.CallNumber).ToList();

        PageWarnings.Clear();

        // BR-02: auto-fail on 2 failed calls
        if (_rules.IsAutoFailByCallCount(completedCalls))
        {
            await _sessionService.SetAutoFailAsync(AutoFailReason.NcNs);
            PageWarnings.Add("Two calls have failed — the session is now an automatic fail.");
            CanProceed = true;
            RefreshProgress();
            return;
        }

        int completedCount = completedCalls.Count;

        OnPropertyChanged(nameof(IsCall1Done));
        OnPropertyChanged(nameof(IsCall2Done));
        OnPropertyChanged(nameof(IsCall3Done));

        if (completedCount == 1)
        {
            IsCall2Visible   = true;
            Call2!.IsVisible = true;
            ActiveCall       = Call2;
            ActiveCallNumber = 2;
        }
        else if (completedCount >= 2)
        {
            var call1Result = completedCalls.FirstOrDefault(c => c.CallNumber == 1)?.Result;
            var call2Result = completedCalls.FirstOrDefault(c => c.CallNumber == 2)?.Result;

            // BR-03
            bool hide3 = _rules.ShouldHideCall3(call1Result, call2Result);
            IsCall3Visible   = !hide3;
            Call3!.IsVisible = !hide3;

            if (!hide3 && completedCount < 3)
            {
                Call3.IsVisible  = true;
                ActiveCall       = Call3;
                ActiveCallNumber = 3;
            }

            PassTypeWarning = _rules.GetPassTypeWarning(completedCalls);
            if (PassTypeWarning != null)
                PageWarnings.Add(PassTypeWarning);

            bool canProc = _rules.CanProceedToSupervisorTransfer(completedCalls)
                        || _rules.IsAutoFailByCallCount(completedCalls)
                        || (hide3 && completedCount >= 2);
            CanProceed = canProc;
        }

        if (completedCount >= 3)
            CanProceed = true;

        RefreshProgress();
    }

    private void RefreshProgress()
    {
        var session = _sessionState.CurrentSession;
        if (session == null) { ProgressPercent = 0; return; }

        bool call3Hidden = !IsCall3Visible;
        ProgressPercent  = _rules.ComputeCallsProgress(session.Calls, call3Hidden);
    }

    private void NavigateToReview()
        => _nav.NavigateTo<ReviewViewModel>();
}

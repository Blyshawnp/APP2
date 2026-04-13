using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Interfaces.Services;
using MTS.Core.Rules;
using MTS.Core.Services;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;

namespace MTS.UI.ViewModels.SupervisorTransfer;

/// <summary>
/// Orchestrates the supervisor transfer workflow (up to 2 attempts).
///
/// Rules enforced via EvaluationRulesService:
///   – Transfer 2 appears only after Transfer 1 fails
///   – After Transfer 1 passes, Proceed becomes available immediately
///   – After both transfers fail, RequiresNewbieShift is surfaced
///
/// No business logic lives in the View.
/// </summary>
public partial class SupervisorTransferViewModel : ViewModelBase
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
    // Per-transfer sub-ViewModels
    // -------------------------------------------------------------------------
    [ObservableProperty] private TransferRecordViewModel? _transfer1;
    [ObservableProperty] private TransferRecordViewModel? _transfer2;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercent))]
    private bool _isTransfer2Visible;

    [ObservableProperty] private bool _canProceed;
    [ObservableProperty] private bool _requiresNewbieShift;
    [ObservableProperty] private int _progressPercent;
    [ObservableProperty] private ObservableCollection<string> _pageWarnings = new();

    // -------------------------------------------------------------------------
    public SupervisorTransferViewModel(
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
            Transfer1 = new TransferRecordViewModel(1, settings, _validation, _sound);
            Transfer2 = new TransferRecordViewModel(2, settings, _validation, _sound);
            Transfer2.IsVisible  = false;
            IsTransfer2Visible   = false;
            RefreshProgress();
        }, "Loading transfer setup...");
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    /// <summary>Submits a completed transfer attempt and evaluates the next step.</summary>
    [RelayCommand]
    private async Task SubmitTransfer(TransferRecordViewModel transferVm)
    {
        if (transferVm.IsCompleted) return;
        if (!transferVm.Validate()) return;

        transferVm.IsCompleted = true;
        var record = transferVm.BuildDomainRecord();
        await _sessionService.SaveSupTransferAsync(record);

        await EvaluateAfterTransfer();
    }

    /// <summary>Proceeds to the next screen based on current session state.</summary>
    [RelayCommand]
    private void Proceed()
    {
        if (!CanProceed) return;

        var session = _sessionState.CurrentSession;
        if (session == null) return;

        if (RequiresNewbieShift)
        {
            // Phase 4: navigate to NewbieShiftViewModel
            // For now navigate to Review as placeholder
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

    private async Task EvaluateAfterTransfer()
    {
        var session = _sessionState.CurrentSession;
        if (session == null) return;

        PageWarnings.Clear();

        int transfersDone = session.SupTransfers.Count;
        int supsPassed    = session.SupsPassed;

        if (supsPassed >= 1)
        {
            // At least one transfer passed — session can proceed to Review
            CanProceed        = true;
            RequiresNewbieShift = false;
            RefreshProgress();
            _sound.PlaySuccess();
            return;
        }

        if (transfersDone == 1)
        {
            // Transfer 1 failed — reveal Transfer 2
            IsTransfer2Visible   = true;
            Transfer2!.IsVisible = true;
            PageWarnings.Add("Transfer 1 failed. One more attempt is available.");
        }
        else if (transfersDone >= 2)
        {
            // Both failed
            var requiresShift = _rules.RequiresNewbieShift(session);
            RequiresNewbieShift = requiresShift;

            if (requiresShift)
            {
                PageWarnings.Add("Both supervisor transfers failed. A newbie shift must be scheduled.");
                _sound.PlayError();
            }

            CanProceed = true;
        }

        RefreshProgress();
        await Task.CompletedTask;
    }

    private void RefreshProgress()
    {
        var session = _sessionState.CurrentSession;
        if (session == null) { ProgressPercent = 0; return; }

        int totalTransfers = IsTransfer2Visible ? 2 : 1;
        int done           = session.SupTransfers.Count(t => t.IsCompleted);
        ProgressPercent    = totalTransfers == 0 ? 0
            : (int)Math.Round((double)done / totalTransfers * 100);
    }

    private void NavigateToReview()
        => _nav.NavigateTo<ReviewViewModel>();
}

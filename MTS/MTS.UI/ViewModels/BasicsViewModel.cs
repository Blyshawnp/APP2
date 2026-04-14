using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Enums;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Session;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;
using MTS.UI.ViewModels.Calls;

namespace MTS.UI.ViewModels;

/// <summary>
/// The Basics pre-check screen.  Collects candidate info and validates
/// headset / VPN / browser conditions before a session starts.
/// </summary>
public partial class BasicsViewModel : ViewModelBase
{
    private readonly ISettingsService _settings;
    private readonly ISessionService  _sessionService;
    private readonly INavigationService _nav;
    private readonly ISessionStateService _sessionState;
    private readonly IDialogService _dialog;

    // -------------------------------------------------------------------------
    // Tester / candidate info
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private string _testerName = string.Empty;

    [ObservableProperty]
    private string _candidateName = string.Empty;

    [ObservableProperty]
    private Pronoun _pronoun = Pronoun.They;

    [ObservableProperty]
    private bool _isFinalAttempt;

    [ObservableProperty]
    private bool _isSupervisorOnly;

    // -------------------------------------------------------------------------
    // Headset pre-check
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private bool? _isHeadsetUsb;

    [ObservableProperty]
    private bool? _hasNoiseCancelling;

    [ObservableProperty]
    private string _headsetBrand = string.Empty;

    // -------------------------------------------------------------------------
    // VPN pre-check
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private bool? _hasVpn;

    [ObservableProperty]
    private bool? _canTurnOffVpn;

    // -------------------------------------------------------------------------
    // Browser pre-check
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private bool? _isChromeDefault;

    [ObservableProperty]
    private bool? _areExtensionsOff;

    [ObservableProperty]
    private bool? _arePopUpsAllowed;

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public BasicsViewModel(
        ISettingsService settings,
        ISessionService sessionService,
        INavigationService nav,
        ISessionStateService sessionState,
        IDialogService dialog)
    {
        _settings      = settings;
        _sessionService = sessionService;
        _nav           = nav;
        _sessionState  = sessionState;
        _dialog        = dialog;
    }

    public override async Task OnNavigatedToAsync(object? parameter)
    {
        var appSettings = await _settings.LoadAsync();
        TesterName = appSettings.TesterProfile?.TesterName ?? string.Empty;
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    /// <summary>NC/NS — candidate never connected; auto-fail immediately.</summary>
    [RelayCommand]
    private async Task NcNs()
    {
        bool confirmed = await _dialog.ShowDangerConfirmAsync(
            "NC/NS Auto-Fail",
            "Mark this session as NC/NS (No Call / No Show)? This will auto-fail the session.",
            "Confirm NC/NS");

        if (!confirmed) return;

        await StartSessionWithAutoFail(AutoFailReason.NcNs);
    }

    /// <summary>Not Ready — candidate joined but was not prepared.</summary>
    [RelayCommand]
    private async Task NotReady()
    {
        bool confirmed = await _dialog.ShowDangerConfirmAsync(
            "Not Ready Auto-Fail",
            "Mark candidate as Not Ready? This will auto-fail the session.",
            "Confirm Not Ready");

        if (!confirmed) return;

        await StartSessionWithAutoFail(AutoFailReason.NotReady);
    }

    [RelayCommand]
    private async Task StoppedResponding()
    {
        bool confirmed = await _dialog.ShowDangerConfirmAsync(
            "Stopped Responding",
            "Mark candidate as Stopped Responding? This will auto-fail the session.",
            "Confirm");

        if (!confirmed) return;

        await StartSessionWithAutoFail(AutoFailReason.StoppedResponding);
    }

    [RelayCommand]
    private async Task TechIssue()
    {
        // TODO: open Tech Issue wizard dialog
        await _dialog.ShowAlertAsync("Tech Issue", "Tech Issue wizard coming soon.");
    }

    [RelayCommand]
    private async Task Continue()
    {
        if (!ValidateForm()) return;

        // Check for auto-fail conditions
        if (IsHeadsetUsb == false)
        {
            await StartSessionWithAutoFail(AutoFailReason.WrongHeadset);
            return;
        }

        if (HasVpn == true && CanTurnOffVpn == false)
        {
            await StartSessionWithAutoFail(AutoFailReason.VpnCantDisable);
            return;
        }

        if (IsChromeDefault == false)
        {
            await StartSessionWithAutoFail(AutoFailReason.BrowserNotDefault);
            return;
        }

        await ExecuteBusyAsync(async () =>
        {
            var candidate = new CandidateInfo
            {
                CandidateName = CandidateName.Trim(),
                Pronoun       = Pronoun,
                IsFinalAttempt = IsFinalAttempt
            };

            var session = await _sessionService.CreateSessionAsync(candidate, IsSupervisorOnly);

            // Persist pre-check data into the session
            session.PreChecks = new PreChecks
            {
                HeadsetUsb         = IsHeadsetUsb,
                NoiseCancelling    = HasNoiseCancelling,
                HeadsetBrand       = HeadsetBrand.Trim(),
                VpnPresent         = HasVpn,
                VpnCanDisable      = CanTurnOffVpn,
                ChromeIsDefault    = IsChromeDefault,
                ExtensionsDisabled = AreExtensionsOff,
                PopupsAllowed      = ArePopUpsAllowed
            };

            if (IsSupervisorOnly)
                _nav.NavigateTo<SupervisorTransfer.SupervisorTransferViewModel>();
            else
                _nav.NavigateTo<CallsViewModel>();
        });
    }

    [RelayCommand]
    private void Back() => _nav.NavigateTo<DashboardViewModel>();

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(CandidateName))
        {
            ValidationMessage = "Candidate name is required.";
            return false;
        }

        ValidationMessage = string.Empty;
        return true;
    }

    private async Task StartSessionWithAutoFail(AutoFailReason reason)
    {
        await ExecuteBusyAsync(async () =>
        {
            var candidate = new CandidateInfo
            {
                CandidateName  = string.IsNullOrWhiteSpace(CandidateName) ? "Unknown" : CandidateName.Trim(),
                Pronoun        = Pronoun,
                IsFinalAttempt = IsFinalAttempt
            };

            await _sessionService.CreateSessionAsync(candidate, IsSupervisorOnly);
            await _sessionService.SetAutoFailAsync(reason);
            _nav.NavigateTo<ReviewViewModel>();
        });
    }
}

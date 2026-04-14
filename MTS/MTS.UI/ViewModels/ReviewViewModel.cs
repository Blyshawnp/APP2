using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Enums;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Session;
using MTS.Core.Rules;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;

namespace MTS.UI.ViewModels;

/// <summary>
/// Manages the Review screen.
///
/// Key design constraints:
///   – NEVER auto-submits: FinishSessionCommand is the only path to completion.
///   – Summaries are editable text — user can override AI output before finishing.
///   – Regenerate commands call ISummaryService independently per summary type.
///   – All async operations run off the UI thread via ExecuteBusyAsync;
///     the UI stays responsive via IsBusy / IsGenerating indicators.
///   – ReadinessWarnings is computed fresh on every relevant state change.
/// </summary>
public partial class ReviewViewModel : ViewModelBase
{
    private readonly ISessionService _sessionService;
    private readonly ISessionStateService _sessionState;
    private readonly ISummaryService _summaryService;
    private readonly EvaluationRulesService _rules;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialog;
    private readonly IClipboardService _clipboard;
    private readonly INotificationService _notification;
    private readonly ISoundService _sound;

    // -------------------------------------------------------------------------
    // Session data (loaded on navigate)
    // -------------------------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CandidateName))]
    [NotifyPropertyChangedFor(nameof(IsSupervisorOnly))]
    [NotifyPropertyChangedFor(nameof(CallsPassed))]
    [NotifyPropertyChangedFor(nameof(SupsPassed))]
    [NotifyPropertyChangedFor(nameof(HasNewbieShift))]
    [NotifyPropertyChangedFor(nameof(HasAutoFail))]
    private Session? _session;

    public string CandidateName   => Session?.Candidate.CandidateName ?? string.Empty;
    public bool IsSupervisorOnly  => Session?.IsSupervisorOnly ?? false;
    public int CallsPassed        => Session?.CallsPassed ?? 0;
    public int SupsPassed         => Session?.SupsPassed ?? 0;
    public bool HasNewbieShift    => Session?.NewbieShift != null;
    public bool HasAutoFail       => Session?.HasAutoFail ?? false;
    public string AutoFailReason  => Session?.AutoFailReason?.ToString() ?? string.Empty;

    // -------------------------------------------------------------------------
    // Computed final status
    // -------------------------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusLabel))]
    [NotifyPropertyChangedFor(nameof(IsPass))]
    [NotifyPropertyChangedFor(nameof(IsFail))]
    [NotifyPropertyChangedFor(nameof(IsIncomplete))]
    private SessionStatus _computedStatus = SessionStatus.Draft;

    public string StatusLabel  => ComputedStatus.ToString().ToUpperInvariant();
    public bool IsPass         => ComputedStatus == SessionStatus.Pass;
    public bool IsFail         => ComputedStatus == SessionStatus.Fail;
    public bool IsIncomplete   => ComputedStatus == SessionStatus.Incomplete;

    // -------------------------------------------------------------------------
    // AI Summaries (editable — user can always override)
    // -------------------------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCoachingSummary))]
    private string _coachingSummary = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFailSummary))]
    private string _failSummary = string.Empty;

    public bool HasCoachingSummary => !string.IsNullOrWhiteSpace(CoachingSummary);
    public bool HasFailSummary     => !string.IsNullOrWhiteSpace(FailSummary);

    [ObservableProperty] private bool _isGeneratingCoachingSummary;
    [ObservableProperty] private bool _isGeneratingFailSummary;
    [ObservableProperty] private bool _isAiEnabled;

    // -------------------------------------------------------------------------
    // Readiness / finishing
    // -------------------------------------------------------------------------
    [ObservableProperty] private ObservableCollection<string> _readinessWarnings = new();
    [ObservableProperty] private bool _canFinish;
    [ObservableProperty] private bool _isFinishing;

    // -------------------------------------------------------------------------
    public ReviewViewModel(
        ISessionService sessionService,
        ISessionStateService sessionState,
        ISummaryService summaryService,
        EvaluationRulesService rules,
        INavigationService nav,
        IDialogService dialog,
        IClipboardService clipboard,
        INotificationService notification,
        ISoundService sound)
    {
        _sessionService  = sessionService;
        _sessionState    = sessionState;
        _summaryService  = summaryService;
        _rules           = rules;
        _nav             = nav;
        _dialog          = dialog;
        _clipboard       = clipboard;
        _notification    = notification;
        _sound           = sound;
    }

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------
    public override async Task OnNavigatedToAsync(object? parameter)
    {
        await ExecuteBusyAsync(async () =>
        {
            Session         = _sessionState.CurrentSession;
            IsAiEnabled     = _summaryService.IsEnabled;
            ComputedStatus  = Session != null ? _rules.ComputeFinalStatus(Session) : SessionStatus.Draft;

            // Restore any previously saved summaries from session
            if (Session != null)
            {
                CoachingSummary = Session.CoachingSummary;
                FailSummary     = Session.FailSummary;
            }

            EvaluateReadiness();

            // Auto-generate summaries if AI is enabled and summaries are empty
            if (IsAiEnabled && Session != null)
            {
                if (string.IsNullOrWhiteSpace(CoachingSummary))
                    await GenerateCoachingInternalAsync();
                if (string.IsNullOrWhiteSpace(FailSummary) && IsFail)
                    await GenerateFailInternalAsync();
            }

            // Play status sound
            if (IsPass)      _sound.PlaySuccess();
            else if (IsFail) _sound.PlayError();
        }, "Loading review...");
    }

    // -------------------------------------------------------------------------
    // Regenerate commands (independent per summary type)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Regenerates the coaching summary without affecting the fail summary.
    /// Does not block the fail summary field or any other part of the UI.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRegenerateCoaching))]
    private async Task RegenerateCoachingSummary()
    {
        if (Session == null || IsGeneratingCoachingSummary) return;

        IsGeneratingCoachingSummary = true;
        try
        {
            var result = await _summaryService.RegenerateCoachingSummaryAsync(Session);
            CoachingSummary = result;
            _sessionState.UpdateSession(s => s.CoachingSummary = result);
            _notification.ShowSuccess("Coaching summary regenerated.");
        }
        catch (Exception ex)
        {
            _notification.ShowError($"Failed to regenerate coaching summary: {ex.Message}");
        }
        finally
        {
            IsGeneratingCoachingSummary = false;
        }
    }

    private bool CanRegenerateCoaching() => IsAiEnabled && !IsGeneratingCoachingSummary;

    /// <summary>
    /// Regenerates the fail summary without affecting the coaching summary.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRegenerateFailSummary))]
    private async Task RegenerateFailSummary()
    {
        if (Session == null || IsGeneratingFailSummary) return;

        IsGeneratingFailSummary = true;
        try
        {
            var result = await _summaryService.RegenerateFailSummaryAsync(Session);
            FailSummary = result;
            _sessionState.UpdateSession(s => s.FailSummary = result);
            _notification.ShowSuccess("Fail summary regenerated.");
        }
        catch (Exception ex)
        {
            _notification.ShowError($"Failed to regenerate fail summary: {ex.Message}");
        }
        finally
        {
            IsGeneratingFailSummary = false;
        }
    }

    private bool CanRegenerateFailSummary() => IsAiEnabled && !IsGeneratingFailSummary;

    // -------------------------------------------------------------------------
    // Back navigation
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void Back() => _nav.GoBack();

    // -------------------------------------------------------------------------
    // Copy to clipboard
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void CopyReview()
    {
        if (Session == null) return;

        var sb = new StringBuilder();
        sb.AppendLine($"=== MOCK TEST REVIEW ===");
        sb.AppendLine($"Candidate: {CandidateName}");
        sb.AppendLine($"Tester:    {Session.TesterInfo.TesterName}");
        sb.AppendLine($"Status:    {StatusLabel}");
        sb.AppendLine($"Date:      {Session.CreatedAt:MM/dd/yyyy}");
        sb.AppendLine();
        sb.AppendLine($"Calls Passed: {CallsPassed}");
        sb.AppendLine($"Sups Passed:  {SupsPassed}");

        if (HasAutoFail)
        {
            sb.AppendLine();
            sb.AppendLine($"AUTO-FAIL: {AutoFailReason}");
        }

        if (HasCoachingSummary)
        {
            sb.AppendLine();
            sb.AppendLine("--- COACHING SUMMARY ---");
            sb.AppendLine(CoachingSummary);
        }

        if (HasFailSummary)
        {
            sb.AppendLine();
            sb.AppendLine("--- FAIL SUMMARY ---");
            sb.AppendLine(FailSummary);
        }

        _clipboard.SetText(sb.ToString());
        _notification.ShowSuccess("Review copied to clipboard.");
    }

    // -------------------------------------------------------------------------
    // Finish session — the ONLY way a session is completed (BR-10: no auto-submit)
    // -------------------------------------------------------------------------

    [RelayCommand(CanExecute = nameof(CanFinishSession))]
    private async Task FinishSession()
    {
        if (!CanFinish) return;

        bool confirmed = await _dialog.ShowConfirmAsync(
            "Finish Session",
            $"Finish this session for {CandidateName}? The result will be recorded as {StatusLabel}.",
            "Finish");

        if (!confirmed) return;

        IsFinishing = true;
        try
        {
            // Persist the edited summaries back to the session draft before finishing
            _sessionState.UpdateSession(s =>
            {
                s.CoachingSummary = CoachingSummary;
                s.FailSummary     = FailSummary;
            });

            var finished = await _sessionService.FinishSessionAsync();

            if (finished.Status == SessionStatus.Pass)
                _sound.PlaySuccess();

            _notification.ShowSuccess($"Session finished: {finished.Status}");
            _nav.ClearHistory();
            _nav.NavigateTo<DashboardViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to finish session: {ex.Message}";
            _notification.ShowError(ErrorMessage);
        }
        finally
        {
            IsFinishing = false;
        }
    }

    private bool CanFinishSession() => CanFinish && !IsFinishing;

    // -------------------------------------------------------------------------
    // Readiness evaluation
    // -------------------------------------------------------------------------

    private void EvaluateReadiness()
    {
        ReadinessWarnings.Clear();

        if (Session == null)
        {
            ReadinessWarnings.Add("No active session found.");
            CanFinish = false;
            return;
        }

        // Warn (soft) — not blockers
        if (!HasCoachingSummary)
            ReadinessWarnings.Add("Coaching summary is empty. You may still finish.");
        if (IsFail && !HasFailSummary)
            ReadinessWarnings.Add("Fail summary is empty. You may still finish.");

        // Always allow finish — readiness warnings are informational only (BR-10)
        CanFinish = true;
    }

    // -------------------------------------------------------------------------
    // Property change side-effects
    // -------------------------------------------------------------------------

    partial void OnCoachingSummaryChanged(string value)
        => EvaluateReadiness();

    partial void OnFailSummaryChanged(string value)
        => EvaluateReadiness();

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task GenerateCoachingInternalAsync()
    {
        if (Session == null || IsGeneratingCoachingSummary) return;
        IsGeneratingCoachingSummary = true;
        try
        {
            CoachingSummary = await _summaryService.GenerateCoachingSummaryAsync(Session);
            if (!string.IsNullOrWhiteSpace(CoachingSummary))
                _sessionState.UpdateSession(s => s.CoachingSummary = CoachingSummary);
        }
        catch { /* silent — user can regenerate manually */ }
        finally { IsGeneratingCoachingSummary = false; }
    }

    private async Task GenerateFailInternalAsync()
    {
        if (Session == null || IsGeneratingFailSummary) return;
        IsGeneratingFailSummary = true;
        try
        {
            FailSummary = await _summaryService.GenerateFailSummaryAsync(Session);
            if (!string.IsNullOrWhiteSpace(FailSummary))
                _sessionState.UpdateSession(s => s.FailSummary = FailSummary);
        }
        catch { /* silent */ }
        finally { IsGeneratingFailSummary = false; }
    }
}

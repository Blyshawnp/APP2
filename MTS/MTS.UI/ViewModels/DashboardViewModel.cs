using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.History;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;
using MTS.UI.ViewModels.SupervisorTransfer;

namespace MTS.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IHistoryService _history;
    private readonly ISettingsService _settings;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialog;

    [ObservableProperty]
    private HistoryStats _stats = new();

    [ObservableProperty]
    private ObservableCollection<SessionSummary> _recentSessions = new();

    [ObservableProperty]
    private bool _hasHistory;

    [ObservableProperty]
    private string _welcomeName = string.Empty;

    public DashboardViewModel(
        IHistoryService history,
        ISettingsService settings,
        INavigationService nav,
        IDialogService dialog)
    {
        _history  = history;
        _settings = settings;
        _nav      = nav;
        _dialog   = dialog;
    }

    public override async Task OnNavigatedToAsync(object? parameter)
        => await LoadDataAsync();

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    private async Task LoadData() => await LoadDataAsync();

    [RelayCommand]
    private void StartSession() => _nav.NavigateTo<BasicsViewModel>();

    [RelayCommand]
    private async Task StartSupervisorOnly()
    {
        bool conductedMockCalls = await _dialog.ShowConfirmAsync(
            "Supervisor Transfer Only",
            "Did you conduct this candidate's initial mock call session?",
            "Yes, I did");

        if (!conductedMockCalls)
        {
            _nav.NavigateTo<BasicsViewModel>("supervisorOnly");
            return;
        }

        // Smart Resume: find sessions with calls done but no supervisor transfers
        var appSettings = await _settings.LoadAsync();
        var testerName  = appSettings.TesterProfile?.TesterName ?? string.Empty;
        var resumable   = await _history.GetResumableAsync(testerName);

        if (resumable.Count == 0)
        {
            await _dialog.ShowAlertAsync(
                "No Resumable Sessions",
                "No prior sessions found with completed mock calls and no supervisor transfers.\n\nStarting a fresh session.");
            _nav.NavigateTo<BasicsViewModel>("supervisorOnly");
            return;
        }

        var picked = await _dialog.ShowPickerAsync(
            "Resume Candidate",
            "Select the candidate whose supervisor transfers you are completing:",
            resumable,
            s => $"{s.CandidateName}  —  {s.CreatedAt:MMM d, yyyy}");

        if (picked == null)
            return;

        bool sameSettings = await _dialog.ShowConfirmAsync(
            "Candidate Setup",
            $"Is {picked.CandidateName}'s headset and browser settings the same as their last session?",
            "Yes, same settings");

        if (sameSettings)
        {
            // Load the full session from history and restore it into session state
            var fullSession = await _history.GetByIdAsync(picked.Id);
            if (fullSession != null)
            {
                _nav.NavigateTo<SupervisorTransferViewModel>(fullSession);
                return;
            }

            await _dialog.ShowAlertAsync(
                "Session Not Found",
                "Could not load the previous session. Starting with fresh pre-checks.");
        }

        // Settings changed (or session not found) — re-run Basics in supervisor-only mode
        _nav.NavigateTo<BasicsViewModel>((picked.CandidateName, true));
    }

    [RelayCommand]
    private void OpenHistory() => _nav.NavigateTo<HistoryViewModel>();

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task LoadDataAsync()
    {
        await ExecuteBusyAsync(async () =>
        {
            var appSettings = await _settings.LoadAsync();
            WelcomeName = appSettings.TesterProfile?.TesterName ?? "Tester";

            Stats = await _history.GetStatsAsync();

            var all = await _history.GetAllAsync();
            RecentSessions.Clear();
            foreach (var s in all.Take(10))
                RecentSessions.Add(s);

            HasHistory = RecentSessions.Count > 0;
        });
    }
}

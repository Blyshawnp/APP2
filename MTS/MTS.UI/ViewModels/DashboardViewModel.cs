using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.History;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;
using MTS.UI.ViewModels.Calls;

namespace MTS.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IHistoryService _history;
    private readonly ISessionService _sessionService;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialog;

    [ObservableProperty]
    private HistoryStats _stats = new();

    [ObservableProperty]
    private ObservableCollection<SessionSummary> _recentSessions = new();

    [ObservableProperty]
    private bool _hasHistory;

    public DashboardViewModel(
        IHistoryService history,
        ISessionService sessionService,
        INavigationService nav,
        IDialogService dialog)
    {
        _history        = history;
        _sessionService = sessionService;
        _nav            = nav;
        _dialog         = dialog;
    }

    public override async Task OnNavigatedToAsync(object? parameter)
        => await LoadDataAsync();

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    private async Task LoadData() => await LoadDataAsync();

    [RelayCommand]
    private async Task StartSession()
    {
        // Phase 2 will wire this to BasicsViewModel.
        // For now navigate to Calls as a scaffold.
        _nav.NavigateTo<CallsViewModel>();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StartSupervisorOnly()
    {
        // Phase 4 will wire this to SupervisorTransferViewModel.
        await _dialog.ShowAlertAsync("Coming Soon", "Supervisor-only sessions will be available in the next phase.");
    }

    [RelayCommand]
    private void OpenHistory()
    {
        // Phase 6 will wire this to HistoryViewModel.
        // Placeholder — no-op for now.
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task LoadDataAsync()
    {
        await ExecuteBusyAsync(async () =>
        {
            Stats = await _history.GetStatsAsync();

            var all = await _history.GetAllAsync();
            RecentSessions.Clear();
            foreach (var s in all.Take(10))
                RecentSessions.Add(s);

            HasHistory = RecentSessions.Count > 0;
        });
    }
}

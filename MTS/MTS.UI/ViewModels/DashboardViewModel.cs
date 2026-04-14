using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.History;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;

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
    private void StartSupervisorOnly()
    {
        // Navigate to The Basics with supervisor-only flag
        _nav.NavigateTo<BasicsViewModel>();
        // The BasicsViewModel will handle supervisor-only via its own checkbox
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

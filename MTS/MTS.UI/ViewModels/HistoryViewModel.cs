using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.History;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;

namespace MTS.UI.ViewModels;

public partial class HistoryViewModel : ViewModelBase
{
    private readonly IHistoryService _history;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialog;

    private List<SessionSummary> _allSessions = new();

    [ObservableProperty]
    private HistoryStats _stats = new();

    [ObservableProperty]
    private ObservableCollection<SessionSummary> _sessions = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _hasHistory;

    public HistoryViewModel(
        IHistoryService history,
        INavigationService nav,
        IDialogService dialog)
    {
        _history = history;
        _nav     = nav;
        _dialog  = dialog;
    }

    public override async Task OnNavigatedToAsync(object? parameter)
        => await LoadDataAsync();

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    private async Task Refresh() => await LoadDataAsync();

    [RelayCommand]
    private void Search()
    {
        var q = SearchQuery.Trim();
        var filtered = string.IsNullOrWhiteSpace(q)
            ? _allSessions
            : _allSessions.Where(s =>
                s.CandidateName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.TesterName.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        Sessions.Clear();
        foreach (var s in filtered)
            Sessions.Add(s);

        HasHistory = Sessions.Count > 0;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        Search();
    }

    [RelayCommand]
    private async Task ClearAllHistory()
    {
        bool confirmed = await _dialog.ShowDangerConfirmAsync(
            "Clear History",
            "Delete all session history? This cannot be undone.",
            "Delete All");

        if (!confirmed) return;

        await ExecuteBusyAsync(async () =>
        {
            await _history.DeleteAllAsync();
            await LoadDataAsync();
        });
    }

    [RelayCommand]
    private void Back() => _nav.NavigateTo<DashboardViewModel>();

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task LoadDataAsync()
    {
        await ExecuteBusyAsync(async () =>
        {
            Stats       = await _history.GetStatsAsync();
            _allSessions = await _history.GetAllAsync();

            Sessions.Clear();
            foreach (var s in _allSessions)
                Sessions.Add(s);

            HasHistory = Sessions.Count > 0;
        });
    }
}

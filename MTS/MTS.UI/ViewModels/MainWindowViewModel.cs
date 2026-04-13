using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Interfaces.Services;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;
using MTS.UI.ViewModels.Calls;

namespace MTS.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _nav;
    private readonly ISessionStateService _sessionState;
    private readonly IDialogService _dialog;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private bool _isSessionActive;

    [ObservableProperty]
    private string _sessionCandidateName = string.Empty;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private bool _isSidebarExpanded = true;

    public MainWindowViewModel(
        INavigationService nav,
        ISessionStateService sessionState,
        IDialogService dialog)
    {
        _nav          = nav;
        _sessionState = sessionState;
        _dialog       = dialog;

        _nav.Navigated          += OnNavigated;
        _sessionState.SessionChanged += OnSessionChanged;

        AppVersion = Assembly.GetExecutingAssembly()
                             .GetName().Version?.ToString(3) ?? "1.0.0";
    }

    public void Initialize()
    {
        // Navigate to Dashboard on startup
        _nav.NavigateTo<DashboardViewModel>();
    }

    // -------------------------------------------------------------------------
    // Navigation commands (bound to sidebar buttons)
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void GoToDashboard() => _nav.NavigateTo<DashboardViewModel>();

    [RelayCommand]
    private void GoToCalls()
    {
        if (!_sessionState.IsSessionActive) return;
        _nav.NavigateTo<CallsViewModel>();
    }

    [RelayCommand]
    private async Task DiscardSession()
    {
        if (!_sessionState.IsSessionActive) return;

        bool confirmed = await _dialog.ShowDangerConfirmAsync(
            "Discard Session",
            "Are you sure you want to discard the current session? All unsaved progress will be lost.",
            "Discard");

        if (!confirmed) return;

        _sessionState.ClearSession();
        _nav.ClearHistory();
        _nav.NavigateTo<DashboardViewModel>();
    }

    [RelayCommand]
    private void ToggleSidebar() => IsSidebarExpanded = !IsSidebarExpanded;

    // -------------------------------------------------------------------------
    // Event handlers
    // -------------------------------------------------------------------------

    private void OnNavigated(object? sender, ViewModelBase vm)
        => CurrentViewModel = vm;

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        IsSessionActive      = _sessionState.IsSessionActive;
        SessionCandidateName = _sessionState.CurrentSession?.Candidate.CandidateName
                               ?? string.Empty;
    }

    public virtual void OnNavigatedFrom() { }
}

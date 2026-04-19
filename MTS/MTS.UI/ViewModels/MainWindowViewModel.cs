using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Interfaces.Services;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;
using MTS.UI.ViewModels.Calls;
using MTS.UI.ViewModels.Settings;
using MTS.UI.ViewModels.SupervisorTransfer;

namespace MTS.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _nav;
    private readonly ISessionStateService _sessionState;
    private readonly IDialogService _dialog;

    // -------------------------------------------------------------------------
    // Current displayed view
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    // -------------------------------------------------------------------------
    // Active page tracking — drives nav highlight DataTriggers
    // -------------------------------------------------------------------------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboardActive))]
    [NotifyPropertyChangedFor(nameof(IsBasicsActive))]
    [NotifyPropertyChangedFor(nameof(IsCallsActive))]
    [NotifyPropertyChangedFor(nameof(IsSupTransferActive))]
    [NotifyPropertyChangedFor(nameof(IsReviewActive))]
    [NotifyPropertyChangedFor(nameof(IsHistoryActive))]
    [NotifyPropertyChangedFor(nameof(IsSettingsActive))]
    [NotifyPropertyChangedFor(nameof(IsHelpActive))]
    private string _currentPage = "Dashboard";

    public bool IsDashboardActive   => CurrentPage == "Dashboard";
    public bool IsBasicsActive      => CurrentPage == "Basics";
    public bool IsCallsActive       => CurrentPage == "Calls";
    public bool IsSupTransferActive => CurrentPage == "SupTransfer";
    public bool IsReviewActive      => CurrentPage == "Review";
    public bool IsHistoryActive     => CurrentPage == "History";
    public bool IsSettingsActive    => CurrentPage == "Settings";
    public bool IsHelpActive        => CurrentPage == "Help";

    // -------------------------------------------------------------------------
    // Ticker
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private string _tickerMessage =
        "  \u2022  Welcome to MTS — the Media Training Suite!  " +
        "\u2022  Complete all pre-checks before starting a session.  " +
        "\u2022  Use Ctrl+S to save progress at any time.  " +
        "\u2022  Contact ACD support if you encounter issues.  ";

    // -------------------------------------------------------------------------
    // Session state
    // -------------------------------------------------------------------------

    [ObservableProperty]
    private bool _isSessionActive;

    [ObservableProperty]
    private string _sessionCandidateName = string.Empty;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public MainWindowViewModel(
        INavigationService nav,
        ISessionStateService sessionState,
        IDialogService dialog)
    {
        _nav          = nav;
        _sessionState = sessionState;
        _dialog       = dialog;

        _nav.Navigated               += OnNavigated;
        _sessionState.SessionChanged += OnSessionChanged;

        AppVersion = Assembly.GetExecutingAssembly()
                             .GetName().Version?.ToString(3) ?? "1.0.0";
    }

    public void Initialize()
    {
        _nav.NavigateTo<DashboardViewModel>();
    }

    // -------------------------------------------------------------------------
    // Navigation commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void GoToDashboard() => _nav.NavigateTo<DashboardViewModel>();

    [RelayCommand]
    private void GoToBasics() => _nav.NavigateTo<BasicsViewModel>();

    [RelayCommand]
    private void GoToCalls()
    {
        if (!_sessionState.IsSessionActive) return;
        _nav.NavigateTo<CallsViewModel>();
    }

    [RelayCommand]
    private void GoToSupTransfer()
    {
        if (!_sessionState.IsSessionActive) return;
        _nav.NavigateTo<SupervisorTransferViewModel>();
    }

    [RelayCommand]
    private void GoToReview()
    {
        if (!_sessionState.IsSessionActive) return;
        _nav.NavigateTo<ReviewViewModel>();
    }

    [RelayCommand]
    private void GoToHistory() => _nav.NavigateTo<HistoryViewModel>();

    [RelayCommand]
    private void GoToSettings() => _nav.NavigateTo<SettingsViewModel>();

    [RelayCommand]
    private void GoToHelp() => _nav.NavigateTo<HelpViewModel>();

    // -------------------------------------------------------------------------
    // Bottom-bar commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    private void GoToDiscordPost()
    {
        // Opens the Discord-post helper panel — navigate to a standalone VM
        _nav.NavigateTo<DiscordPostViewModel>();
    }

    [RelayCommand]
    private void OpenSheets()
    {
        const string url = "https://docs.google.com/spreadsheets";
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { }
    }

    [RelayCommand]
    private void OpenCertSpreadsheet()
    {
        const string url = "https://docs.google.com/spreadsheets";
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { }
    }

    [RelayCommand]
    private async Task ExitApp()
    {
        bool confirmed = await _dialog.ShowDangerConfirmAsync(
            "Exit MTS",
            "Are you sure you want to exit?",
            "Exit");
        if (confirmed)
            System.Windows.Application.Current.Shutdown();
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

    // -------------------------------------------------------------------------
    // Event handlers
    // -------------------------------------------------------------------------

    private void OnNavigated(object? sender, ViewModelBase vm)
    {
        CurrentViewModel = vm;

        // Derive the page name from the ViewModel type name
        CurrentPage = vm.GetType().Name
            .Replace("ViewModel", string.Empty, StringComparison.Ordinal);
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        IsSessionActive      = _sessionState.IsSessionActive;
        SessionCandidateName = _sessionState.CurrentSession?.Candidate.CandidateName
                               ?? string.Empty;
    }
}

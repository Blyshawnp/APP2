using MTS.UI.ViewModels.Base;

namespace MTS.UI.Services;

/// <summary>
/// Defines screen navigation for ViewModels.
/// Lives in MTS.UI because it references ViewModelBase (a UI concern).
/// </summary>
public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }

    /// <summary>Fired after CurrentViewModel changes.</summary>
    event EventHandler<ViewModelBase>? Navigated;

    bool CanGoBack { get; }

    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;
    void GoBack();

    /// <summary>
    /// Registers a guard that is called before navigating away from TViewModel.
    /// Returning false prevents navigation.
    /// </summary>
    void RegisterGuard<TViewModel>(Func<bool> canNavigateAway) where TViewModel : ViewModelBase;

    /// <summary>Clears the navigation back-stack.</summary>
    void ClearHistory();
}

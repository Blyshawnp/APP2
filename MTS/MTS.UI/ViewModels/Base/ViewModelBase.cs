using CommunityToolkit.Mvvm.ComponentModel;

namespace MTS.UI.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels.
/// Provides INotifyPropertyChanged via ObservableObject,
/// common busy/error state, and navigation lifecycle hooks.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _busyMessage = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Called by NavigationService after the ViewModel is resolved from DI and before the View renders.
    /// Override to load data. Default implementation is synchronous no-op.
    /// </summary>
    public virtual void OnNavigatedTo(object? parameter) { }

    /// <summary>
    /// Async variant — called immediately after OnNavigatedTo.
    /// Override for data loading that requires awaiting.
    /// </summary>
    public virtual Task OnNavigatedToAsync(object? parameter) => Task.CompletedTask;

    /// <summary>
    /// Called when navigating away from this ViewModel (before the next ViewModel loads).
    /// Use for cleanup or persisting transient state.
    /// </summary>
    public virtual void OnNavigatedFrom() { }

    /// <summary>
    /// Called when navigating away from this ViewModel.
    /// Return false to cancel the navigation (e.g., unsaved changes).
    /// </summary>
    public virtual bool CanNavigateAway() => true;

    /// <summary>
    /// Clears the error message banner.
    /// </summary>
    protected void ClearError() => ErrorMessage = string.Empty;

    /// <summary>
    /// Runs an async action with IsBusy tracking and error capture.
    /// </summary>
    protected async Task ExecuteBusyAsync(Func<Task> action, string busyMessage = "Loading...")
    {
        if (IsBusy) return;
        try
        {
            IsBusy      = true;
            BusyMessage = busyMessage;
            ClearError();
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy      = false;
            BusyMessage = string.Empty;
        }
    }
}

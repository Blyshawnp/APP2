using MTS.UI.ViewModels.Base;

namespace MTS.UI.Services;

/// <summary>
/// Resolves ViewModels from DI, maintains a navigation back-stack,
/// enforces navigation guards, and fires the Navigated event so that
/// MainWindowViewModel can update its CurrentViewModel binding.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly Stack<ViewModelBase> _backStack = new();
    private readonly Dictionary<Type, Func<bool>> _guards  = new();

    private ViewModelBase? _current;

    public ViewModelBase? CurrentViewModel => _current;

    public event EventHandler<ViewModelBase>? Navigated;

    public bool CanGoBack => _backStack.Count > 0;

    public NavigationService(IServiceProvider services)
        => _services = services;

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        => NavigateCore<TViewModel>(null);

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase
        => NavigateCore<TViewModel>(parameter);

    public void GoBack()
    {
        if (!CanGoBack) return;
        if (!CheckGuard(_current)) return;

        _current?.OnNavigatedFrom();
        _current = _backStack.Pop();
        Navigated?.Invoke(this, _current);
    }

    public void RegisterGuard<TViewModel>(Func<bool> canNavigateAway) where TViewModel : ViewModelBase
        => _guards[typeof(TViewModel)] = canNavigateAway;

    public void ClearHistory() => _backStack.Clear();

    // -------------------------------------------------------------------------

    private void NavigateCore<TViewModel>(object? parameter) where TViewModel : ViewModelBase
    {
        if (!CheckGuard(_current)) return;

        if (_current != null)
        {
            _current.OnNavigatedFrom();
            _backStack.Push(_current);
        }

        var vm = _services.GetRequiredService<TViewModel>();

        _current = vm;
        vm.OnNavigatedTo(parameter);

        // Fire-and-forget async load on the UI dispatcher so that
        // any ObservableCollection or property updates stay on the UI thread.
        _ = System.Windows.Application.Current?.Dispatcher.InvokeAsync(async () =>
        {
            try { await vm.OnNavigatedToAsync(parameter); }
            catch (Exception ex) { vm.ErrorMessage = ex.Message; }
        });

        Navigated?.Invoke(this, vm);
    }

    private bool CheckGuard(ViewModelBase? vm)
    {
        if (vm == null) return true;
        if (_guards.TryGetValue(vm.GetType(), out var guard))
            return guard();
        return vm.CanNavigateAway();
    }
}

// Extension so service resolution doesn't require a cast everywhere
file static class ServiceProviderExtensions
{
    internal static T GetRequiredService<T>(this IServiceProvider sp)
        where T : notnull
        => (T)sp.GetService(typeof(T))!;
}

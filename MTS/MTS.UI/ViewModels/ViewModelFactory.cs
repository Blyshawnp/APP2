using MTS.UI.ViewModels.Base;

namespace MTS.UI.ViewModels;

/// <summary>
/// Thin wrapper around IServiceProvider so that NavigationService
/// can resolve ViewModels without taking a direct IServiceProvider dependency.
/// </summary>
public class ViewModelFactory
{
    private readonly IServiceProvider _services;

    public ViewModelFactory(IServiceProvider services)
        => _services = services;

    public TViewModel Create<TViewModel>() where TViewModel : ViewModelBase
        => (TViewModel)_services.GetService(typeof(TViewModel))!;
}

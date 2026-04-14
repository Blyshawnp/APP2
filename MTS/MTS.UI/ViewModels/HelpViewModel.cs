using CommunityToolkit.Mvvm.Input;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;

namespace MTS.UI.ViewModels;

public partial class HelpViewModel : ViewModelBase
{
    private readonly INavigationService _nav;

    public HelpViewModel(INavigationService nav)
    {
        _nav = nav;
    }

    [RelayCommand]
    private void Back() => _nav.NavigateTo<DashboardViewModel>();
}

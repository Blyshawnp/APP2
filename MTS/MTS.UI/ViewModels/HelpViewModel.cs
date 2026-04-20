using System.Diagnostics;
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

    [RelayCommand]
    private void ReplayTutorial()
    {
        // Placeholder — tutorial replay not yet implemented
    }

    [RelayCommand]
    private void SendEmail()
    {
        try { Process.Start(new ProcessStartInfo("mailto:blyshawnp@gmail.com") { UseShellExecute = true }); }
        catch { }
    }

    [RelayCommand]
    private void OpenDiscord()
    {
        try { Process.Start(new ProcessStartInfo("https://discord.com/users/shawnbly") { UseShellExecute = true }); }
        catch { }
    }
}

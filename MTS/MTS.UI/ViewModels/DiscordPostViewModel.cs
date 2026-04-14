using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;

namespace MTS.UI.ViewModels;

/// <summary>
/// Standalone Discord Post helper panel.
/// Allows the user to compose and copy a Discord post without an active session.
/// </summary>
public partial class DiscordPostViewModel : ViewModelBase
{
    private readonly INavigationService _nav;
    private readonly IClipboardService _clipboard;

    [ObservableProperty]
    private string _postText =
        "⭐ STAR transfer scheduled.\n" +
        "Please connect to the ACD supervisor line.\n" +
        "Thank you!";

    public DiscordPostViewModel(INavigationService nav, IClipboardService clipboard)
    {
        _nav       = nav;
        _clipboard = clipboard;
    }

    [RelayCommand]
    private void CopyPost()
    {
        _clipboard.SetText(PostText);
    }

    [RelayCommand]
    private void Back() => _nav.NavigateTo<DashboardViewModel>();
}

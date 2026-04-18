using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Settings;

public partial class DiscordScreenshotRowViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    public string DisplayLabel => string.IsNullOrWhiteSpace(Title) ? "(new screenshot)" : Title;

    partial void OnTitleChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    public static DiscordScreenshotRowViewModel FromDomain(DiscordScreenshot s) => new()
    {
        Id       = s.Id,
        Title    = s.Title,
        ImageUrl = s.ImageUrl
    };

    public DiscordScreenshot ToDomain() => new()
    {
        Id       = Id,
        Title    = Title,
        ImageUrl = ImageUrl
    };
}

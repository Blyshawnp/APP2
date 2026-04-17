using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Settings;

/// <summary>
/// Observable wrapper for a DiscordTemplate (Trigger + Message).
/// </summary>
public partial class DiscordTemplateRowViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _trigger = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    public string DisplayLabel => string.IsNullOrWhiteSpace(Trigger) ? "(new template)" : Trigger;

    partial void OnTriggerChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    public static DiscordTemplateRowViewModel FromDomain(DiscordTemplate t) => new()
    {
        Id      = t.Id,
        Trigger = t.Trigger,
        Message = t.Message
    };

    public DiscordTemplate ToDomain() => new()
    {
        Id      = Id,
        Trigger = Trigger,
        Message = Message
    };
}

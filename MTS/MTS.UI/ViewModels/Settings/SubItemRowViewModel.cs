using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Settings;

/// <summary>
/// Observable wrapper for a CoachingSubItem.
/// Used inside CoachingCategoryRowViewModel's SubItems collection.
/// </summary>
public partial class SubItemRowViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _label = string.Empty;

    public string DisplayLabel => string.IsNullOrWhiteSpace(Label) ? "(new sub-item)" : Label;

    partial void OnLabelChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    // ---- Factory ----

    public static SubItemRowViewModel FromDomain(CoachingSubItem s) => new()
    {
        Id    = s.Id,
        Label = s.Label
    };

    public CoachingSubItem ToDomain() => new()
    {
        Id    = Id,
        Label = Label
    };
}

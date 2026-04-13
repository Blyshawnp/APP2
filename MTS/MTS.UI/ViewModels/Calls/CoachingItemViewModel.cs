using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Calls;

/// <summary>
/// Wraps one CoachingCategory from settings as a live, bindable form row.
/// Tracks selection, sub-item selection, and "Other" notes.
/// Business rule BR-04: when IsOther is true and IsSelected is true,
/// Notes becomes required — enforced in ValidationService, surfaced here.
/// </summary>
public partial class CoachingItemViewModel : ObservableObject
{
    public CoachingCategory Category { get; }

    public Guid CategoryId  => Category.Id;
    public string Label     => Category.Label;
    public bool IsOther     => Category.IsOther;
    public bool HasSubItems => Category.SubItems.Count > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOtherInput))]
    private bool _isSelected;

    [ObservableProperty]
    private string _notes = string.Empty;

    /// <summary>True when this item is checked AND it carries the "Other" flag.</summary>
    public bool ShowOtherInput => IsSelected && IsOther;

    public ObservableCollection<SubItemViewModel> SubItems { get; }

    public CoachingItemViewModel(CoachingCategory category)
    {
        Category = category;
        SubItems = new ObservableCollection<SubItemViewModel>(
            category.SubItems.Select(s => new SubItemViewModel(s)));
    }

    /// <summary>Builds the domain CoachingSelection snapshot for persistence.</summary>
    public Core.Models.Session.CoachingSelection ToSelection() => new()
    {
        CategoryId      = Category.Id,
        CategoryLabel   = Category.Label,
        IsOther         = Category.IsOther,
        SelectedSubItems = SubItems
            .Where(s => s.IsSelected)
            .Select(s => s.Label)
            .ToList(),
        Notes = Notes
    };
}

public partial class SubItemViewModel : ObservableObject
{
    public CoachingSubItem SubItem { get; }
    public string Label => SubItem.Label;

    [ObservableProperty]
    private bool _isSelected;

    public SubItemViewModel(CoachingSubItem subItem)
        => SubItem = subItem;
}

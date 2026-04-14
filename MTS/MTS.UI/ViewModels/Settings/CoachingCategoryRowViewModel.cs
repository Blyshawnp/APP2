using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Settings;

/// <summary>
/// Observable wrapper for a CoachingCategory.
/// Owns its SubItems collection and the commands to add/remove/reorder sub-items
/// so the detail editor in SettingsView can bind directly without going through
/// SettingsViewModel for sub-item CRUD.
/// </summary>
public partial class CoachingCategoryRowViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private bool _isOther;

    [ObservableProperty]
    private bool _appliesToCalls = true;

    [ObservableProperty]
    private bool _appliesToSupTransfers;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private SubItemRowViewModel? _selectedSubItem;

    public ObservableCollection<SubItemRowViewModel> SubItems { get; } = new();

    public string DisplayLabel => string.IsNullOrWhiteSpace(Label) ? "(new category)" : Label;

    partial void OnLabelChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    // ---- Sub-item commands (owned here, not on SettingsViewModel) ----

    [RelayCommand]
    private void AddSubItem()
    {
        var vm = new SubItemRowViewModel();
        SubItems.Add(vm);
        SelectedSubItem = vm;
    }

    [RelayCommand]
    private void RemoveSubItem(SubItemRowViewModel? item)
    {
        if (item is null) return;
        SubItems.Remove(item);
        SelectedSubItem = SubItems.Count > 0 ? SubItems[^1] : null;
    }

    [RelayCommand]
    private void MoveSubItemUp(SubItemRowViewModel? item)
    {
        if (item is null) return;
        var idx = SubItems.IndexOf(item);
        if (idx > 0) SubItems.Move(idx, idx - 1);
    }

    [RelayCommand]
    private void MoveSubItemDown(SubItemRowViewModel? item)
    {
        if (item is null) return;
        var idx = SubItems.IndexOf(item);
        if (idx >= 0 && idx < SubItems.Count - 1) SubItems.Move(idx, idx + 1);
    }

    // ---- Factory ----

    public static CoachingCategoryRowViewModel FromDomain(CoachingCategory cc)
    {
        var vm = new CoachingCategoryRowViewModel
        {
            Id                    = cc.Id,
            Label                 = cc.Label,
            IsOther               = cc.IsOther,
            AppliesToCalls        = cc.AppliesToCalls,
            AppliesToSupTransfers = cc.AppliesToSupTransfers,
            IsEnabled             = cc.IsEnabled
        };
        foreach (var sub in cc.SubItems)
            vm.SubItems.Add(SubItemRowViewModel.FromDomain(sub));
        return vm;
    }

    public CoachingCategory ToDomain() => new()
    {
        Id                    = Id,
        Label                 = Label,
        IsOther               = IsOther,
        AppliesToCalls        = AppliesToCalls,
        AppliesToSupTransfers = AppliesToSupTransfers,
        IsEnabled             = IsEnabled,
        SubItems              = SubItems.Select(s => s.ToDomain()).ToList()
    };
}

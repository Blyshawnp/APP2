using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Settings;
using MTS.UI.Services;
using MTS.UI.ViewModels;
using MTS.UI.ViewModels.Base;

namespace MTS.UI.ViewModels.Settings;

/// <summary>
/// Drives the Settings screen.
///
/// Responsibilities:
///   – Loads all editable lookup tables from ISettingsService on navigate.
///   – Exposes per-section ObservableCollections + SelectedXxx properties.
///   – Provides Add / Remove / MoveUp / MoveDown commands for every section.
///   – On Save: maps row VMs back to domain models, writes via ISettingsService.
///   – On ResetToDefaults: reloads from embedded seed defaults (with confirmation).
///   – IsDirty tracks structural changes (add/remove/reorder); individual field edits
///     are always persisted on the next explicit Save.
///
/// NOT in this ViewModel:
///   – Discord messages (system-owned, never editable)
///   – Help or Tutorial content
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService    _settingsService;
    private readonly IDialogService      _dialog;
    private readonly INotificationService _notification;
    private readonly INavigationService  _nav;

    // ================================================================
    // Shows
    // ================================================================
    [ObservableProperty] private ObservableCollection<ShowRowViewModel> _shows = new();
    [ObservableProperty] private ShowRowViewModel? _selectedShow;

    // ================================================================
    // Call Types
    // ================================================================
    [ObservableProperty] private ObservableCollection<CallTypeRowViewModel> _callTypes = new();
    [ObservableProperty] private CallTypeRowViewModel? _selectedCallType;

    // ================================================================
    // Donors — three distinct categories preserved from Python prototype
    // ================================================================
    [ObservableProperty] private ObservableCollection<DonorRowViewModel> _newDonors = new();
    [ObservableProperty] private DonorRowViewModel? _selectedNewDonor;

    [ObservableProperty] private ObservableCollection<DonorRowViewModel> _existingMembers = new();
    [ObservableProperty] private DonorRowViewModel? _selectedExistingMember;

    [ObservableProperty] private ObservableCollection<DonorRowViewModel> _increaseSustaining = new();
    [ObservableProperty] private DonorRowViewModel? _selectedIncreaseSustaining;

    // ================================================================
    // Supervisor Reasons
    // ================================================================
    [ObservableProperty] private ObservableCollection<SimpleRowViewModel> _supervisorReasons = new();
    [ObservableProperty] private SimpleRowViewModel? _selectedSupervisorReason;

    // ================================================================
    // Fail Reasons
    // ================================================================
    [ObservableProperty] private ObservableCollection<FailReasonRowViewModel> _failReasons = new();
    [ObservableProperty] private FailReasonRowViewModel? _selectedFailReason;

    // ================================================================
    // Coaching Categories
    // ================================================================
    [ObservableProperty] private ObservableCollection<CoachingCategoryRowViewModel> _coachingCategories = new();
    [ObservableProperty] private CoachingCategoryRowViewModel? _selectedCoachingCategory;

    // ================================================================
    // Status
    // ================================================================
    [ObservableProperty] private bool _isDirty;

    // ================================================================
    public SettingsViewModel(
        ISettingsService     settingsService,
        IDialogService       dialog,
        INotificationService notification,
        INavigationService   nav)
    {
        _settingsService = settingsService;
        _dialog          = dialog;
        _notification    = notification;
        _nav             = nav;
    }

    // ================================================================
    // Lifecycle
    // ================================================================

    public override async Task OnNavigatedToAsync(object? parameter)
    {
        await ExecuteBusyAsync(async () =>
        {
            var settings = await _settingsService.LoadAsync();
            LoadFromSettings(settings);
            IsDirty = false;
        }, "Loading settings...");
    }

    // ================================================================
    // Load / Apply helpers
    // ================================================================

    private void LoadFromSettings(AppSettings settings)
    {
        Shows.Clear();
        foreach (var s in settings.Shows)
            Shows.Add(ShowRowViewModel.FromDomain(s));

        CallTypes.Clear();
        foreach (var ct in settings.CallTypes)
            CallTypes.Add(CallTypeRowViewModel.FromDomain(ct));

        NewDonors.Clear();
        foreach (var d in settings.Donors.NewDonors)
            NewDonors.Add(DonorRowViewModel.FromDomain(d));

        ExistingMembers.Clear();
        foreach (var d in settings.Donors.ExistingMembers)
            ExistingMembers.Add(DonorRowViewModel.FromDomain(d));

        IncreaseSustaining.Clear();
        foreach (var d in settings.Donors.IncreaseSustaining)
            IncreaseSustaining.Add(DonorRowViewModel.FromDomain(d));

        SupervisorReasons.Clear();
        foreach (var r in settings.SupervisorReasons)
            SupervisorReasons.Add(SimpleRowViewModel.FromSupervisorReason(r));

        FailReasons.Clear();
        foreach (var fr in settings.FailReasons)
            FailReasons.Add(FailReasonRowViewModel.FromDomain(fr));

        CoachingCategories.Clear();
        foreach (var cc in settings.CoachingCategories)
            CoachingCategories.Add(CoachingCategoryRowViewModel.FromDomain(cc));

        // Clear selections
        SelectedShow                = null;
        SelectedCallType            = null;
        SelectedNewDonor            = null;
        SelectedExistingMember      = null;
        SelectedIncreaseSustaining  = null;
        SelectedSupervisorReason    = null;
        SelectedFailReason          = null;
        SelectedCoachingCategory    = null;
    }

    private void ApplyToSettings(AppSettings settings)
    {
        settings.Shows                    = Shows.Select(vm => vm.ToDomain()).ToList();
        settings.CallTypes                = CallTypes.Select(vm => vm.ToDomain()).ToList();
        settings.Donors.NewDonors         = NewDonors.Select(vm => vm.ToDomain()).ToList();
        settings.Donors.ExistingMembers   = ExistingMembers.Select(vm => vm.ToDomain()).ToList();
        settings.Donors.IncreaseSustaining= IncreaseSustaining.Select(vm => vm.ToDomain()).ToList();
        settings.SupervisorReasons        = SupervisorReasons.Select(vm => vm.ToSupervisorReason()).ToList();
        settings.FailReasons              = FailReasons.Select(vm => vm.ToDomain()).ToList();
        settings.CoachingCategories       = CoachingCategories.Select(vm => vm.ToDomain()).ToList();
    }

    // ================================================================
    // Global commands
    // ================================================================

    [RelayCommand]
    private async Task Save()
    {
        await ExecuteBusyAsync(async () =>
        {
            // Load fresh copy to preserve non-lookup settings (TesterProfile, URLs, etc.)
            var settings = await _settingsService.LoadAsync();
            ApplyToSettings(settings);
            await _settingsService.SaveAsync(settings);
            IsDirty = false;
            _notification.ShowSuccess("Settings saved.");
        }, "Saving settings...");
    }

    [RelayCommand]
    private async Task ResetToDefaults()
    {
        bool confirmed = await _dialog.ShowConfirmAsync(
            "Reset to Defaults",
            "Reset all lookup tables to their default values? Your current customizations will be lost.",
            "Reset");

        if (!confirmed) return;

        await ExecuteBusyAsync(async () =>
        {
            var defaults = await _settingsService.GetDefaultsAsync();
            LoadFromSettings(defaults);
            IsDirty = true;
            _notification.ShowSuccess("Lookup tables reset to defaults. Click Save to apply.");
        }, "Resetting...");
    }

    [RelayCommand]
    private async Task NavigateBack()
    {
        if (IsDirty)
        {
            bool confirmed = await _dialog.ShowConfirmAsync(
                "Unsaved Changes",
                "You have unsaved changes. Leave without saving?",
                "Leave without saving");
            if (!confirmed) return;
        }

        if (_nav.CanGoBack)
            _nav.GoBack();
        else
            _nav.NavigateTo<DashboardViewModel>();
    }

    // ================================================================
    // Shows commands
    // ================================================================

    [RelayCommand]
    private void AddShow()
    {
        var vm = new ShowRowViewModel();
        Shows.Add(vm);
        SelectedShow = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveShow(ShowRowViewModel? item)
    {
        if (item is null) return;
        Shows.Remove(item);
        SelectedShow = Shows.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveShowUp(ShowRowViewModel? item) { MoveUp(Shows, item); IsDirty = true; }

    [RelayCommand]
    private void MoveShowDown(ShowRowViewModel? item) { MoveDown(Shows, item); IsDirty = true; }

    // ================================================================
    // Call Types commands
    // ================================================================

    [RelayCommand]
    private void AddCallType()
    {
        var vm = new CallTypeRowViewModel();
        CallTypes.Add(vm);
        SelectedCallType = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveCallType(CallTypeRowViewModel? item)
    {
        if (item is null) return;
        CallTypes.Remove(item);
        SelectedCallType = CallTypes.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveCallTypeUp(CallTypeRowViewModel? item) { MoveUp(CallTypes, item); IsDirty = true; }

    [RelayCommand]
    private void MoveCallTypeDown(CallTypeRowViewModel? item) { MoveDown(CallTypes, item); IsDirty = true; }

    // ================================================================
    // New Donors commands
    // ================================================================

    [RelayCommand]
    private void AddNewDonor()
    {
        var vm = new DonorRowViewModel();
        NewDonors.Add(vm);
        SelectedNewDonor = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveNewDonor(DonorRowViewModel? item)
    {
        if (item is null) return;
        NewDonors.Remove(item);
        SelectedNewDonor = NewDonors.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveNewDonorUp(DonorRowViewModel? item) { MoveUp(NewDonors, item); IsDirty = true; }

    [RelayCommand]
    private void MoveNewDonorDown(DonorRowViewModel? item) { MoveDown(NewDonors, item); IsDirty = true; }

    // ================================================================
    // Existing Members commands
    // ================================================================

    [RelayCommand]
    private void AddExistingMember()
    {
        var vm = new DonorRowViewModel();
        ExistingMembers.Add(vm);
        SelectedExistingMember = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveExistingMember(DonorRowViewModel? item)
    {
        if (item is null) return;
        ExistingMembers.Remove(item);
        SelectedExistingMember = ExistingMembers.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveExistingMemberUp(DonorRowViewModel? item) { MoveUp(ExistingMembers, item); IsDirty = true; }

    [RelayCommand]
    private void MoveExistingMemberDown(DonorRowViewModel? item) { MoveDown(ExistingMembers, item); IsDirty = true; }

    // ================================================================
    // Increase Sustaining commands
    // ================================================================

    [RelayCommand]
    private void AddIncreaseSustaining()
    {
        var vm = new DonorRowViewModel();
        IncreaseSustaining.Add(vm);
        SelectedIncreaseSustaining = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveIncreaseSustaining(DonorRowViewModel? item)
    {
        if (item is null) return;
        IncreaseSustaining.Remove(item);
        SelectedIncreaseSustaining = IncreaseSustaining.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveIncreaseSustainingUp(DonorRowViewModel? item) { MoveUp(IncreaseSustaining, item); IsDirty = true; }

    [RelayCommand]
    private void MoveIncreaseSustainingDown(DonorRowViewModel? item) { MoveDown(IncreaseSustaining, item); IsDirty = true; }

    // ================================================================
    // Supervisor Reasons commands
    // ================================================================

    [RelayCommand]
    private void AddSupervisorReason()
    {
        var vm = new SimpleRowViewModel();
        SupervisorReasons.Add(vm);
        SelectedSupervisorReason = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveSupervisorReason(SimpleRowViewModel? item)
    {
        if (item is null) return;
        SupervisorReasons.Remove(item);
        SelectedSupervisorReason = SupervisorReasons.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveSupervisorReasonUp(SimpleRowViewModel? item) { MoveUp(SupervisorReasons, item); IsDirty = true; }

    [RelayCommand]
    private void MoveSupervisorReasonDown(SimpleRowViewModel? item) { MoveDown(SupervisorReasons, item); IsDirty = true; }

    // ================================================================
    // Fail Reasons commands
    // ================================================================

    [RelayCommand]
    private void AddFailReason()
    {
        var vm = new FailReasonRowViewModel();
        FailReasons.Add(vm);
        SelectedFailReason = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveFailReason(FailReasonRowViewModel? item)
    {
        if (item is null) return;
        FailReasons.Remove(item);
        SelectedFailReason = FailReasons.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveFailReasonUp(FailReasonRowViewModel? item) { MoveUp(FailReasons, item); IsDirty = true; }

    [RelayCommand]
    private void MoveFailReasonDown(FailReasonRowViewModel? item) { MoveDown(FailReasons, item); IsDirty = true; }

    // ================================================================
    // Coaching Categories commands
    // ================================================================

    [RelayCommand]
    private void AddCoachingCategory()
    {
        var vm = new CoachingCategoryRowViewModel();
        CoachingCategories.Add(vm);
        SelectedCoachingCategory = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveCoachingCategory(CoachingCategoryRowViewModel? item)
    {
        if (item is null) return;
        CoachingCategories.Remove(item);
        SelectedCoachingCategory = CoachingCategories.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveCoachingCategoryUp(CoachingCategoryRowViewModel? item) { MoveUp(CoachingCategories, item); IsDirty = true; }

    [RelayCommand]
    private void MoveCoachingCategoryDown(CoachingCategoryRowViewModel? item) { MoveDown(CoachingCategories, item); IsDirty = true; }

    // ================================================================
    // Shared helpers
    // ================================================================

    private static void MoveUp<T>(ObservableCollection<T> collection, T? item) where T : class
    {
        if (item is null) return;
        var idx = collection.IndexOf(item);
        if (idx > 0) collection.Move(idx, idx - 1);
    }

    private static void MoveDown<T>(ObservableCollection<T> collection, T? item) where T : class
    {
        if (item is null) return;
        var idx = collection.IndexOf(item);
        if (idx >= 0 && idx < collection.Count - 1) collection.Move(idx, idx + 1);
    }
}

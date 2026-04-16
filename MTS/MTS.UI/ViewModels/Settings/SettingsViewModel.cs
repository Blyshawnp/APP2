using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Settings;
using MTS.UI.Services;
using MTS.UI.ViewModels;
using MTS.UI.ViewModels.Base;

namespace MTS.UI.ViewModels.Settings;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService    _settingsService;
    private readonly IDialogService      _dialog;
    private readonly INotificationService _notification;
    private readonly INavigationService  _nav;

    // ================================================================
    // General — Profile
    // ================================================================
    [ObservableProperty] private string _testerName = string.Empty;
    [ObservableProperty] private string _displayName = string.Empty;

    // ================================================================
    // General — URLs
    // ================================================================
    [ObservableProperty] private string _formUrl = string.Empty;
    [ObservableProperty] private string _certSheetUrl = string.Empty;

    // ================================================================
    // Gemini AI
    // ================================================================
    [ObservableProperty] private bool   _geminiEnabled;
    [ObservableProperty] private string _geminiApiKey = string.Empty;
    [ObservableProperty] private string _geminiCoachingPrompt = string.Empty;
    [ObservableProperty] private string _geminiFailPrompt = string.Empty;

    // ================================================================
    // Google Sheets
    // ================================================================
    [ObservableProperty] private bool   _sheetsEnabled;
    [ObservableProperty] private string _sheetsSheetId = string.Empty;
    [ObservableProperty] private string _sheetsWorksheet = string.Empty;
    [ObservableProperty] private string _sheetsServiceAccountPath = string.Empty;

    // ================================================================
    // Calendar
    // ================================================================
    [ObservableProperty] private bool _calendarEnabled;

    // ================================================================
    // Payment — Credit Card
    // ================================================================
    [ObservableProperty] private string _paymentCcType = string.Empty;
    [ObservableProperty] private string _paymentCcNumber = string.Empty;
    [ObservableProperty] private string _paymentCcExpiration = string.Empty;
    [ObservableProperty] private string _paymentCcCvv = string.Empty;

    // ================================================================
    // Payment — EFT / Bank
    // ================================================================
    [ObservableProperty] private string _paymentEftRouting = string.Empty;
    [ObservableProperty] private string _paymentEftAccount = string.Empty;

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
    // Discord — Templates
    // ================================================================
    [ObservableProperty] private ObservableCollection<DiscordTemplateRowViewModel> _discordTemplates = new();
    [ObservableProperty] private DiscordTemplateRowViewModel? _selectedDiscordTemplate;

    // ================================================================
    // Discord — Screenshots
    // ================================================================
    [ObservableProperty] private ObservableCollection<DiscordScreenshotRowViewModel> _discordScreenshots = new();
    [ObservableProperty] private DiscordScreenshotRowViewModel? _selectedDiscordScreenshot;

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
        // General
        TesterName   = settings.TesterProfile.TesterName;
        DisplayName  = settings.TesterProfile.DisplayName;
        FormUrl      = settings.Urls.FormUrl;
        CertSheetUrl = settings.Urls.CertSheetUrl;

        // Gemini AI
        GeminiEnabled        = settings.Gemini.Enabled;
        GeminiApiKey         = settings.Gemini.ApiKey;
        GeminiCoachingPrompt = settings.Gemini.CoachingPrompt;
        GeminiFailPrompt     = settings.Gemini.FailPrompt;

        // Google Sheets
        SheetsEnabled            = settings.Sheets.Enabled;
        SheetsSheetId            = settings.Sheets.SheetId;
        SheetsWorksheet          = settings.Sheets.Worksheet;
        SheetsServiceAccountPath = settings.Sheets.ServiceAccountPath;

        // Calendar
        CalendarEnabled = settings.Calendar.Enabled;

        // Payment
        PaymentCcType       = settings.Payment.CcType;
        PaymentCcNumber     = settings.Payment.CcNumber;
        PaymentCcExpiration = settings.Payment.CcExpiration;
        PaymentCcCvv        = settings.Payment.CcCvv;
        PaymentEftRouting   = settings.Payment.EftRouting;
        PaymentEftAccount   = settings.Payment.EftAccount;

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

        DiscordTemplates.Clear();
        foreach (var t in settings.Discord.Templates)
            DiscordTemplates.Add(DiscordTemplateRowViewModel.FromDomain(t));

        DiscordScreenshots.Clear();
        foreach (var s in settings.Discord.Screenshots)
            DiscordScreenshots.Add(DiscordScreenshotRowViewModel.FromDomain(s));

        // Clear selections
        SelectedShow                = null;
        SelectedCallType            = null;
        SelectedNewDonor            = null;
        SelectedExistingMember      = null;
        SelectedIncreaseSustaining  = null;
        SelectedSupervisorReason    = null;
        SelectedFailReason          = null;
        SelectedCoachingCategory    = null;
        SelectedDiscordTemplate     = null;
        SelectedDiscordScreenshot   = null;
    }

    private void ApplyToSettings(AppSettings settings)
    {
        // General
        settings.TesterProfile.TesterName  = TesterName.Trim();
        settings.TesterProfile.DisplayName = DisplayName.Trim();
        settings.Urls.FormUrl              = FormUrl.Trim();
        settings.Urls.CertSheetUrl         = CertSheetUrl.Trim();

        // Gemini AI
        settings.Gemini.Enabled        = GeminiEnabled;
        settings.Gemini.ApiKey         = GeminiApiKey.Trim();
        settings.Gemini.CoachingPrompt = GeminiCoachingPrompt;
        settings.Gemini.FailPrompt     = GeminiFailPrompt;

        // Google Sheets
        settings.Sheets.Enabled            = SheetsEnabled;
        settings.Sheets.SheetId            = SheetsSheetId.Trim();
        settings.Sheets.Worksheet          = SheetsWorksheet.Trim();
        settings.Sheets.ServiceAccountPath = SheetsServiceAccountPath.Trim();

        // Calendar
        settings.Calendar.Enabled = CalendarEnabled;

        // Payment
        settings.Payment.CcType       = PaymentCcType.Trim();
        settings.Payment.CcNumber     = PaymentCcNumber.Trim();
        settings.Payment.CcExpiration = PaymentCcExpiration.Trim();
        settings.Payment.CcCvv        = PaymentCcCvv.Trim();
        settings.Payment.EftRouting   = PaymentEftRouting.Trim();
        settings.Payment.EftAccount   = PaymentEftAccount.Trim();

        // Discord
        settings.Discord.Templates   = DiscordTemplates.Select(vm => vm.ToDomain()).ToList();
        settings.Discord.Screenshots = DiscordScreenshots.Select(vm => vm.ToDomain()).ToList();

        // Lookup tables
        settings.Shows                     = Shows.Select(vm => vm.ToDomain()).ToList();
        settings.CallTypes                 = CallTypes.Select(vm => vm.ToDomain()).ToList();
        settings.Donors.NewDonors          = NewDonors.Select(vm => vm.ToDomain()).ToList();
        settings.Donors.ExistingMembers    = ExistingMembers.Select(vm => vm.ToDomain()).ToList();
        settings.Donors.IncreaseSustaining = IncreaseSustaining.Select(vm => vm.ToDomain()).ToList();
        settings.SupervisorReasons         = SupervisorReasons.Select(vm => vm.ToSupervisorReason()).ToList();
        settings.FailReasons               = FailReasons.Select(vm => vm.ToDomain()).ToList();
        settings.CoachingCategories        = CoachingCategories.Select(vm => vm.ToDomain()).ToList();
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
    private async Task ToggleTheme()
    {
        await ExecuteBusyAsync(async () =>
        {
            var settings = await _settingsService.LoadAsync();
            settings.UiPreferences.Theme = settings.UiPreferences.Theme == MTS.Core.Enums.AppTheme.Dark
                ? MTS.Core.Enums.AppTheme.Light
                : MTS.Core.Enums.AppTheme.Dark;
            await _settingsService.SaveAsync(settings);
            _notification.ShowSuccess($"Theme set to {settings.UiPreferences.Theme}. Restart to apply.");
        });
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
    // Discord Templates commands
    // ================================================================

    [RelayCommand]
    private void AddDiscordTemplate()
    {
        var vm = new DiscordTemplateRowViewModel();
        DiscordTemplates.Add(vm);
        SelectedDiscordTemplate = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveDiscordTemplate(DiscordTemplateRowViewModel? item)
    {
        if (item is null) return;
        DiscordTemplates.Remove(item);
        SelectedDiscordTemplate = DiscordTemplates.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveDiscordTemplateUp(DiscordTemplateRowViewModel? item) { MoveUp(DiscordTemplates, item); IsDirty = true; }

    [RelayCommand]
    private void MoveDiscordTemplateDown(DiscordTemplateRowViewModel? item) { MoveDown(DiscordTemplates, item); IsDirty = true; }

    // ================================================================
    // Discord Screenshots commands
    // ================================================================

    [RelayCommand]
    private void AddDiscordScreenshot()
    {
        var vm = new DiscordScreenshotRowViewModel();
        DiscordScreenshots.Add(vm);
        SelectedDiscordScreenshot = vm;
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveDiscordScreenshot(DiscordScreenshotRowViewModel? item)
    {
        if (item is null) return;
        DiscordScreenshots.Remove(item);
        SelectedDiscordScreenshot = DiscordScreenshots.FirstOrDefault();
        IsDirty = true;
    }

    [RelayCommand]
    private void MoveDiscordScreenshotUp(DiscordScreenshotRowViewModel? item) { MoveUp(DiscordScreenshots, item); IsDirty = true; }

    [RelayCommand]
    private void MoveDiscordScreenshotDown(DiscordScreenshotRowViewModel? item) { MoveDown(DiscordScreenshots, item); IsDirty = true; }

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

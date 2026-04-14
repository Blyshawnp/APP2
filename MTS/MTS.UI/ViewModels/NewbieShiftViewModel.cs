using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTS.Core.Enums;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Session;
using MTS.UI.Services;
using MTS.UI.ViewModels.Base;

namespace MTS.UI.ViewModels;

public partial class NewbieShiftViewModel : ViewModelBase
{
    private readonly ISessionService _sessionService;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialog;

    // -------------------------------------------------------------------------
    // Shift details
    // -------------------------------------------------------------------------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CalendarEventTitle))]
    private DateTime _shiftDate = DateTime.Today.AddDays(7);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CalendarEventTitle))]
    private int _shiftHour = 10;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CalendarEventTitle))]
    private int _shiftMinute = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CalendarEventTitle))]
    private bool _isAm = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CalendarEventTitle))]
    private AppTimezone _timezone = AppTimezone.Eastern;

    public string CalendarEventTitle =>
        $"Newbie Shift — {ShiftDate:MMM d, yyyy} {ShiftHour:D2}:{ShiftMinute:D2} {(IsAm ? "AM" : "PM")} {Timezone}";

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public NewbieShiftViewModel(
        ISessionService sessionService,
        INavigationService nav,
        IDialogService dialog)
    {
        _sessionService = sessionService;
        _nav    = nav;
        _dialog = dialog;
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    [RelayCommand]
    private async Task SaveAndContinue()
    {
        await ExecuteBusyAsync(async () =>
        {
            var record = new NewbieShiftRecord
            {
                Date     = DateOnly.FromDateTime(ShiftDate),
                Time     = new TimeOnly(IsAm ? ShiftHour % 12 : (ShiftHour % 12) + 12, ShiftMinute),
                IsAm     = IsAm,
                Timezone = Timezone
            };

            await _sessionService.SaveNewbieShiftAsync(record);
            _nav.NavigateTo<ReviewViewModel>();
        });
    }

    [RelayCommand]
    private void Discard()
    {
        // Skip the shift record and go straight to review
        _nav.NavigateTo<ReviewViewModel>();
    }

    [RelayCommand]
    private void Back() => _nav.GoBack();
}

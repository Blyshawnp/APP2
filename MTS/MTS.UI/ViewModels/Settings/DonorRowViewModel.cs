using CommunityToolkit.Mvvm.ComponentModel;
using MTS.Core.Models.Settings;

namespace MTS.UI.ViewModels.Settings;

/// <summary>
/// Observable wrapper for a Donor.
/// Donor category (New / Existing / Increase) is determined by which collection this VM lives in
/// (SettingsViewModel.NewDonors / ExistingMembers / IncreaseSustaining) — not stored on the VM itself.
/// </summary>
public partial class DonorRowViewModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName  = string.Empty;
    [ObservableProperty] private string _address   = string.Empty;
    [ObservableProperty] private string _city      = string.Empty;
    [ObservableProperty] private string _state     = string.Empty;
    [ObservableProperty] private string _zipCode   = string.Empty;
    [ObservableProperty] private string _phone     = string.Empty;
    [ObservableProperty] private string _email     = string.Empty;
    [ObservableProperty] private bool   _isEnabled = true;

    public string DisplayLabel
    {
        get
        {
            var name = $"{FirstName} {LastName}".Trim();
            return string.IsNullOrWhiteSpace(name) ? "(new donor)" : name;
        }
    }

    partial void OnFirstNameChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));
    partial void OnLastNameChanged(string value)  => OnPropertyChanged(nameof(DisplayLabel));

    // ---- Factory ----

    public static DonorRowViewModel FromDomain(Donor d) => new()
    {
        Id        = d.Id,
        FirstName = d.FirstName,
        LastName  = d.LastName,
        Address   = d.Address,
        City      = d.City,
        State     = d.State,
        ZipCode   = d.ZipCode,
        Phone     = d.Phone,
        Email     = d.Email,
        IsEnabled = d.IsEnabled
    };

    public Donor ToDomain() => new()
    {
        Id        = Id,
        FirstName = FirstName,
        LastName  = LastName,
        Address   = Address,
        City      = City,
        State     = State,
        ZipCode   = ZipCode,
        Phone     = Phone,
        Email     = Email,
        IsEnabled = IsEnabled
    };
}

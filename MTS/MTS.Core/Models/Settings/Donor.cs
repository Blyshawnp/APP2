namespace MTS.Core.Models.Settings;

public class Donor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
    public string DisplayName => $"{FirstName} {LastName}".Trim();
    public string FullAddress => $"{Address}, {City}, {State} {ZipCode}".Trim();
}

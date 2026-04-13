namespace MTS.Core.Models.Settings;

public class Show
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal OneTimeAmount { get; set; }
    public decimal MonthlyAmount { get; set; }
    public string GiftDescription { get; set; } = string.Empty;
}

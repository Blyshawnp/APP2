namespace MTS.Core.Models.Settings;

public class CoachingCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Label { get; set; } = string.Empty;
    public bool IsOther { get; set; }
    public bool AppliesToCalls { get; set; } = true;
    public bool AppliesToSupTransfers { get; set; }
    public List<CoachingSubItem> SubItems { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
}

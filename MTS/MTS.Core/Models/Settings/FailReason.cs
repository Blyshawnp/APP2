namespace MTS.Core.Models.Settings;

public class FailReason
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Label { get; set; } = string.Empty;
    public bool IsOther { get; set; }
    public bool AppliesToCalls { get; set; } = true;
    public bool AppliesToSupTransfers { get; set; }
}

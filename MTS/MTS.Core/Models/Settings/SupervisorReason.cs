namespace MTS.Core.Models.Settings;

public class SupervisorReason
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Label { get; set; } = string.Empty;
}

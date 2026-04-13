using MTS.Core.Enums;

namespace MTS.Core.Models.Settings;

public class CallType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Label { get; set; } = string.Empty;
    public CallTypeCategory Category { get; set; }
}

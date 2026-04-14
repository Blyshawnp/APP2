using MTS.Core.Enums;

namespace MTS.Core.Models.History;

public class SessionSummary
{
    public Guid Id { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string TesterName { get; set; } = string.Empty;
    public SessionStatus Status { get; set; }
    public AutoFailReason? AutoFailReason { get; set; }
    public int CallsPassed { get; set; }
    public int CallsFailed { get; set; }
    public int SupsPassed { get; set; }
    public bool HasNewbieShift { get; set; }
    public bool IsSupervisorOnly { get; set; }
    public bool IsFinalAttempt { get; set; }
    public DateTime CreatedAt { get; set; }
}

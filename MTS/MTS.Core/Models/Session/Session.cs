using MTS.Core.Enums;

namespace MTS.Core.Models.Session;

public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public SessionStatus Status { get; set; } = SessionStatus.Draft;

    public TesterInfo TesterInfo { get; set; } = new();
    public CandidateInfo Candidate { get; set; } = new();
    public PreChecks PreChecks { get; set; } = new();

    public AutoFailReason? AutoFailReason { get; set; }

    public List<CallRecord> Calls { get; set; } = new();
    public List<SupTransferRecord> SupTransfers { get; set; } = new();
    public NewbieShiftRecord? NewbieShift { get; set; }
    public List<TechIssueRecord> TechIssues { get; set; } = new();

    public string CoachingSummary { get; set; } = string.Empty;
    public string FailSummary { get; set; } = string.Empty;

    public bool IsSupervisorOnly { get; set; }
    public bool TimeForSup { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Derived helpers
    public int CallsPassed => Calls.Count(c => c.IsPassed);
    public int CallsFailed => Calls.Count(c => c.IsFailed);
    public int SupsPassed => SupTransfers.Count(s => s.IsPassed);
    public bool HasAutoFail => AutoFailReason.HasValue;
}

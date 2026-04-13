using MTS.Core.Enums;

namespace MTS.Core.Models.Session;

public class TechIssueRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public TechIssueType IssueType { get; set; }
    public string OtherNotes { get; set; } = string.Empty;
    public bool Resolved { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

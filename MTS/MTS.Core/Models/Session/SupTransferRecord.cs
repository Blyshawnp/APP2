using MTS.Core.Enums;

namespace MTS.Core.Models.Session;

public class SupTransferRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int TransferNumber { get; set; }

    // Setup
    public Guid ShowId { get; set; }
    public string ShowName { get; set; } = string.Empty;
    public Guid CallerId { get; set; }
    public string CallerDisplayName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    // Result
    public CallResult? Result { get; set; }

    // Coaching
    public List<CoachingSelection> CoachingItems { get; set; } = new();
    public string CoachingNotes { get; set; } = string.Empty;

    // Fail reasons
    public List<FailSelection> FailItems { get; set; } = new();
    public string FailNotes { get; set; } = string.Empty;

    // Derived
    public bool IsPassed => Result == CallResult.Pass;
    public bool IsCompleted => Result.HasValue;
}

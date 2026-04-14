using MTS.Core.Enums;

namespace MTS.Core.Models.Session;

public class CallRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int CallNumber { get; set; }

    // Setup
    public CallTypeCategory CallTypeCategory { get; set; }
    public string CallTypeLabel { get; set; } = string.Empty;
    public Guid ShowId { get; set; }
    public string ShowName { get; set; } = string.Empty;
    public Guid CallerId { get; set; }
    public string CallerDisplayName { get; set; } = string.Empty;
    public decimal DonationAmount { get; set; }
    public ScenarioFlags ScenarioFlags { get; set; } = new();

    // Result
    public CallResult? Result { get; set; }

    // Coaching
    public List<CoachingSelection> CoachingItems { get; set; } = new();
    public string CoachingNotes { get; set; } = string.Empty;

    // Fail reasons
    public List<FailSelection> FailItems { get; set; } = new();
    public string FailNotes { get; set; } = string.Empty;

    // Derived helpers
    public bool IsPassed => Result == CallResult.Pass;
    public bool IsFailed => Result == CallResult.Fail;
    public bool IsCompleted => Result.HasValue;
}

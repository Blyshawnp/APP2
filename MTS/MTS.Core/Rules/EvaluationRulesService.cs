using MTS.Core.Enums;
using MTS.Core.Models.Session;

namespace MTS.Core.Rules;

/// <summary>
/// Centralizes all business rules for session evaluation.
/// No UI, no infrastructure — pure domain logic only.
/// </summary>
public class EvaluationRulesService
{
    // BR-08: Speed test thresholds
    private const double MinDownloadMbps = 25.0;
    private const double MinUploadMbps   = 10.0;

    // -------------------------------------------------------------------------
    // Call routing rules
    // -------------------------------------------------------------------------

    /// <summary>
    /// BR-03: If Call 1 AND Call 2 both pass, Call 3 must be hidden.
    /// </summary>
    public bool ShouldHideCall3(CallResult? call1Result, CallResult? call2Result)
        => call1Result == CallResult.Pass && call2Result == CallResult.Pass;

    /// <summary>
    /// BR-02: Two or more failed calls means the session is an automatic fail.
    /// </summary>
    public bool IsAutoFailByCallCount(IReadOnlyList<CallRecord> calls)
        => calls.Count(c => c.IsFailed) >= 2;

    /// <summary>
    /// BR-01: To qualify for supervisor transfers, the tester must have
    /// at least one New Donor pass AND at least one Existing Member pass.
    /// </summary>
    public bool HasSufficientPassTypes(IReadOnlyList<CallRecord> completedCalls)
    {
        var passes = completedCalls.Where(c => c.IsPassed).ToList();
        bool hasNewDonorPass      = passes.Any(c => c.CallTypeCategory == CallTypeCategory.NewDonor);
        bool hasExistingMemberPass = passes.Any(c => c.CallTypeCategory == CallTypeCategory.ExistingMember);
        return hasNewDonorPass && hasExistingMemberPass;
    }

    /// <summary>
    /// Returns a warning message if the tester has 2 passes but they are
    /// the same donor type — the pass types requirement will not be met.
    /// </summary>
    public string? GetPassTypeWarning(IReadOnlyList<CallRecord> completedCalls)
    {
        var passes = completedCalls.Where(c => c.IsPassed).ToList();
        if (passes.Count < 2) return null;

        bool hasNewDonorPass      = passes.Any(c => c.CallTypeCategory == CallTypeCategory.NewDonor);
        bool hasExistingMemberPass = passes.Any(c => c.CallTypeCategory == CallTypeCategory.ExistingMember);

        if (!hasNewDonorPass)
            return "Both passes are Existing Member calls. You need one New Donor pass to qualify.";
        if (!hasExistingMemberPass)
            return "Both passes are New Donor calls. You need one Existing Member pass to qualify.";

        return null;
    }

    /// <summary>
    /// Determines whether the session has enough call passes to move
    /// to the Supervisor Transfer phase.
    /// </summary>
    public bool CanProceedToSupervisorTransfer(IReadOnlyList<CallRecord> calls)
        => calls.Count(c => c.IsPassed) >= 2;

    // -------------------------------------------------------------------------
    // Session status derivation
    // -------------------------------------------------------------------------

    /// <summary>
    /// BR-09: Derives the final session status from completed data.
    /// </summary>
    public SessionStatus ComputeFinalStatus(Session session)
    {
        if (session.HasAutoFail)
            return SessionStatus.Fail;

        if (session.IsSupervisorOnly)
        {
            if (session.SupsPassed >= 1)
                return SessionStatus.Pass;
            if (session.NewbieShift != null)
                return SessionStatus.Incomplete;
            return SessionStatus.Fail;
        }

        // Full session
        if (session.CallsPassed >= 2)
        {
            if (session.SupsPassed >= 1)
                return SessionStatus.Pass;
            if (session.NewbieShift != null)
                return SessionStatus.Incomplete;
        }

        return SessionStatus.Fail;
    }

    /// <summary>
    /// Returns true when the session requires a Newbie Shift to be scheduled.
    /// This occurs when all supervisor transfers have failed.
    /// </summary>
    public bool RequiresNewbieShift(Session session)
    {
        if (session.IsSupervisorOnly)
            return session.SupTransfers.Count >= 2 && session.SupsPassed == 0;

        return session.TimeForSup
            && session.SupTransfers.Count >= 2
            && session.SupsPassed == 0;
    }

    // -------------------------------------------------------------------------
    // Pre-check / auto-fail rules
    // -------------------------------------------------------------------------

    /// <summary>
    /// BR-07: Evaluates whether the pre-check data triggers an immediate auto-fail.
    /// Returns the reason if so, or null if the candidate can continue.
    /// </summary>
    public AutoFailReason? EvaluatePreCheckAutoFail(Models.Session.PreChecks checks)
    {
        if (checks.HeadsetUsb == false || checks.NoiseCancelling == false)
            return AutoFailReason.WrongHeadset;
        if (checks.VpnPresent == true && checks.VpnCanDisable == false)
            return AutoFailReason.VpnCantDisable;
        return null;
    }

    // -------------------------------------------------------------------------
    // Tech issue rules
    // -------------------------------------------------------------------------

    /// <summary>
    /// BR-08: Speed test fails when download is below 25 Mbps or upload below 10 Mbps.
    /// </summary>
    public bool IsSpeedTestFail(double downloadMbps, double uploadMbps)
        => downloadMbps < MinDownloadMbps || uploadMbps < MinUploadMbps;

    // -------------------------------------------------------------------------
    // Progress calculation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns 0–100 progress for the calls phase based on completed calls
    /// and whether Call 3 is in play.
    /// </summary>
    public int ComputeCallsProgress(IReadOnlyList<CallRecord> calls, bool call3Hidden)
    {
        int totalCalls     = call3Hidden ? 2 : 3;
        int completedCalls = calls.Count(c => c.IsCompleted);
        return (int)Math.Round((double)completedCalls / totalCalls * 100);
    }
}

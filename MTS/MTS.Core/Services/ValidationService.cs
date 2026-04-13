using MTS.Core.Enums;
using MTS.Core.Models.Session;

namespace MTS.Core.Services;

/// <summary>
/// Centralizes all call and transfer validation rules.
/// Returns human-readable error lists consumed by ViewModels for display.
/// </summary>
public class ValidationService
{
    // -------------------------------------------------------------------------
    // Call validation
    // -------------------------------------------------------------------------

    public ValidationResult ValidateCallRecord(CallRecord call)
    {
        var errors = new List<string>();

        if (call.Result == null)
        {
            errors.Add("Select Pass or Fail before continuing.");
            return new ValidationResult(errors); // early exit — further checks need Result
        }

        if (call.Result == CallResult.Fail)
        {
            if (call.FailItems.Count == 0)
                errors.Add("Select at least one fail reason.");

            var otherFail = call.FailItems.FirstOrDefault(f => f.IsOther);
            if (otherFail != null && string.IsNullOrWhiteSpace(call.FailNotes))
                errors.Add("Enter notes for the \"Other\" fail reason.");
        }

        var otherCoaching = call.CoachingItems.FirstOrDefault(c => c.IsOther);
        if (otherCoaching != null && string.IsNullOrWhiteSpace(call.CoachingNotes))
            errors.Add("Enter notes for the \"Other\" coaching category.");

        return new ValidationResult(errors);
    }

    public ValidationResult ValidateCallSetup(CallRecord call)
    {
        var errors = new List<string>();

        if (call.ShowId == Guid.Empty)
            errors.Add("Select a show.");
        if (call.CallerId == Guid.Empty)
            errors.Add("Select a caller.");
        if (string.IsNullOrWhiteSpace(call.CallTypeLabel))
            errors.Add("Select a call type.");
        if (call.DonationAmount <= 0)
            errors.Add("Enter a donation amount.");

        return new ValidationResult(errors);
    }

    // -------------------------------------------------------------------------
    // Supervisor transfer validation
    // -------------------------------------------------------------------------

    public ValidationResult ValidateSupTransferRecord(SupTransferRecord transfer)
    {
        var errors = new List<string>();

        if (transfer.Result == null)
        {
            errors.Add("Select Pass or Fail before continuing.");
            return new ValidationResult(errors);
        }

        if (transfer.Result == CallResult.Fail)
        {
            if (transfer.FailItems.Count == 0)
                errors.Add("Select at least one fail reason.");

            var otherFail = transfer.FailItems.FirstOrDefault(f => f.IsOther);
            if (otherFail != null && string.IsNullOrWhiteSpace(transfer.FailNotes))
                errors.Add("Enter notes for the \"Other\" fail reason.");
        }

        var otherCoaching = transfer.CoachingItems.FirstOrDefault(c => c.IsOther);
        if (otherCoaching != null && string.IsNullOrWhiteSpace(transfer.CoachingNotes))
            errors.Add("Enter notes for the \"Other\" coaching category.");

        return new ValidationResult(errors);
    }

    // -------------------------------------------------------------------------
    // Pre-check validation
    // -------------------------------------------------------------------------

    public ValidationResult ValidatePreChecks(PreChecks checks, string candidateName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(candidateName))
            errors.Add("Enter the candidate's name.");
        if (checks.HeadsetUsb == null)
            errors.Add("Answer the USB headset question.");
        if (checks.NoiseCancelling == null)
            errors.Add("Answer the noise-cancelling question.");
        if (string.IsNullOrWhiteSpace(checks.HeadsetBrand))
            errors.Add("Enter the headset brand.");
        if (checks.VpnPresent == null)
            errors.Add("Answer the VPN question.");
        if (checks.VpnPresent == true && checks.VpnCanDisable == null)
            errors.Add("Answer whether the VPN can be disabled.");
        if (checks.ChromeIsDefault == null)
            errors.Add("Answer the default browser question.");
        if (checks.ExtensionsDisabled == null)
            errors.Add("Answer the extensions question.");
        if (checks.PopupsAllowed == null)
            errors.Add("Answer the pop-ups question.");

        return new ValidationResult(errors);
    }

    // -------------------------------------------------------------------------
    // Newbie shift validation
    // -------------------------------------------------------------------------

    public ValidationResult ValidateNewbieShift(NewbieShiftRecord shift)
    {
        var errors = new List<string>();

        if (shift.Date == default)
            errors.Add("Select a date.");
        if (shift.Date < DateOnly.FromDateTime(DateTime.Today))
            errors.Add("Date must be today or in the future.");

        return new ValidationResult(errors);
    }
}

// -------------------------------------------------------------------------
// Result type
// -------------------------------------------------------------------------

public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public IReadOnlyList<string> Errors { get; }

    public ValidationResult(List<string> errors)
        => Errors = errors.AsReadOnly();

    public static ValidationResult Success()
        => new(new List<string>());
}

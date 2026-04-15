using FluentAssertions;
using MTS.Core.Enums;
using MTS.Core.Models.Session;
using MTS.Core.Services;

namespace MTS.Tests.Unit.Validators;

public class ValidationServiceTests
{
    private readonly ValidationService _sut = new();

    // =========================================================================
    // ValidateCallRecord
    // =========================================================================

    [Fact]
    public void ValidateCallRecord_NoResultSelected_ReturnsError()
    {
        var call = new CallRecord { Result = null };
        var result = _sut.ValidateCallRecord(call);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Pass or Fail"));
    }

    [Fact]
    public void ValidateCallRecord_Pass_NoOtherItems_IsValid()
    {
        var call = new CallRecord { Result = CallResult.Pass };
        _sut.ValidateCallRecord(call).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCallRecord_Fail_NoFailItemsSelected_ReturnsError()
    {
        var call = new CallRecord { Result = CallResult.Fail, FailItems = new List<FailSelection>() };
        var result = _sut.ValidateCallRecord(call);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("fail reason"));
    }

    [Fact]
    public void ValidateCallRecord_Fail_WithFailItems_IsValid()
    {
        var call = new CallRecord
        {
            Result    = CallResult.Fail,
            FailItems = new List<FailSelection> { new() { ReasonLabel = "Bad tone" } },
        };
        _sut.ValidateCallRecord(call).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCallRecord_Fail_OtherFailReasonWithoutNotes_ReturnsError()
    {
        var call = new CallRecord
        {
            Result    = CallResult.Fail,
            FailItems = new List<FailSelection> { new() { IsOther = true } },
            FailNotes = string.Empty,
        };
        var result = _sut.ValidateCallRecord(call);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("notes") && e.Contains("Other"));
    }

    [Fact]
    public void ValidateCallRecord_Fail_OtherFailReasonWithNotes_IsValid()
    {
        var call = new CallRecord
        {
            Result    = CallResult.Fail,
            FailItems = new List<FailSelection> { new() { IsOther = true } },
            FailNotes = "Some detail",
        };
        _sut.ValidateCallRecord(call).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCallRecord_OtherCoachingWithoutNotes_ReturnsError()
    {
        var call = new CallRecord
        {
            Result        = CallResult.Pass,
            CoachingItems = new List<CoachingSelection> { new() { IsOther = true } },
            CoachingNotes = string.Empty,
        };
        var result = _sut.ValidateCallRecord(call);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("coaching"));
    }

    [Fact]
    public void ValidateCallRecord_OtherCoachingWithNotes_IsValid()
    {
        var call = new CallRecord
        {
            Result        = CallResult.Pass,
            CoachingItems = new List<CoachingSelection> { new() { IsOther = true } },
            CoachingNotes = "Good detail",
        };
        _sut.ValidateCallRecord(call).IsValid.Should().BeTrue();
    }

    // =========================================================================
    // ValidateCallSetup
    // =========================================================================

    [Fact]
    public void ValidateCallSetup_AllFieldsMissing_ReturnsFourErrors()
    {
        var call = new CallRecord
        {
            ShowId        = Guid.Empty,
            CallerId      = Guid.Empty,
            CallTypeLabel = string.Empty,
            DonationAmount = 0,
        };
        var result = _sut.ValidateCallSetup(call);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
    }

    [Fact]
    public void ValidateCallSetup_AllFieldsPresent_IsValid()
    {
        var call = new CallRecord
        {
            ShowId         = Guid.NewGuid(),
            CallerId       = Guid.NewGuid(),
            CallTypeLabel  = "New Donor",
            DonationAmount = 25,
        };
        _sut.ValidateCallSetup(call).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCallSetup_MissingShow_ReturnsError()
    {
        var call = new CallRecord
        {
            ShowId         = Guid.Empty,
            CallerId       = Guid.NewGuid(),
            CallTypeLabel  = "New Donor",
            DonationAmount = 10,
        };
        var result = _sut.ValidateCallSetup(call);
        result.Errors.Should().ContainSingle(e => e.Contains("show"));
    }

    [Fact]
    public void ValidateCallSetup_MissingCaller_ReturnsError()
    {
        var call = new CallRecord
        {
            ShowId         = Guid.NewGuid(),
            CallerId       = Guid.Empty,
            CallTypeLabel  = "New Donor",
            DonationAmount = 10,
        };
        var result = _sut.ValidateCallSetup(call);
        result.Errors.Should().ContainSingle(e => e.Contains("caller"));
    }

    [Fact]
    public void ValidateCallSetup_MissingCallType_ReturnsError()
    {
        var call = new CallRecord
        {
            ShowId         = Guid.NewGuid(),
            CallerId       = Guid.NewGuid(),
            CallTypeLabel  = "   ",
            DonationAmount = 10,
        };
        var result = _sut.ValidateCallSetup(call);
        result.Errors.Should().ContainSingle(e => e.Contains("call type"));
    }

    [Fact]
    public void ValidateCallSetup_ZeroDonation_ReturnsError()
    {
        var call = new CallRecord
        {
            ShowId         = Guid.NewGuid(),
            CallerId       = Guid.NewGuid(),
            CallTypeLabel  = "New Donor",
            DonationAmount = 0,
        };
        var result = _sut.ValidateCallSetup(call);
        result.Errors.Should().ContainSingle(e => e.Contains("donation"));
    }

    // =========================================================================
    // ValidateSupTransferRecord
    // =========================================================================

    [Fact]
    public void ValidateSupTransfer_NoResultSelected_ReturnsError()
    {
        var transfer = new SupTransferRecord { Result = null };
        var result = _sut.ValidateSupTransferRecord(transfer);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Pass or Fail"));
    }

    [Fact]
    public void ValidateSupTransfer_Pass_IsValid()
    {
        var transfer = new SupTransferRecord { Result = CallResult.Pass };
        _sut.ValidateSupTransferRecord(transfer).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateSupTransfer_Fail_NoFailItems_ReturnsError()
    {
        var transfer = new SupTransferRecord { Result = CallResult.Fail };
        var result = _sut.ValidateSupTransferRecord(transfer);
        result.Errors.Should().ContainSingle(e => e.Contains("fail reason"));
    }

    [Fact]
    public void ValidateSupTransfer_Fail_OtherReasonWithoutNotes_ReturnsError()
    {
        var transfer = new SupTransferRecord
        {
            Result    = CallResult.Fail,
            FailItems = new List<FailSelection> { new() { IsOther = true } },
            FailNotes = string.Empty,
        };
        var result = _sut.ValidateSupTransferRecord(transfer);
        result.Errors.Should().ContainSingle(e => e.Contains("notes") && e.Contains("Other"));
    }

    [Fact]
    public void ValidateSupTransfer_OtherCoachingWithoutNotes_ReturnsError()
    {
        var transfer = new SupTransferRecord
        {
            Result        = CallResult.Pass,
            CoachingItems = new List<CoachingSelection> { new() { IsOther = true } },
            CoachingNotes = string.Empty,
        };
        var result = _sut.ValidateSupTransferRecord(transfer);
        result.Errors.Should().ContainSingle(e => e.Contains("coaching"));
    }

    // =========================================================================
    // ValidatePreChecks
    // =========================================================================

    [Fact]
    public void ValidatePreChecks_EmptyState_ReturnsMultipleErrors()
    {
        var checks = new PreChecks();
        var result = _sut.ValidatePreChecks(checks, string.Empty);
        result.IsValid.Should().BeFalse();
        // candidateName + HeadsetUsb + NoiseCancelling + HeadsetBrand + VpnPresent + ChromeIsDefault + ExtensionsDisabled + PopupsAllowed = 8
        result.Errors.Should().HaveCount(8);
    }

    [Fact]
    public void ValidatePreChecks_MissingCandidateName_ReturnsError()
    {
        var checks = FullyPopulatedPreChecks();
        var result = _sut.ValidatePreChecks(checks, " ");
        result.Errors.Should().ContainSingle(e => e.Contains("candidate"));
    }

    [Fact]
    public void ValidatePreChecks_VpnPresentButDisableAnswerMissing_ReturnsError()
    {
        var checks = FullyPopulatedPreChecks();
        checks.VpnPresent    = true;
        checks.VpnCanDisable = null;
        var result = _sut.ValidatePreChecks(checks, "Alice");
        result.Errors.Should().ContainSingle(e => e.Contains("VPN can be disabled"));
    }

    [Fact]
    public void ValidatePreChecks_VpnNotPresent_NoVpnDisableQuestionRequired()
    {
        var checks = FullyPopulatedPreChecks();
        checks.VpnPresent    = false;
        checks.VpnCanDisable = null; // unanswered — but should NOT be required when VPN absent
        var result = _sut.ValidatePreChecks(checks, "Alice");
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePreChecks_AllAnswered_IsValid()
    {
        var checks = FullyPopulatedPreChecks();
        _sut.ValidatePreChecks(checks, "Alice").IsValid.Should().BeTrue();
    }

    // =========================================================================
    // ValidateNewbieShift
    // =========================================================================

    [Fact]
    public void ValidateNewbieShift_DefaultDate_ReturnsSelectDateError()
    {
        var shift = new NewbieShiftRecord { Date = default };
        var result = _sut.ValidateNewbieShift(shift);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("Select a date"));
    }

    [Fact]
    public void ValidateNewbieShift_PastDate_ReturnsFutureError()
    {
        var shift = new NewbieShiftRecord { Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) };
        var result = _sut.ValidateNewbieShift(shift);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("future"));
    }

    [Fact]
    public void ValidateNewbieShift_Today_IsValid()
    {
        var shift = new NewbieShiftRecord { Date = DateOnly.FromDateTime(DateTime.Today) };
        _sut.ValidateNewbieShift(shift).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateNewbieShift_FutureDate_IsValid()
    {
        var shift = new NewbieShiftRecord { Date = DateOnly.FromDateTime(DateTime.Today.AddDays(7)) };
        _sut.ValidateNewbieShift(shift).IsValid.Should().BeTrue();
    }

    // =========================================================================
    // ValidationResult
    // =========================================================================

    [Fact]
    public void ValidationResult_Success_IsValidTrue()
    {
        var result = ValidationResult.Success();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static PreChecks FullyPopulatedPreChecks() => new()
    {
        HeadsetUsb         = true,
        NoiseCancelling     = true,
        HeadsetBrand       = "Jabra",
        VpnPresent         = false,
        VpnCanDisable      = null,
        ChromeIsDefault    = true,
        ExtensionsDisabled = true,
        PopupsAllowed      = true,
    };
}

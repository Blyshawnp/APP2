using FluentAssertions;
using MTS.Core.Enums;
using MTS.Core.Models.Session;
using MTS.Core.Rules;

namespace MTS.Tests.Unit.Rules;

public class EvaluationRulesServiceTests
{
    private readonly EvaluationRulesService _sut = new();

    // -------------------------------------------------------------------------
    // BR-03: ShouldHideCall3
    // -------------------------------------------------------------------------

    [Fact]
    public void ShouldHideCall3_BothPass_ReturnsTrue()
    {
        _sut.ShouldHideCall3(CallResult.Pass, CallResult.Pass).Should().BeTrue();
    }

    [Theory]
    [InlineData(CallResult.Fail,  CallResult.Pass)]
    [InlineData(CallResult.Pass,  CallResult.Fail)]
    [InlineData(CallResult.Fail,  CallResult.Fail)]
    public void ShouldHideCall3_NotBothPass_ReturnsFalse(CallResult r1, CallResult r2)
    {
        _sut.ShouldHideCall3(r1, r2).Should().BeFalse();
    }

    [Theory]
    [InlineData(null,             CallResult.Pass)]
    [InlineData(CallResult.Pass,  null)]
    [InlineData(null,             null)]
    public void ShouldHideCall3_NullResult_ReturnsFalse(CallResult? r1, CallResult? r2)
    {
        _sut.ShouldHideCall3(r1, r2).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // BR-02: IsAutoFailByCallCount
    // -------------------------------------------------------------------------

    [Fact]
    public void IsAutoFailByCallCount_TwoFails_ReturnsTrue()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Fail },
            new() { Result = CallResult.Fail },
        };
        _sut.IsAutoFailByCallCount(calls).Should().BeTrue();
    }

    [Fact]
    public void IsAutoFailByCallCount_ThreeFails_ReturnsTrue()
    {
        var calls = Enumerable.Range(0, 3)
            .Select(_ => new CallRecord { Result = CallResult.Fail })
            .ToList();
        _sut.IsAutoFailByCallCount(calls).Should().BeTrue();
    }

    [Fact]
    public void IsAutoFailByCallCount_OneFail_ReturnsFalse()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Fail },
            new() { Result = CallResult.Pass },
        };
        _sut.IsAutoFailByCallCount(calls).Should().BeFalse();
    }

    [Fact]
    public void IsAutoFailByCallCount_NoFails_ReturnsFalse()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass },
            new() { Result = CallResult.Pass },
        };
        _sut.IsAutoFailByCallCount(calls).Should().BeFalse();
    }

    [Fact]
    public void IsAutoFailByCallCount_EmptyList_ReturnsFalse()
    {
        _sut.IsAutoFailByCallCount(new List<CallRecord>()).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // BR-01: HasSufficientPassTypes
    // -------------------------------------------------------------------------

    [Fact]
    public void HasSufficientPassTypes_OneNewDonorOneExistingMember_ReturnsTrue()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.NewDonor },
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.ExistingMember },
        };
        _sut.HasSufficientPassTypes(calls).Should().BeTrue();
    }

    [Fact]
    public void HasSufficientPassTypes_BothNewDonor_ReturnsFalse()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.NewDonor },
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.NewDonor },
        };
        _sut.HasSufficientPassTypes(calls).Should().BeFalse();
    }

    [Fact]
    public void HasSufficientPassTypes_BothExistingMember_ReturnsFalse()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.ExistingMember },
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.ExistingMember },
        };
        _sut.HasSufficientPassTypes(calls).Should().BeFalse();
    }

    [Fact]
    public void HasSufficientPassTypes_NoPassedCalls_ReturnsFalse()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Fail, CallTypeCategory = CallTypeCategory.NewDonor },
            new() { Result = CallResult.Fail, CallTypeCategory = CallTypeCategory.ExistingMember },
        };
        _sut.HasSufficientPassTypes(calls).Should().BeFalse();
    }

    [Fact]
    public void HasSufficientPassTypes_FailedCallsIgnored_RequiresBothPassTypes()
    {
        // 1 NewDonor pass + 1 ExistingMember fail — should not qualify
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.NewDonor },
            new() { Result = CallResult.Fail, CallTypeCategory = CallTypeCategory.ExistingMember },
        };
        _sut.HasSufficientPassTypes(calls).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // GetPassTypeWarning
    // -------------------------------------------------------------------------

    [Fact]
    public void GetPassTypeWarning_LessThanTwoPasses_ReturnsNull()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.NewDonor },
        };
        _sut.GetPassTypeWarning(calls).Should().BeNull();
    }

    [Fact]
    public void GetPassTypeWarning_BothNewDonor_ReturnsExistingMemberMessage()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.NewDonor },
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.NewDonor },
        };
        var warning = _sut.GetPassTypeWarning(calls);
        warning.Should().Contain("Existing Member");
    }

    [Fact]
    public void GetPassTypeWarning_BothExistingMember_ReturnsNewDonorMessage()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.ExistingMember },
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.ExistingMember },
        };
        var warning = _sut.GetPassTypeWarning(calls);
        warning.Should().Contain("New Donor");
    }

    [Fact]
    public void GetPassTypeWarning_MixedPassTypes_ReturnsNull()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.NewDonor },
            new() { Result = CallResult.Pass, CallTypeCategory = CallTypeCategory.ExistingMember },
        };
        _sut.GetPassTypeWarning(calls).Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // CanProceedToSupervisorTransfer
    // -------------------------------------------------------------------------

    [Fact]
    public void CanProceedToSupervisorTransfer_TwoPasses_ReturnsTrue()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass },
            new() { Result = CallResult.Pass },
        };
        _sut.CanProceedToSupervisorTransfer(calls).Should().BeTrue();
    }

    [Fact]
    public void CanProceedToSupervisorTransfer_OnlyOnePass_ReturnsFalse()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass },
            new() { Result = CallResult.Fail },
        };
        _sut.CanProceedToSupervisorTransfer(calls).Should().BeFalse();
    }

    [Fact]
    public void CanProceedToSupervisorTransfer_NoPasses_ReturnsFalse()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Fail },
        };
        _sut.CanProceedToSupervisorTransfer(calls).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // BR-09: ComputeFinalStatus
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputeFinalStatus_HasAutoFail_ReturnsFail()
    {
        var session = new Session { AutoFailReason = AutoFailReason.NcNs };
        _sut.ComputeFinalStatus(session).Should().Be(SessionStatus.Fail);
    }

    [Fact]
    public void ComputeFinalStatus_SupervisorOnly_OneSupPass_ReturnsPass()
    {
        var session = new Session
        {
            IsSupervisorOnly = true,
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Pass },
            }
        };
        _sut.ComputeFinalStatus(session).Should().Be(SessionStatus.Pass);
    }

    [Fact]
    public void ComputeFinalStatus_SupervisorOnly_NoPassWithNewbieShift_ReturnsIncomplete()
    {
        var session = new Session
        {
            IsSupervisorOnly = true,
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Fail },
            },
            NewbieShift = new NewbieShiftRecord { Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) }
        };
        _sut.ComputeFinalStatus(session).Should().Be(SessionStatus.Incomplete);
    }

    [Fact]
    public void ComputeFinalStatus_SupervisorOnly_NoPassNoNewbieShift_ReturnsFail()
    {
        var session = new Session
        {
            IsSupervisorOnly = true,
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Fail },
            }
        };
        _sut.ComputeFinalStatus(session).Should().Be(SessionStatus.Fail);
    }

    [Fact]
    public void ComputeFinalStatus_FullSession_TwoCallPassesOneSupPass_ReturnsPass()
    {
        var session = new Session
        {
            Calls = new List<CallRecord>
            {
                new() { Result = CallResult.Pass },
                new() { Result = CallResult.Pass },
            },
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Pass },
            }
        };
        _sut.ComputeFinalStatus(session).Should().Be(SessionStatus.Pass);
    }

    [Fact]
    public void ComputeFinalStatus_FullSession_TwoCallPassesNoSupPassWithNewbieShift_ReturnsIncomplete()
    {
        var session = new Session
        {
            Calls = new List<CallRecord>
            {
                new() { Result = CallResult.Pass },
                new() { Result = CallResult.Pass },
            },
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Fail },
            },
            NewbieShift = new NewbieShiftRecord { Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) }
        };
        _sut.ComputeFinalStatus(session).Should().Be(SessionStatus.Incomplete);
    }

    [Fact]
    public void ComputeFinalStatus_FullSession_LessThanTwoCallPasses_ReturnsFail()
    {
        var session = new Session
        {
            Calls = new List<CallRecord>
            {
                new() { Result = CallResult.Pass },
                new() { Result = CallResult.Fail },
            },
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Pass },
            }
        };
        _sut.ComputeFinalStatus(session).Should().Be(SessionStatus.Fail);
    }

    // -------------------------------------------------------------------------
    // RequiresNewbieShift
    // -------------------------------------------------------------------------

    [Fact]
    public void RequiresNewbieShift_SupervisorOnly_TwoTransfersAllFailed_ReturnsTrue()
    {
        var session = new Session
        {
            IsSupervisorOnly = true,
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Fail },
                new() { Result = CallResult.Fail },
            }
        };
        _sut.RequiresNewbieShift(session).Should().BeTrue();
    }

    [Fact]
    public void RequiresNewbieShift_SupervisorOnly_OnlyOneTransfer_ReturnsFalse()
    {
        var session = new Session
        {
            IsSupervisorOnly = true,
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Fail },
            }
        };
        _sut.RequiresNewbieShift(session).Should().BeFalse();
    }

    [Fact]
    public void RequiresNewbieShift_FullSession_TimeForSupTwoFailedTransfers_ReturnsTrue()
    {
        var session = new Session
        {
            TimeForSup = true,
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Fail },
                new() { Result = CallResult.Fail },
            }
        };
        _sut.RequiresNewbieShift(session).Should().BeTrue();
    }

    [Fact]
    public void RequiresNewbieShift_FullSession_NotTimeForSup_ReturnsFalse()
    {
        var session = new Session
        {
            TimeForSup = false,
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Fail },
                new() { Result = CallResult.Fail },
            }
        };
        _sut.RequiresNewbieShift(session).Should().BeFalse();
    }

    [Fact]
    public void RequiresNewbieShift_FullSession_OneTransferPassed_ReturnsFalse()
    {
        var session = new Session
        {
            TimeForSup = true,
            SupTransfers = new List<SupTransferRecord>
            {
                new() { Result = CallResult.Fail },
                new() { Result = CallResult.Pass },
            }
        };
        _sut.RequiresNewbieShift(session).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // BR-07: EvaluatePreCheckAutoFail
    // -------------------------------------------------------------------------

    [Fact]
    public void EvaluatePreCheckAutoFail_NonUsbHeadset_ReturnsWrongHeadset()
    {
        var checks = new PreChecks { HeadsetUsb = false, NoiseCancelling = true };
        _sut.EvaluatePreCheckAutoFail(checks).Should().Be(AutoFailReason.WrongHeadset);
    }

    [Fact]
    public void EvaluatePreCheckAutoFail_NoNoiseCancelling_ReturnsWrongHeadset()
    {
        var checks = new PreChecks { HeadsetUsb = true, NoiseCancelling = false };
        _sut.EvaluatePreCheckAutoFail(checks).Should().Be(AutoFailReason.WrongHeadset);
    }

    [Fact]
    public void EvaluatePreCheckAutoFail_VpnPresentCannotDisable_ReturnsVpnCantDisable()
    {
        var checks = new PreChecks
        {
            HeadsetUsb     = true,
            NoiseCancelling = true,
            VpnPresent     = true,
            VpnCanDisable  = false,
        };
        _sut.EvaluatePreCheckAutoFail(checks).Should().Be(AutoFailReason.VpnCantDisable);
    }

    [Fact]
    public void EvaluatePreCheckAutoFail_VpnPresentCanDisable_ReturnsNull()
    {
        var checks = new PreChecks
        {
            HeadsetUsb     = true,
            NoiseCancelling = true,
            VpnPresent     = true,
            VpnCanDisable  = true,
        };
        _sut.EvaluatePreCheckAutoFail(checks).Should().BeNull();
    }

    [Fact]
    public void EvaluatePreCheckAutoFail_NoVpn_ReturnsNull()
    {
        var checks = new PreChecks
        {
            HeadsetUsb     = true,
            NoiseCancelling = true,
            VpnPresent     = false,
        };
        _sut.EvaluatePreCheckAutoFail(checks).Should().BeNull();
    }

    [Fact]
    public void EvaluatePreCheckAutoFail_AllGood_ReturnsNull()
    {
        var checks = new PreChecks
        {
            HeadsetUsb      = true,
            NoiseCancelling  = true,
            VpnPresent       = false,
            ChromeIsDefault  = true,
            ExtensionsDisabled = true,
            PopupsAllowed    = true,
        };
        _sut.EvaluatePreCheckAutoFail(checks).Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // BR-08: IsSpeedTestFail
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(24.9, 10.0)]   // download below threshold
    [InlineData(25.0, 9.9)]    // upload below threshold
    [InlineData(0.0,  0.0)]    // both below
    public void IsSpeedTestFail_BelowThresholds_ReturnsTrue(double download, double upload)
    {
        _sut.IsSpeedTestFail(download, upload).Should().BeTrue();
    }

    [Theory]
    [InlineData(25.0, 10.0)]   // exact thresholds — should pass
    [InlineData(100.0, 50.0)]  // well above thresholds
    public void IsSpeedTestFail_AtOrAboveThresholds_ReturnsFalse(double download, double upload)
    {
        _sut.IsSpeedTestFail(download, upload).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // ComputeCallsProgress
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputeCallsProgress_ZeroCompletedOfThree_ReturnsZero()
    {
        var calls = new List<CallRecord> { new(), new(), new() };
        _sut.ComputeCallsProgress(calls, call3Hidden: false).Should().Be(0);
    }

    [Fact]
    public void ComputeCallsProgress_OneCompletedOfThree_Returns33()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass },
            new(),
            new(),
        };
        _sut.ComputeCallsProgress(calls, call3Hidden: false).Should().Be(33);
    }

    [Fact]
    public void ComputeCallsProgress_TwoCompletedOfThree_Returns67()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass },
            new() { Result = CallResult.Fail },
            new(),
        };
        _sut.ComputeCallsProgress(calls, call3Hidden: false).Should().Be(67);
    }

    [Fact]
    public void ComputeCallsProgress_AllThreeCompleted_Returns100()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass },
            new() { Result = CallResult.Fail },
            new() { Result = CallResult.Pass },
        };
        _sut.ComputeCallsProgress(calls, call3Hidden: false).Should().Be(100);
    }

    [Fact]
    public void ComputeCallsProgress_Call3Hidden_TwoCompletedOfTwo_Returns100()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass },
            new() { Result = CallResult.Pass },
        };
        _sut.ComputeCallsProgress(calls, call3Hidden: true).Should().Be(100);
    }

    [Fact]
    public void ComputeCallsProgress_Call3Hidden_OneCompletedOfTwo_Returns50()
    {
        var calls = new List<CallRecord>
        {
            new() { Result = CallResult.Pass },
            new(),
        };
        _sut.ComputeCallsProgress(calls, call3Hidden: true).Should().Be(50);
    }
}

using FluentAssertions;
using MTS.Core.Models.History;

namespace MTS.Tests.Unit.Services;

public class HistoryStatsTests
{
    // -------------------------------------------------------------------------
    // PassRate
    // -------------------------------------------------------------------------

    [Fact]
    public void PassRate_NoSessions_ReturnsZero()
    {
        var stats = new HistoryStats { TotalSessions = 0, TotalPass = 0 };
        stats.PassRate.Should().Be(0);
    }

    [Fact]
    public void PassRate_AllPass_Returns100()
    {
        var stats = new HistoryStats { TotalSessions = 5, TotalPass = 5 };
        stats.PassRate.Should().Be(100);
    }

    [Fact]
    public void PassRate_HalfPass_Returns50()
    {
        var stats = new HistoryStats { TotalSessions = 10, TotalPass = 5 };
        stats.PassRate.Should().Be(50);
    }

    [Fact]
    public void PassRate_OneOfThree_Returns33Point3()
    {
        var stats = new HistoryStats { TotalSessions = 3, TotalPass = 1 };
        // Math.Round(33.333, 1) = 33.3
        stats.PassRate.Should().Be(33.3);
    }

    [Fact]
    public void PassRate_TwoOfThree_Returns66Point7()
    {
        var stats = new HistoryStats { TotalSessions = 3, TotalPass = 2 };
        // Math.Round(66.666, 1) = 66.7
        stats.PassRate.Should().Be(66.7);
    }

    // -------------------------------------------------------------------------
    // NcNsRate
    // -------------------------------------------------------------------------

    [Fact]
    public void NcNsRate_NoSessions_ReturnsZero()
    {
        var stats = new HistoryStats { TotalSessions = 0, TotalNcNs = 0 };
        stats.NcNsRate.Should().Be(0);
    }

    [Fact]
    public void NcNsRate_AllNcNs_Returns100()
    {
        var stats = new HistoryStats { TotalSessions = 4, TotalNcNs = 4 };
        stats.NcNsRate.Should().Be(100);
    }

    [Fact]
    public void NcNsRate_ZeroNcNs_ReturnsZero()
    {
        var stats = new HistoryStats { TotalSessions = 10, TotalNcNs = 0 };
        stats.NcNsRate.Should().Be(0);
    }

    [Fact]
    public void NcNsRate_OneOfFour_Returns25()
    {
        var stats = new HistoryStats { TotalSessions = 4, TotalNcNs = 1 };
        stats.NcNsRate.Should().Be(25);
    }

    [Fact]
    public void NcNsRate_OneOfThree_Returns33Point3()
    {
        var stats = new HistoryStats { TotalSessions = 3, TotalNcNs = 1 };
        stats.NcNsRate.Should().Be(33.3);
    }
}

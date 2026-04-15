using FluentAssertions;
using MTS.Core.Models.Session;

namespace MTS.Tests.Unit.Services;

public class ScenarioFlagsTests
{
    [Fact]
    public void GenerateRandom_ReturnsBoolProperties_NeverNull()
    {
        // Run several times to reduce flakiness from random values
        for (int i = 0; i < 50; i++)
        {
            var flags = ScenarioFlags.GenerateRandom();

            // All properties are non-nullable bool — this test confirms no
            // nullable coercion issues exist at the model level
            _ = flags.HasPhone;
            _ = flags.HasSms;
            _ = flags.HasEnews;
            _ = flags.HasShipping;
            _ = flags.HasCcFee;
        }
    }

    [Fact]
    public void GenerateRandom_OverManyRuns_ProducesBothTrueAndFalseForHasPhone()
    {
        // Statistical test: across 200 runs the property must appear true at least
        // once and false at least once (P(all same) < 2^-199)
        var results = Enumerable.Range(0, 200)
            .Select(_ => ScenarioFlags.GenerateRandom().HasPhone)
            .ToList();

        results.Should().Contain(true);
        results.Should().Contain(false);
    }

    [Fact]
    public void GenerateRandom_OverManyRuns_ProducesBothTrueAndFalseForHasCcFee()
    {
        // CcFee uses a higher threshold (> 0.7) so needs more runs to see both values
        var results = Enumerable.Range(0, 200)
            .Select(_ => ScenarioFlags.GenerateRandom().HasCcFee)
            .ToList();

        results.Should().Contain(true);
        results.Should().Contain(false);
    }

    [Fact]
    public void GenerateRandom_EachCallReturnsNewInstance()
    {
        var a = ScenarioFlags.GenerateRandom();
        var b = ScenarioFlags.GenerateRandom();
        a.Should().NotBeSameAs(b);
    }
}

using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Session;

namespace MTS.Infrastructure.ExternalServices;

/// <summary>
/// No-op summary service used when Gemini AI is not configured.
/// Returns empty strings so the Review screen can still function.
/// Replace with GeminiSummaryService in Phase 5 when Gemini key is set.
/// </summary>
public class NullSummaryService : ISummaryService
{
    public bool IsEnabled => false;

    public Task<string> GenerateCoachingSummaryAsync(Session session)
        => Task.FromResult(string.Empty);

    public Task<string> GenerateFailSummaryAsync(Session session)
        => Task.FromResult(string.Empty);

    public Task<string> RegenerateCoachingSummaryAsync(Session session)
        => Task.FromResult(string.Empty);

    public Task<string> RegenerateFailSummaryAsync(Session session)
        => Task.FromResult(string.Empty);
}

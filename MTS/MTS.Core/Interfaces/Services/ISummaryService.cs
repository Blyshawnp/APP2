using MTS.Core.Models.Session;

namespace MTS.Core.Interfaces.Services;

public interface ISummaryService
{
    bool IsEnabled { get; }
    Task<string> GenerateCoachingSummaryAsync(Session session);
    Task<string> GenerateFailSummaryAsync(Session session);
    Task<string> RegenerateCoachingSummaryAsync(Session session);
    Task<string> RegenerateFailSummaryAsync(Session session);
}

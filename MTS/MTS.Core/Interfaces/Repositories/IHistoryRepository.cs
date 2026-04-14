using MTS.Core.Models.History;
using MTS.Core.Models.Session;

namespace MTS.Core.Interfaces.Repositories;

public interface IHistoryRepository
{
    Task<List<SessionSummary>> GetAllSummariesAsync();
    Task<Session?> GetByIdAsync(Guid id);
    Task<HistoryStats> GetStatsAsync();
    Task<List<SessionSummary>> SearchAsync(string query);
    Task DeleteAllAsync();
}

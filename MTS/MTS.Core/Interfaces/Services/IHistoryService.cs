using MTS.Core.Models.History;
using MTS.Core.Models.Session;

namespace MTS.Core.Interfaces.Services;

public interface IHistoryService
{
    Task<List<SessionSummary>> GetAllAsync();
    Task<Session?> GetByIdAsync(Guid id);
    Task<HistoryStats> GetStatsAsync();
    Task<List<SessionSummary>> SearchAsync(string query);
    Task DeleteAllAsync();
}

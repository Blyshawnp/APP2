using MTS.Core.Interfaces.Repositories;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.History;
using MTS.Core.Models.Session;

namespace MTS.Core.Services;

public class HistoryService : IHistoryService
{
    private readonly IHistoryRepository _repo;

    public HistoryService(IHistoryRepository repo)
        => _repo = repo;

    public Task<List<SessionSummary>> GetAllAsync()    => _repo.GetAllSummariesAsync();
    public Task<Session?> GetByIdAsync(Guid id)        => _repo.GetByIdAsync(id);
    public Task<HistoryStats> GetStatsAsync()           => _repo.GetStatsAsync();
    public Task<List<SessionSummary>> SearchAsync(string query) => _repo.SearchAsync(query);
    public Task DeleteAllAsync()                        => _repo.DeleteAllAsync();

    public async Task<List<SessionSummary>> GetResumableAsync(string testerName)
    {
        var all = await _repo.GetAllSummariesAsync();
        return all
            .Where(s =>
                (string.IsNullOrWhiteSpace(testerName) ||
                 s.TesterName.Equals(testerName, StringComparison.OrdinalIgnoreCase)) &&
                (s.CallsPassed + s.CallsFailed) > 0 &&
                s.SupsPassed == 0)
            .OrderByDescending(s => s.CreatedAt)
            .ToList();
    }
}

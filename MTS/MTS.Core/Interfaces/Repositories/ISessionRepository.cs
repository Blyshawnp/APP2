using MTS.Core.Models.Session;

namespace MTS.Core.Interfaces.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetDraftAsync();
    Task SaveDraftAsync(Session session);
    Task DeleteDraftAsync();
    Task SaveCompletedAsync(Session session);
}

using MTS.Core.Models.Session;

namespace MTS.Core.Interfaces.Services;

public interface ISessionStateService
{
    Session? CurrentSession { get; }
    bool IsSessionActive { get; }
    event EventHandler? SessionChanged;
    void SetSession(Session session);
    void UpdateSession(Action<Session> mutate);
    void ClearSession();
}

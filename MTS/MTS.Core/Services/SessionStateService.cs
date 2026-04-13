using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Session;

namespace MTS.Core.Services;

/// <summary>
/// Singleton in-memory store for the currently active session draft.
/// All ViewModels share a single instance via dependency injection.
/// Raises SessionChanged whenever the session is mutated.
/// </summary>
public sealed class SessionStateService : ISessionStateService
{
    private Session? _currentSession;

    public Session? CurrentSession => _currentSession;

    public bool IsSessionActive => _currentSession != null;

    public event EventHandler? SessionChanged;

    public void SetSession(Session session)
    {
        _currentSession = session ?? throw new ArgumentNullException(nameof(session));
        RaiseSessionChanged();
    }

    /// <summary>
    /// Applies a mutation to the current session and raises SessionChanged.
    /// Throws if no session is active.
    /// </summary>
    public void UpdateSession(Action<Session> mutate)
    {
        if (_currentSession == null)
            throw new InvalidOperationException("No active session to update.");

        mutate(_currentSession);
        _currentSession.UpdatedAt = DateTime.UtcNow;
        RaiseSessionChanged();
    }

    public void ClearSession()
    {
        _currentSession = null;
        RaiseSessionChanged();
    }

    private void RaiseSessionChanged()
        => SessionChanged?.Invoke(this, EventArgs.Empty);
}

using MTS.Core.Enums;
using MTS.Core.Interfaces.Repositories;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Session;
using MTS.Core.Rules;

namespace MTS.Core.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepo;
    private readonly IHistoryRepository _historyRepo;
    private readonly ISessionStateService _state;
    private readonly ISettingsService _settings;
    private readonly EvaluationRulesService _rules;

    public SessionService(
        ISessionRepository sessionRepo,
        IHistoryRepository historyRepo,
        ISessionStateService state,
        ISettingsService settings,
        EvaluationRulesService rules)
    {
        _sessionRepo = sessionRepo;
        _historyRepo = historyRepo;
        _state       = state;
        _settings    = settings;
        _rules       = rules;
    }

    public async Task<Session> CreateSessionAsync(CandidateInfo candidate, bool supervisorOnly = false)
    {
        var appSettings = await _settings.LoadAsync();

        var session = new Session
        {
            Candidate        = candidate,
            IsSupervisorOnly = supervisorOnly,
            TesterInfo       = new TesterInfo
            {
                TesterName  = appSettings.TesterProfile.TesterName,
                DisplayName = appSettings.TesterProfile.DisplayName
            }
        };

        await _sessionRepo.SaveDraftAsync(session);
        _state.SetSession(session);
        return session;
    }

    public async Task SaveCallAsync(CallRecord call)
    {
        _state.UpdateSession(s =>
        {
            var existing = s.Calls.FirstOrDefault(c => c.CallNumber == call.CallNumber);
            if (existing != null)
                s.Calls.Remove(existing);
            s.Calls.Add(call);
        });

        await _sessionRepo.SaveDraftAsync(_state.CurrentSession!);
    }

    public async Task SaveSupTransferAsync(SupTransferRecord transfer)
    {
        _state.UpdateSession(s =>
        {
            var existing = s.SupTransfers.FirstOrDefault(t => t.TransferNumber == transfer.TransferNumber);
            if (existing != null)
                s.SupTransfers.Remove(existing);
            s.SupTransfers.Add(transfer);
        });

        await _sessionRepo.SaveDraftAsync(_state.CurrentSession!);
    }

    public async Task SaveNewbieShiftAsync(NewbieShiftRecord shift)
    {
        _state.UpdateSession(s => s.NewbieShift = shift);
        await _sessionRepo.SaveDraftAsync(_state.CurrentSession!);
    }

    public async Task SetAutoFailAsync(AutoFailReason reason)
    {
        _state.UpdateSession(s =>
        {
            s.AutoFailReason = reason;
            s.Status         = SessionStatus.Fail;
        });

        await _sessionRepo.SaveDraftAsync(_state.CurrentSession!);
    }

    public async Task<Session> FinishSessionAsync()
    {
        var session = _state.CurrentSession
            ?? throw new InvalidOperationException("No active session to finish.");

        session.Status    = _rules.ComputeFinalStatus(session);
        session.UpdatedAt = DateTime.UtcNow;

        await _historyRepo.DeleteAllAsync(); // handled separately; just save completed
        await _sessionRepo.SaveCompletedAsync(session);
        await _sessionRepo.DeleteDraftAsync();

        _state.ClearSession();
        return session;
    }

    public async Task DiscardSessionAsync()
    {
        await _sessionRepo.DeleteDraftAsync();
        _state.ClearSession();
    }

    public Task<Session?> GetCurrentSessionAsync()
        => _sessionRepo.GetDraftAsync();
}

using MTS.Core.Enums;
using MTS.Core.Models.Session;

namespace MTS.Core.Interfaces.Services;

public interface ISessionService
{
    Task<Session> CreateSessionAsync(CandidateInfo candidate, bool supervisorOnly = false);
    Task SaveCallAsync(CallRecord call);
    Task SaveSupTransferAsync(SupTransferRecord transfer);
    Task SaveNewbieShiftAsync(NewbieShiftRecord shift);
    Task SetAutoFailAsync(AutoFailReason reason);
    Task<Session> FinishSessionAsync();
    Task DiscardSessionAsync();
    Task<Session?> GetCurrentSessionAsync();
}

using LiteDB;
using MTS.Core.Interfaces.Repositories;
using MTS.Core.Models.History;
using MTS.Core.Models.Session;

namespace MTS.Infrastructure.Persistence.LiteDb;

/// <summary>
/// LiteDB-backed implementation of session and history persistence.
/// Uses a single .db file stored in AppData.
/// </summary>
public class LiteDbSessionRepository : ISessionRepository, IHistoryRepository, IDisposable
{
    private readonly LiteDatabase _db;
    private const string DraftCollection     = "session_draft";
    private const string CompletedCollection = "sessions";

    public LiteDbSessionRepository()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir     = Path.Combine(appData, "MTS");
        Directory.CreateDirectory(dir);
        var dbPath = Path.Combine(dir, "mts.db");
        _db = new LiteDatabase(dbPath);
    }

    // ---- ISessionRepository ----

    public Task<Session?> GetDraftAsync()
    {
        var col   = _db.GetCollection<Session>(DraftCollection);
        var draft = col.FindOne(Query.All());
        return Task.FromResult<Session?>(draft);
    }

    public Task SaveDraftAsync(Session session)
    {
        var col = _db.GetCollection<Session>(DraftCollection);
        col.DeleteAll();
        col.Insert(session);
        return Task.CompletedTask;
    }

    public Task DeleteDraftAsync()
    {
        _db.GetCollection<Session>(DraftCollection).DeleteAll();
        return Task.CompletedTask;
    }

    public Task SaveCompletedAsync(Session session)
    {
        _db.GetCollection<Session>(CompletedCollection).Upsert(session);
        return Task.CompletedTask;
    }

    // ---- IHistoryRepository ----

    public Task<List<SessionSummary>> GetAllSummariesAsync()
    {
        var col       = _db.GetCollection<Session>(CompletedCollection);
        var summaries = col
            .FindAll()
            .OrderByDescending(s => s.CreatedAt)
            .Select(ToSummary)
            .ToList();
        return Task.FromResult(summaries);
    }

    public Task<Session?> GetByIdAsync(Guid id)
    {
        var col     = _db.GetCollection<Session>(CompletedCollection);
        var session = col.FindOne(s => s.Id == id);
        return Task.FromResult<Session?>(session);
    }

    public Task<HistoryStats> GetStatsAsync()
    {
        var col      = _db.GetCollection<Session>(CompletedCollection);
        var sessions = col.FindAll().ToList();

        var stats = new HistoryStats
        {
            TotalSessions  = sessions.Count,
            TotalPass      = sessions.Count(s => s.Status == Core.Enums.SessionStatus.Pass),
            TotalFail      = sessions.Count(s => s.Status == Core.Enums.SessionStatus.Fail),
            TotalIncomplete = sessions.Count(s => s.Status == Core.Enums.SessionStatus.Incomplete)
        };
        return Task.FromResult(stats);
    }

    public Task<List<SessionSummary>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAllSummariesAsync();

        var col = _db.GetCollection<Session>(CompletedCollection);
        var results = col
            .FindAll()
            .Where(s => s.Candidate.CandidateName
                .Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.CreatedAt)
            .Select(ToSummary)
            .ToList();
        return Task.FromResult(results);
    }

    public Task DeleteAllAsync()
    {
        _db.GetCollection<Session>(CompletedCollection).DeleteAll();
        return Task.CompletedTask;
    }

    // ---- Helpers ----

    private static SessionSummary ToSummary(Session s) => new()
    {
        Id              = s.Id,
        CandidateName   = s.Candidate.CandidateName,
        TesterName      = s.TesterInfo.TesterName,
        Status          = s.Status,
        CallsPassed     = s.CallsPassed,
        CallsFailed     = s.CallsFailed,
        SupsPassed      = s.SupsPassed,
        HasNewbieShift  = s.NewbieShift != null,
        IsSupervisorOnly = s.IsSupervisorOnly,
        IsFinalAttempt  = s.Candidate.IsFinalAttempt,
        CreatedAt       = s.CreatedAt
    };

    public void Dispose() => _db.Dispose();
}

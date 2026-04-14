using MTS.Core.Models.Session;

namespace MTS.Core.Interfaces.Services;

public interface ISheetsService
{
    bool IsConfigured { get; }
    Task ExportSessionAsync(Session session);
}

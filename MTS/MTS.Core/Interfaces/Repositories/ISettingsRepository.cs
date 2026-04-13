using MTS.Core.Models.Settings;

namespace MTS.Core.Interfaces.Repositories;

public interface ISettingsRepository
{
    Task<AppSettings?> LoadAsync();
    Task SaveAsync(AppSettings settings);
    Task<bool> ExistsAsync();
}

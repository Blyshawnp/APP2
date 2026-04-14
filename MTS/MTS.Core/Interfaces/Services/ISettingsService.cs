using MTS.Core.Models.Settings;

namespace MTS.Core.Interfaces.Services;

public interface ISettingsService
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
    Task<AppSettings> GetDefaultsAsync();
    Task<bool> IsSetupCompleteAsync();
    Task MarkSetupCompleteAsync();
    Task MarkTutorialCompleteAsync();
}

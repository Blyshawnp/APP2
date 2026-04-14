namespace MTS.Core.Interfaces.Services;

public record UpdateInfo(string Version, string Url, string Notes);

public interface IUpdateService
{
    Task<UpdateInfo?> CheckForUpdateAsync(string currentVersion);
}

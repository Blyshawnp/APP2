using System.Text.Json;
using System.Text.Json.Serialization;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.Settings;

namespace MTS.Infrastructure.Persistence.Json;

/// <summary>
/// Persists AppSettings as a JSON file in the user's AppData folder.
/// Seeds from embedded defaults when the file doesn't exist yet.
/// Implements ISettingsService directly — no separate repository abstraction
/// needed for a single-user desktop app.
/// </summary>
public class JsonSettingsService : ISettingsService
{
    private readonly string _filePath;
    private readonly JsonLookupTableService _lookupService;
    private AppSettings? _cache;

    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented            = true,
        DefaultIgnoreCondition   = JsonIgnoreCondition.Never,
        Converters               = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions _readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters                  = { new JsonStringEnumConverter() }
    };

    public JsonSettingsService(JsonLookupTableService lookupService)
    {
        _lookupService = lookupService;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir     = Path.Combine(appData, "MTS");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "settings.json");
    }

    public async Task<AppSettings> LoadAsync()
    {
        if (_cache != null) return _cache;

        if (!File.Exists(_filePath))
        {
            _cache = _lookupService.BuildDefaultSettings();
            await SaveAsync(_cache);
            return _cache;
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var loaded = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _readOptions);
            _cache = loaded ?? _lookupService.BuildDefaultSettings();
        }
        catch (JsonException)
        {
            // Corrupt file — reset to defaults
            _cache = _lookupService.BuildDefaultSettings();
            await SaveAsync(_cache);
        }

        return _cache;
    }

    public async Task SaveAsync(AppSettings settings)
    {
        _cache = settings;
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, settings, _writeOptions);
    }

    public async Task<AppSettings> GetDefaultsAsync()
        => await Task.FromResult(_lookupService.BuildDefaultSettings());

    public async Task<bool> IsSetupCompleteAsync()
    {
        var s = await LoadAsync();
        return s.SetupComplete;
    }

    public async Task MarkSetupCompleteAsync()
    {
        var s = await LoadAsync();
        s.SetupComplete = true;
        await SaveAsync(s);
    }

    public async Task MarkTutorialCompleteAsync()
    {
        var s = await LoadAsync();
        s.TutorialCompleted = true;
        await SaveAsync(s);
    }
}

using System.Reflection;
using System.Text.Json;
using MTS.Core.Interfaces.Services;
using MTS.Core.Models.System;

namespace MTS.Infrastructure.ExternalServices;

/// <summary>
/// Loads Discord message templates from the embedded DiscordMessages.json.
///
/// These messages are system-owned and immutable at runtime.
/// They are never stored in user AppData, never serialized to settings.json,
/// and never surfaced in the Settings UI.
/// </summary>
public class DiscordMessageService : IDiscordMessageService
{
    private static readonly Assembly _assembly = typeof(DiscordMessageService).Assembly;

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private IReadOnlyList<DiscordMessage>? _cache;

    public async Task<IReadOnlyList<DiscordMessage>> GetAllAsync()
    {
        if (_cache != null) return _cache;

        var resourceName = _assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("DiscordMessages.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
            throw new InvalidOperationException(
                "Embedded resource 'DiscordMessages.json' not found. " +
                "Ensure it is included as EmbeddedResource in MTS.Infrastructure.csproj.");

        await using var stream = _assembly.GetManifestResourceStream(resourceName)!;
        var list = await JsonSerializer.DeserializeAsync<List<DiscordMessage>>(stream, _options);
        _cache = (list ?? new List<DiscordMessage>()).AsReadOnly();
        return _cache;
    }

    public async Task<string?> GetByTriggerAsync(string trigger)
    {
        var all = await GetAllAsync();
        return all
            .FirstOrDefault(m => string.Equals(m.Trigger, trigger, StringComparison.OrdinalIgnoreCase))
            ?.Text;
    }
}

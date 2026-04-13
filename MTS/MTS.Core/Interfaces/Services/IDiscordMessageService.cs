using MTS.Core.Models.System;

namespace MTS.Core.Interfaces.Services;

/// <summary>
/// Provides read-only access to system-owned Discord message templates.
///
/// IMPORTANT: These messages are NOT editable by users and must NOT appear in Settings.
/// The data is embedded in the assembly and never written to user AppData.
/// </summary>
public interface IDiscordMessageService
{
    /// <summary>Returns all Discord message templates in their defined order.</summary>
    Task<IReadOnlyList<DiscordMessage>> GetAllAsync();

    /// <summary>
    /// Returns the message text for the given trigger, or null if not found.
    /// Comparison is case-insensitive.
    /// </summary>
    Task<string?> GetByTriggerAsync(string trigger);
}

namespace MTS.Core.Models.System;

/// <summary>
/// Represents a system-owned Discord message template keyed by a trigger name.
///
/// IMPORTANT: Discord messages are NOT user-editable and do NOT appear in Settings.
/// They are loaded read-only via IDiscordMessageService from an embedded JSON file.
/// Any workflow code that needs a Discord message must inject IDiscordMessageService.
/// </summary>
public class DiscordMessage
{
    /// <summary>
    /// The trigger key used to look up this message (e.g. "Pass", "Fail", "Sup Intro").
    /// </summary>
    public string Trigger { get; set; } = string.Empty;

    /// <summary>
    /// The message body. May contain Discord markdown and \n newlines.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}

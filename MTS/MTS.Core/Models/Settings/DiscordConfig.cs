namespace MTS.Core.Models.Settings;

public class DiscordConfig
{
    public List<DiscordTemplate> Templates { get; set; } = new();
    public List<DiscordScreenshot> Screenshots { get; set; } = new();
}

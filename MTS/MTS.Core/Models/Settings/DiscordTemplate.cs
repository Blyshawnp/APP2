namespace MTS.Core.Models.Settings;

public class DiscordTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Trigger { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

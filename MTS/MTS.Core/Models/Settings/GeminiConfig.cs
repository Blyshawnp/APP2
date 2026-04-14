namespace MTS.Core.Models.Settings;

public class GeminiConfig
{
    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string CoachingPrompt { get; set; } = string.Empty;
    public string FailPrompt { get; set; } = string.Empty;
}

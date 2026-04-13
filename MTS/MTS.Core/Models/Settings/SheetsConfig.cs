namespace MTS.Core.Models.Settings;

public class SheetsConfig
{
    public bool Enabled { get; set; }
    public string SheetId { get; set; } = string.Empty;
    public string Worksheet { get; set; } = string.Empty;
    public string ServiceAccountPath { get; set; } = string.Empty;
}

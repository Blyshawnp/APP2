namespace MTS.Core.Models.Session;

public class CoachingSelection
{
    public Guid CategoryId { get; set; }
    public string CategoryLabel { get; set; } = string.Empty;
    public bool IsOther { get; set; }
    public List<string> SelectedSubItems { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

namespace MTS.Core.Models.Session;

public class FailSelection
{
    public Guid ReasonId { get; set; }
    public string ReasonLabel { get; set; } = string.Empty;
    public bool IsOther { get; set; }
    public string Notes { get; set; } = string.Empty;
}

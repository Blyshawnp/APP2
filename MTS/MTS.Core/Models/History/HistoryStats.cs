namespace MTS.Core.Models.History;

public class HistoryStats
{
    public int TotalSessions { get; set; }
    public int TotalPass { get; set; }
    public int TotalFail { get; set; }
    public int TotalIncomplete { get; set; }
    public int TotalNcNs { get; set; }

    public double PassRate  => TotalSessions == 0 ? 0 : Math.Round((double)TotalPass  / TotalSessions * 100, 1);
    public double NcNsRate  => TotalSessions == 0 ? 0 : Math.Round((double)TotalNcNs  / TotalSessions * 100, 1);
}

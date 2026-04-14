using MTS.Core.Enums;

namespace MTS.Core.Models.Session;

public class NewbieShiftRecord
{
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsAm { get; set; } = true;
    public AppTimezone Timezone { get; set; } = AppTimezone.Eastern;
}

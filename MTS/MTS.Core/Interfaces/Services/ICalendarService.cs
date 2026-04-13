using MTS.Core.Models.Session;

namespace MTS.Core.Interfaces.Services;

public interface ICalendarService
{
    string BuildCalendarUrl(NewbieShiftRecord shift, CandidateInfo candidate);
    void OpenInBrowser(string url);
}

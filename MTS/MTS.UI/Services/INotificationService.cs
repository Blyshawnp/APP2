using MTS.Core.Enums;

namespace MTS.UI.Services;

public interface INotificationService
{
    void ShowSuccess(string message, int durationMs = 3000);
    void ShowError(string message, int durationMs = 5000);
    void ShowWarning(string message, int durationMs = 4000);
    void ShowInfo(string message, int durationMs = 3000);
}

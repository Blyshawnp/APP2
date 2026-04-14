using MTS.Core.Enums;

namespace MTS.UI.Services;

/// <summary>
/// Stub notification service.  In Phase 8 this will be wired to a
/// toast host panel in MainWindow via an ObservableCollection.
/// </summary>
public class NotificationService : INotificationService
{
    public void ShowSuccess(string message, int durationMs = 3000)
        => Log("SUCCESS", message);
    public void ShowError(string message, int durationMs = 5000)
        => Log("ERROR", message);
    public void ShowWarning(string message, int durationMs = 4000)
        => Log("WARNING", message);
    public void ShowInfo(string message, int durationMs = 3000)
        => Log("INFO", message);

    private static void Log(string level, string msg)
        => System.Diagnostics.Debug.WriteLine($"[{level}] {msg}");
}

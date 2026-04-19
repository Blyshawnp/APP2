namespace MTS.UI.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmAsync(string title, string message, string confirmLabel = "OK");
    Task<bool> ShowDangerConfirmAsync(string title, string message, string confirmLabel = "Delete");
    Task ShowAlertAsync(string title, string message);
    Task<T?> ShowPickerAsync<T>(string title, string message, IList<T> items, Func<T, string> labelSelector) where T : class;
}

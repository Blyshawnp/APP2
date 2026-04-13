using System.Windows;

namespace MTS.UI.Services;

public class DialogService : IDialogService
{
    public Task<bool> ShowConfirmAsync(string title, string message, string confirmLabel = "OK")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.OK);
    }

    public Task<bool> ShowDangerConfirmAsync(string title, string message, string confirmLabel = "Delete")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task ShowAlertAsync(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }
}

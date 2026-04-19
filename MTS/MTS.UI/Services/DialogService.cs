using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

    public Task<T?> ShowPickerAsync<T>(string title, string message, IList<T> items, Func<T, string> labelSelector)
        where T : class
    {
        var win = new Window
        {
            Title                 = title,
            Width                 = 480,
            Height                = 380,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner                 = Application.Current.MainWindow,
            Background            = new SolidColorBrush(Color.FromRgb(18, 24, 38)),
            ResizeMode            = ResizeMode.NoResize
        };

        T? selected = null;

        var root   = new DockPanel { Margin = new Thickness(20) };
        var header = new TextBlock
        {
            Text       = message,
            Foreground = Brushes.White,
            FontSize   = 14,
            Margin     = new Thickness(0, 0, 0, 12),
            TextWrapping = TextWrapping.Wrap
        };
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var btnRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0)
        };
        DockPanel.SetDock(btnRow, Dock.Bottom);

        var listBox = new ListBox
        {
            Background        = new SolidColorBrush(Color.FromRgb(26, 34, 52)),
            Foreground        = Brushes.White,
            BorderBrush       = new SolidColorBrush(Color.FromRgb(55, 65, 90)),
            BorderThickness   = new Thickness(1),
            Margin            = new Thickness(0)
        };
        foreach (var item in items)
            listBox.Items.Add(new ListBoxItem { Content = labelSelector(item), Tag = item });

        var btnOk = new Button
        {
            Content    = "Select",
            Width      = 90, Height = 34,
            Margin     = new Thickness(8, 0, 0, 0),
            Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0)
        };
        var btnCancel = new Button
        {
            Content    = "Cancel",
            Width      = 90, Height = 34,
            Background = new SolidColorBrush(Color.FromRgb(55, 65, 90)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0)
        };

        btnOk.Click += (_, _) =>
        {
            if (listBox.SelectedItem is ListBoxItem li)
                selected = (T)li.Tag!;
            win.DialogResult = selected != null;
        };
        btnCancel.Click += (_, _) => win.DialogResult = false;

        btnRow.Children.Add(btnCancel);
        btnRow.Children.Add(btnOk);
        root.Children.Add(btnRow);
        root.Children.Add(listBox);

        win.Content = root;
        win.ShowDialog();

        return Task.FromResult(selected);
    }
}

using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace MTS.UI.Controls.WarningBanner;

/// <summary>
/// Shows a styled amber warning panel with a list of messages.
/// Auto-hides when the Warnings collection is empty.
/// </summary>
public partial class WarningBannerControl : UserControl
{
    public static readonly DependencyProperty WarningsProperty =
        DependencyProperty.Register(
            nameof(Warnings),
            typeof(IEnumerable),
            typeof(WarningBannerControl),
            new PropertyMetadata(null, OnWarningsChanged));

    public IEnumerable? Warnings
    {
        get => (IEnumerable?)GetValue(WarningsProperty);
        set => SetValue(WarningsProperty, value);
    }

    public WarningBannerControl()
        => InitializeComponent();

    private static void OnWarningsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (WarningBannerControl)d;

        if (e.OldValue is INotifyCollectionChanged oldCol)
            oldCol.CollectionChanged -= ctrl.OnCollectionChanged;

        if (e.NewValue is INotifyCollectionChanged newCol)
            newCol.CollectionChanged += ctrl.OnCollectionChanged;

        ctrl.RefreshVisibility();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => RefreshVisibility();

    private void RefreshVisibility()
    {
        bool hasItems = false;
        if (Warnings != null)
            foreach (var _ in Warnings) { hasItems = true; break; }

        BannerRoot.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
    }
}

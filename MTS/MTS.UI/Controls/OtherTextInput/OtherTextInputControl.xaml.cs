using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MTS.UI.Controls.OtherTextInput;

/// <summary>
/// Shows a labelled, multi-line text input that appears only when
/// "Other" is selected. Highlights red when empty and IsRequired is true.
/// Exposes three DPs: IsOtherSelected, Label, Text.
/// </summary>
public partial class OtherTextInputControl : UserControl
{
    // ---- IsOtherSelected DP ----
    public static readonly DependencyProperty IsOtherSelectedProperty =
        DependencyProperty.Register(
            nameof(IsOtherSelected), typeof(bool), typeof(OtherTextInputControl),
            new PropertyMetadata(false, OnVisibilityChanged));

    public bool IsOtherSelected
    {
        get => (bool)GetValue(IsOtherSelectedProperty);
        set => SetValue(IsOtherSelectedProperty, value);
    }

    // ---- Label DP ----
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(OtherTextInputControl),
            new PropertyMetadata("Notes (required):"));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // ---- Text DP (two-way) ----
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(OtherTextInputControl),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // ---- HasError DP ----
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(
            nameof(HasError), typeof(bool), typeof(OtherTextInputControl),
            new PropertyMetadata(false, OnErrorChanged));

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    private static readonly SolidColorBrush ErrorBrush  = new(Color.FromRgb(239, 68, 68));
    private static readonly SolidColorBrush NormalBrush = new(Color.FromRgb(52, 52, 74));

    public OtherTextInputControl()
        => InitializeComponent();

    private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (OtherTextInputControl)d;
        ctrl.Container.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((OtherTextInputControl)d).RefreshError();

    private static void OnErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((OtherTextInputControl)d).RefreshError();

    private void RefreshError()
    {
        bool showError = HasError && IsOtherSelected && string.IsNullOrWhiteSpace(Text);
        InputBorder.BorderBrush       = showError ? ErrorBrush : NormalBrush;
        InputBorder.BorderThickness   = new Thickness(showError ? 1 : 0);
        RequiredHint.Visibility       = showError ? Visibility.Visible : Visibility.Collapsed;
    }
}

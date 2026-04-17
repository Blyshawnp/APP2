using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MTS.Core.Enums;

namespace MTS.UI.Controls.PassFailToggle;

/// <summary>
/// A two-state toggle control for Pass / Fail selection.
/// Exposes a two-way bindable SelectedResult dependency property.
/// Visual state (colors) is updated in code-behind — no business logic.
/// </summary>
public partial class PassFailToggleControl : UserControl
{
    // -------------------------------------------------------------------------
    // Dependency property
    // -------------------------------------------------------------------------

    public static readonly DependencyProperty SelectedResultProperty =
        DependencyProperty.Register(
            nameof(SelectedResult),
            typeof(CallResult?),
            typeof(PassFailToggleControl),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedResultChanged));

    public CallResult? SelectedResult
    {
        get => (CallResult?)GetValue(SelectedResultProperty);
        set => SetValue(SelectedResultProperty, value);
    }

    // Commands exposed to XAML (element-name binding)
    public ICommand SelectPassCommand { get; }
    public ICommand SelectFailCommand { get; }

    // -------------------------------------------------------------------------
    // Brushes
    // -------------------------------------------------------------------------
    private static readonly SolidColorBrush PassActiveBg    = new(Color.FromRgb(20, 39, 30));
    private static readonly SolidColorBrush PassActiveBorder = new(Color.FromRgb(34, 197, 94));
    private static readonly SolidColorBrush PassActiveFg    = new(Color.FromRgb(34, 197, 94));
    private static readonly SolidColorBrush FailActiveBg    = new(Color.FromRgb(42, 21, 21));
    private static readonly SolidColorBrush FailActiveBorder = new(Color.FromRgb(239, 68, 68));
    private static readonly SolidColorBrush FailActiveFg    = new(Color.FromRgb(239, 68, 68));

    // -------------------------------------------------------------------------

    public PassFailToggleControl()
    {
        InitializeComponent();
        SelectPassCommand = new RelayCommand(() => SelectedResult = CallResult.Pass);
        SelectFailCommand = new RelayCommand(() => SelectedResult = CallResult.Fail);
        UpdateVisualState(null);
    }

    private static void OnSelectedResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((PassFailToggleControl)d).UpdateVisualState((CallResult?)e.NewValue);

    private void UpdateVisualState(CallResult? result)
    {
        var neutralBg     = (SolidColorBrush)FindResource("SurfaceElevatedBrush");
        var neutralBorder  = (SolidColorBrush)FindResource("BorderBrush");
        var neutralFg      = (SolidColorBrush)FindResource("TextMutedBrush");

        // Reset both
        PassBorder.Background    = neutralBg;
        PassBorder.BorderBrush   = neutralBorder;
        PassLabel.Foreground     = neutralFg;
        FailBorder.Background    = neutralBg;
        FailBorder.BorderBrush   = neutralBorder;
        FailLabel.Foreground     = neutralFg;

        switch (result)
        {
            case CallResult.Pass:
                PassBorder.Background  = PassActiveBg;
                PassBorder.BorderBrush = PassActiveBorder;
                PassLabel.Foreground   = PassActiveFg;
                break;
            case CallResult.Fail:
                FailBorder.Background  = FailActiveBg;
                FailBorder.BorderBrush = FailActiveBorder;
                FailLabel.Foreground   = FailActiveFg;
                break;
        }
    }

    // Simple inline relay command (avoids CommunityToolkit ref in code-behind)
    private sealed class RelayCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
    }
}

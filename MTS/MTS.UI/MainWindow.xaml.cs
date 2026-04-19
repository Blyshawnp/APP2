using System.Windows;
using System.Windows.Media.Animation;
using MTS.UI.ViewModels;

namespace MTS.UI;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel  = viewModel;
        DataContext = _viewModel;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        _viewModel.Initialize();
    }

    // -------------------------------------------------------------------------
    // Ticker marquee animation
    // -------------------------------------------------------------------------

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        StartTickerAnimation();
    }

    private void StartTickerAnimation()
    {
        // Measure the text so we know how far to scroll
        TickerText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double textWidth   = TickerText.DesiredSize.Width;
        double canvasWidth = TickerCanvas.ActualWidth;

        // Scroll from right edge → left (off screen)
        var anim = new DoubleAnimation
        {
            From           = canvasWidth,
            To             = -textWidth,
            Duration       = new Duration(TimeSpan.FromSeconds(Math.Max(25, (textWidth + canvasWidth) / 60))),
            RepeatBehavior = RepeatBehavior.Forever
        };

        TickerText.BeginAnimation(System.Windows.Controls.Canvas.LeftProperty, anim);
    }
}

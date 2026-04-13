using System.Windows;
using MTS.UI.ViewModels;

namespace MTS.UI;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        // Trigger initial navigation after the window is fully rendered
        _viewModel.Initialize();
    }
}

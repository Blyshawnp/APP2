using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MTS.UI.Controls.FailReasonsGrid;

public partial class FailReasonsGrid : UserControl
{
    // ---- Items DP ----
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(
            nameof(Items), typeof(IEnumerable), typeof(FailReasonsGrid),
            new PropertyMetadata(null));

    public IEnumerable? Items
    {
        get => (IEnumerable?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    // ---- ShowRequiredError DP ----
    public static readonly DependencyProperty ShowRequiredErrorProperty =
        DependencyProperty.Register(
            nameof(ShowRequiredError), typeof(bool), typeof(FailReasonsGrid),
            new PropertyMetadata(false));

    public bool ShowRequiredError
    {
        get => (bool)GetValue(ShowRequiredErrorProperty);
        set => SetValue(ShowRequiredErrorProperty, value);
    }

    public FailReasonsGrid()
        => InitializeComponent();
}

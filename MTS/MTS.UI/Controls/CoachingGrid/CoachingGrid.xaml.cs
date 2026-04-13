using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MTS.UI.Controls.CoachingGrid;

/// <summary>
/// Data-driven coaching checklist. Receives CoachingItemViewModels via the Items DP.
/// The ShowCoachingWarning DP toggles the soft "no coaching selected" hint.
/// All business logic (what the items mean) lives in the parent ViewModel.
/// </summary>
public partial class CoachingGrid : UserControl
{
    // ---- Items DP ----
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(
            nameof(Items), typeof(IEnumerable), typeof(CoachingGrid),
            new PropertyMetadata(null));

    public IEnumerable? Items
    {
        get => (IEnumerable?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    // ---- ShowCoachingWarning DP ----
    public static readonly DependencyProperty ShowCoachingWarningProperty =
        DependencyProperty.Register(
            nameof(ShowCoachingWarning), typeof(bool), typeof(CoachingGrid),
            new PropertyMetadata(false));

    public bool ShowCoachingWarning
    {
        get => (bool)GetValue(ShowCoachingWarningProperty);
        set => SetValue(ShowCoachingWarningProperty, value);
    }

    public CoachingGrid()
        => InitializeComponent();
}

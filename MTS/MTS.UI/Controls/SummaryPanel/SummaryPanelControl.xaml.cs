using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MTS.UI.Controls.SummaryPanel;

/// <summary>
/// Reusable panel that displays and edits a single AI-generated summary.
/// Exposes:
///   – SummaryText (string, two-way) — the editable text content
///   – Title (string) — section header label
///   – RegenerateLabel (string) — button text
///   – RegenerateCommand (ICommand) — wired to ReviewViewModel
///   – IsGenerating (bool) — shows spinner, hides text area
///   – IsAiEnabled (bool) — controls visibility of AI badge + Regenerate button
///   – EmptyHint (string) — placeholder shown when summary is empty
///   – HasSummary (bool, read-only) — derived from SummaryText
/// </summary>
public partial class SummaryPanelControl : UserControl
{
    // ---- SummaryText DP ----
    public static readonly DependencyProperty SummaryTextProperty =
        DependencyProperty.Register(
            nameof(SummaryText), typeof(string), typeof(SummaryPanelControl),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, _) => ((SummaryPanelControl)d).OnSummaryTextChanged()));

    public string SummaryText
    {
        get => (string)GetValue(SummaryTextProperty);
        set => SetValue(SummaryTextProperty, value);
    }

    // ---- Title DP ----
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(SummaryPanelControl),
            new PropertyMetadata("Summary"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ---- RegenerateLabel DP ----
    public static readonly DependencyProperty RegenerateLabelProperty =
        DependencyProperty.Register(
            nameof(RegenerateLabel), typeof(string), typeof(SummaryPanelControl),
            new PropertyMetadata("Regenerate"));

    public string RegenerateLabel
    {
        get => (string)GetValue(RegenerateLabelProperty);
        set => SetValue(RegenerateLabelProperty, value);
    }

    // ---- RegenerateCommand DP ----
    public static readonly DependencyProperty RegenerateCommandProperty =
        DependencyProperty.Register(
            nameof(RegenerateCommand), typeof(ICommand), typeof(SummaryPanelControl),
            new PropertyMetadata(null));

    public ICommand? RegenerateCommand
    {
        get => (ICommand?)GetValue(RegenerateCommandProperty);
        set => SetValue(RegenerateCommandProperty, value);
    }

    // ---- IsGenerating DP ----
    public static readonly DependencyProperty IsGeneratingProperty =
        DependencyProperty.Register(
            nameof(IsGenerating), typeof(bool), typeof(SummaryPanelControl),
            new PropertyMetadata(false));

    public bool IsGenerating
    {
        get => (bool)GetValue(IsGeneratingProperty);
        set => SetValue(IsGeneratingProperty, value);
    }

    // ---- IsAiEnabled DP ----
    public static readonly DependencyProperty IsAiEnabledProperty =
        DependencyProperty.Register(
            nameof(IsAiEnabled), typeof(bool), typeof(SummaryPanelControl),
            new PropertyMetadata(false));

    public bool IsAiEnabled
    {
        get => (bool)GetValue(IsAiEnabledProperty);
        set => SetValue(IsAiEnabledProperty, value);
    }

    // ---- EmptyHint DP ----
    public static readonly DependencyProperty EmptyHintProperty =
        DependencyProperty.Register(
            nameof(EmptyHint), typeof(string), typeof(SummaryPanelControl),
            new PropertyMetadata("No summary generated. Type manually or use Regenerate."));

    public string EmptyHint
    {
        get => (string)GetValue(EmptyHintProperty);
        set => SetValue(EmptyHintProperty, value);
    }

    // ---- HasSummary (read-only) ----
    private static readonly DependencyPropertyKey HasSummaryPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasSummary), typeof(bool), typeof(SummaryPanelControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasSummaryProperty = HasSummaryPropertyKey.DependencyProperty;

    public bool HasSummary => (bool)GetValue(HasSummaryProperty);

    // -------------------------------------------------------------------------

    public SummaryPanelControl()
        => InitializeComponent();

    private void OnSummaryTextChanged()
        => SetValue(HasSummaryPropertyKey, !string.IsNullOrWhiteSpace(SummaryText));
}

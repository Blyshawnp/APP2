using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MTS.UI.Controls.LookupTableEditor;

/// <summary>
/// Generic list-and-detail editor control.
///
/// Left panel: ListBox showing DisplayLabel for each item, with Add / Remove / ↑ / ↓ buttons.
/// Right panel: ContentControl rendering the selected item via the caller-supplied DetailTemplate.
///
/// Dependency properties:
///   Items          – IEnumerable bound to a section's ObservableCollection.
///   SelectedItem   – Two-way selected item; drives both the ListBox and the right-side form.
///   HasSelection   – Read-only bool DP updated automatically when SelectedItem changes.
///   AddCommand     – Triggered by the Add button (no parameter).
///   RemoveCommand  – Triggered by Remove (CommandParameter = SelectedItem).
///   MoveUpCommand  – Triggered by ↑ (CommandParameter = SelectedItem).
///   MoveDownCommand– Triggered by ↓ (CommandParameter = SelectedItem).
///   DetailTemplate – DataTemplate rendered by the right-side ContentControl.
/// </summary>
public partial class LookupTableEditorControl : UserControl
{
    // ---- Items ----
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IEnumerable),
            typeof(LookupTableEditorControl), new PropertyMetadata(null));

    public IEnumerable? Items
    {
        get => (IEnumerable?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    // ---- SelectedItem (two-way by default) ----
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object),
            typeof(LookupTableEditorControl),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((LookupTableEditorControl)d).HasSelection = e.NewValue is not null;

    // ---- HasSelection (read-only DP) ----
    private static readonly DependencyPropertyKey HasSelectionPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasSelection), typeof(bool),
            typeof(LookupTableEditorControl), new PropertyMetadata(false));

    public static readonly DependencyProperty HasSelectionProperty =
        HasSelectionPropertyKey.DependencyProperty;

    public bool HasSelection
    {
        get => (bool)GetValue(HasSelectionProperty);
        private set => SetValue(HasSelectionPropertyKey, value);
    }

    // ---- AddCommand ----
    public static readonly DependencyProperty AddCommandProperty =
        DependencyProperty.Register(nameof(AddCommand), typeof(ICommand),
            typeof(LookupTableEditorControl), new PropertyMetadata(null));

    public ICommand? AddCommand
    {
        get => (ICommand?)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    // ---- RemoveCommand ----
    public static readonly DependencyProperty RemoveCommandProperty =
        DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand),
            typeof(LookupTableEditorControl), new PropertyMetadata(null));

    public ICommand? RemoveCommand
    {
        get => (ICommand?)GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    // ---- MoveUpCommand ----
    public static readonly DependencyProperty MoveUpCommandProperty =
        DependencyProperty.Register(nameof(MoveUpCommand), typeof(ICommand),
            typeof(LookupTableEditorControl), new PropertyMetadata(null));

    public ICommand? MoveUpCommand
    {
        get => (ICommand?)GetValue(MoveUpCommandProperty);
        set => SetValue(MoveUpCommandProperty, value);
    }

    // ---- MoveDownCommand ----
    public static readonly DependencyProperty MoveDownCommandProperty =
        DependencyProperty.Register(nameof(MoveDownCommand), typeof(ICommand),
            typeof(LookupTableEditorControl), new PropertyMetadata(null));

    public ICommand? MoveDownCommand
    {
        get => (ICommand?)GetValue(MoveDownCommandProperty);
        set => SetValue(MoveDownCommandProperty, value);
    }

    // ---- DetailTemplate ----
    public static readonly DependencyProperty DetailTemplateProperty =
        DependencyProperty.Register(nameof(DetailTemplate), typeof(DataTemplate),
            typeof(LookupTableEditorControl), new PropertyMetadata(null));

    public DataTemplate? DetailTemplate
    {
        get => (DataTemplate?)GetValue(DetailTemplateProperty);
        set => SetValue(DetailTemplateProperty, value);
    }

    public LookupTableEditorControl()
    {
        InitializeComponent();
    }
}

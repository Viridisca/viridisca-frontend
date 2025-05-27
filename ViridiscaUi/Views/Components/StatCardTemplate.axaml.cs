using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ViridiscaUi.Views.Components;

/// <summary>
/// Reusable statistics card template for dashboard views
/// </summary>
public partial class StatCardTemplate : UserControl
{
    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<StatCardTemplate, string>(nameof(Value), "0");

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<StatCardTemplate, string>(nameof(Label), "Статистика");

    public static readonly StyledProperty<string> IconDataProperty =
        AvaloniaProperty.Register<StatCardTemplate, string>(nameof(IconData), "");

    public static readonly StyledProperty<IBrush> ValueColorProperty =
        AvaloniaProperty.Register<StatCardTemplate, IBrush>(nameof(ValueColor), Brushes.Blue);

    public static readonly StyledProperty<string> ChangeTextProperty =
        AvaloniaProperty.Register<StatCardTemplate, string>(nameof(ChangeText), "");

    public static readonly StyledProperty<bool> IsPositiveChangeProperty =
        AvaloniaProperty.Register<StatCardTemplate, bool>(nameof(IsPositiveChange), false);

    public static readonly StyledProperty<bool> IsNegativeChangeProperty =
        AvaloniaProperty.Register<StatCardTemplate, bool>(nameof(IsNegativeChange), false);

    public static readonly StyledProperty<bool> IsNeutralChangeProperty =
        AvaloniaProperty.Register<StatCardTemplate, bool>(nameof(IsNeutralChange), false);

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string IconData
    {
        get => GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    public IBrush ValueColor
    {
        get => GetValue(ValueColorProperty);
        set => SetValue(ValueColorProperty, value);
    }

    public string ChangeText
    {
        get => GetValue(ChangeTextProperty);
        set => SetValue(ChangeTextProperty, value);
    }

    public bool IsPositiveChange
    {
        get => GetValue(IsPositiveChangeProperty);
        set => SetValue(IsPositiveChangeProperty, value);
    }

    public bool IsNegativeChange
    {
        get => GetValue(IsNegativeChangeProperty);
        set => SetValue(IsNegativeChangeProperty, value);
    }

    public bool IsNeutralChange
    {
        get => GetValue(IsNeutralChangeProperty);
        set => SetValue(IsNeutralChangeProperty, value);
    }

    public StatCardTemplate()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 
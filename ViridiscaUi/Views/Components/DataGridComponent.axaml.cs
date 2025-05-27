using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections;

namespace ViridiscaUi.Views.Components;

/// <summary>
/// Data Grid Component
/// Provides comprehensive data grid functionality with search, pagination, and modern styling
/// </summary>
public partial class DataGridComponent : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<DataGridComponent, string>(nameof(Title), "Данные");

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<DataGridComponent, string>(nameof(Description), "");

    public static readonly StyledProperty<IEnumerable> ItemsProperty =
        AvaloniaProperty.Register<DataGridComponent, IEnumerable>(nameof(Items));

    public static readonly StyledProperty<object> SelectedItemProperty =
        AvaloniaProperty.Register<DataGridComponent, object>(nameof(SelectedItem));

    public static readonly StyledProperty<string> SearchTextProperty =
        AvaloniaProperty.Register<DataGridComponent, string>(nameof(SearchText), "");

    public static readonly StyledProperty<string> SearchWatermarkProperty =
        AvaloniaProperty.Register<DataGridComponent, string>(nameof(SearchWatermark), "Поиск...");

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<DataGridComponent, bool>(nameof(IsLoading), false);

    public static readonly StyledProperty<bool> IsEmptyProperty =
        AvaloniaProperty.Register<DataGridComponent, bool>(nameof(IsEmpty), false);

    public static readonly StyledProperty<string> EmptyStateMessageProperty =
        AvaloniaProperty.Register<DataGridComponent, string>(nameof(EmptyStateMessage), "Нет данных для отображения");

    public static readonly StyledProperty<bool> ShowAddButtonProperty =
        AvaloniaProperty.Register<DataGridComponent, bool>(nameof(ShowAddButton), true);

    public static readonly StyledProperty<bool> ShowRefreshButtonProperty =
        AvaloniaProperty.Register<DataGridComponent, bool>(nameof(ShowRefreshButton), true);

    public static readonly StyledProperty<bool> ShowExportButtonProperty =
        AvaloniaProperty.Register<DataGridComponent, bool>(nameof(ShowExportButton), true);

    public static readonly StyledProperty<bool> ShowPaginationProperty =
        AvaloniaProperty.Register<DataGridComponent, bool>(nameof(ShowPagination), true);

    public static readonly StyledProperty<bool> ShowFilterOptionsProperty =
        AvaloniaProperty.Register<DataGridComponent, bool>(nameof(ShowFilterOptions), false);

    public static readonly StyledProperty<string> AddButtonTextProperty =
        AvaloniaProperty.Register<DataGridComponent, string>(nameof(AddButtonText), "+ Добавить");

    public static readonly StyledProperty<string> StatusTextProperty =
        AvaloniaProperty.Register<DataGridComponent, string>(nameof(StatusText), "Готово");

    public static readonly StyledProperty<int> TotalRecordsProperty =
        AvaloniaProperty.Register<DataGridComponent, int>(nameof(TotalRecords), 0);

    public static readonly StyledProperty<int> StartRecordProperty =
        AvaloniaProperty.Register<DataGridComponent, int>(nameof(StartRecord), 0);

    public static readonly StyledProperty<int> EndRecordProperty =
        AvaloniaProperty.Register<DataGridComponent, int>(nameof(EndRecord), 0);

    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<DataGridComponent, int>(nameof(PageSize), 25);

    public static readonly StyledProperty<bool> CanGoToPreviousPageProperty =
        AvaloniaProperty.Register<DataGridComponent, bool>(nameof(CanGoToPreviousPage), false);

    public static readonly StyledProperty<bool> CanGoToNextPageProperty =
        AvaloniaProperty.Register<DataGridComponent, bool>(nameof(CanGoToNextPage), false);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IEnumerable Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public string SearchWatermark
    {
        get => GetValue(SearchWatermarkProperty);
        set => SetValue(SearchWatermarkProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsEmpty
    {
        get => GetValue(IsEmptyProperty);
        set => SetValue(IsEmptyProperty, value);
    }

    public string EmptyStateMessage
    {
        get => GetValue(EmptyStateMessageProperty);
        set => SetValue(EmptyStateMessageProperty, value);
    }

    public bool ShowAddButton
    {
        get => GetValue(ShowAddButtonProperty);
        set => SetValue(ShowAddButtonProperty, value);
    }

    public bool ShowRefreshButton
    {
        get => GetValue(ShowRefreshButtonProperty);
        set => SetValue(ShowRefreshButtonProperty, value);
    }

    public bool ShowExportButton
    {
        get => GetValue(ShowExportButtonProperty);
        set => SetValue(ShowExportButtonProperty, value);
    }

    public bool ShowPagination
    {
        get => GetValue(ShowPaginationProperty);
        set => SetValue(ShowPaginationProperty, value);
    }

    public bool ShowFilterOptions
    {
        get => GetValue(ShowFilterOptionsProperty);
        set => SetValue(ShowFilterOptionsProperty, value);
    }

    public string AddButtonText
    {
        get => GetValue(AddButtonTextProperty);
        set => SetValue(AddButtonTextProperty, value);
    }

    public string StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public int TotalRecords
    {
        get => GetValue(TotalRecordsProperty);
        set => SetValue(TotalRecordsProperty, value);
    }

    public int StartRecord
    {
        get => GetValue(StartRecordProperty);
        set => SetValue(StartRecordProperty, value);
    }

    public int EndRecord
    {
        get => GetValue(EndRecordProperty);
        set => SetValue(EndRecordProperty, value);
    }

    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public bool CanGoToPreviousPage
    {
        get => GetValue(CanGoToPreviousPageProperty);
        set => SetValue(CanGoToPreviousPageProperty, value);
    }

    public bool CanGoToNextPage
    {
        get => GetValue(CanGoToNextPageProperty);
        set => SetValue(CanGoToNextPageProperty, value);
    }

    public DataGridComponent()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 
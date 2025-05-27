using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace ViridiscaUi.Views.Components;

/// <summary>
/// Navigation Menu Component
/// Provides comprehensive navigation menu with role-based access control
/// </summary>
public partial class NavigationMenuComponent : UserControl
{
    public static readonly StyledProperty<string> ApplicationTitleProperty =
        AvaloniaProperty.Register<NavigationMenuComponent, string>(nameof(ApplicationTitle), "ViridiscaUi LMS");

    public static readonly StyledProperty<string> UserRoleProperty =
        AvaloniaProperty.Register<NavigationMenuComponent, string>(nameof(UserRole), "Система управления обучением");

    public static readonly StyledProperty<bool> ShowAcademicSectionProperty =
        AvaloniaProperty.Register<NavigationMenuComponent, bool>(nameof(ShowAcademicSection), true);

    public static readonly StyledProperty<bool> ShowPeopleSectionProperty =
        AvaloniaProperty.Register<NavigationMenuComponent, bool>(nameof(ShowPeopleSection), true);

    public static readonly StyledProperty<bool> ShowAdministrationSectionProperty =
        AvaloniaProperty.Register<NavigationMenuComponent, bool>(nameof(ShowAdministrationSection), false);

    public static readonly StyledProperty<bool> ShowSupportServicesSectionProperty =
        AvaloniaProperty.Register<NavigationMenuComponent, bool>(nameof(ShowSupportServicesSection), false);

    public static readonly StyledProperty<bool> ShowParentsSectionProperty =
        AvaloniaProperty.Register<NavigationMenuComponent, bool>(nameof(ShowParentsSection), false);

    public static readonly StyledProperty<string> CurrentPageProperty =
        AvaloniaProperty.Register<NavigationMenuComponent, string>(nameof(CurrentPage), "Dashboard");

    public string ApplicationTitle
    {
        get => GetValue(ApplicationTitleProperty);
        set => SetValue(ApplicationTitleProperty, value);
    }

    public string UserRole
    {
        get => GetValue(UserRoleProperty);
        set => SetValue(UserRoleProperty, value);
    }

    public bool ShowAcademicSection
    {
        get => GetValue(ShowAcademicSectionProperty);
        set => SetValue(ShowAcademicSectionProperty, value);
    }

    public bool ShowPeopleSection
    {
        get => GetValue(ShowPeopleSectionProperty);
        set => SetValue(ShowPeopleSectionProperty, value);
    }

    public bool ShowAdministrationSection
    {
        get => GetValue(ShowAdministrationSectionProperty);
        set => SetValue(ShowAdministrationSectionProperty, value);
    }

    public bool ShowSupportServicesSection
    {
        get => GetValue(ShowSupportServicesSectionProperty);
        set => SetValue(ShowSupportServicesSectionProperty, value);
    }

    public bool ShowParentsSection
    {
        get => GetValue(ShowParentsSectionProperty);
        set => SetValue(ShowParentsSectionProperty, value);
    }

    public string CurrentPage
    {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public NavigationMenuComponent()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 
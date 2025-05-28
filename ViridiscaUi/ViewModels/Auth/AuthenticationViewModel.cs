using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.ViewModels.Bases.Navigations;

namespace ViridiscaUi.ViewModels.Auth;

/// <summary>
/// ViewModel для объединенного представления авторизации (вход + регистрация)
/// </summary>
[Route("auth", DisplayName = "Авторизация", IconKey = "Lock", Order = 0, ShowInMenu = false, Description = "Страница входа и регистрации в системе")]
public class AuthenticationViewModel : RoutableViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IUnifiedNavigationService _navigationService;
    private readonly IRoleService _roleService;

    /// <summary>
    /// Режим регистрации (false = вход, true = регистрация)
    /// </summary>
    [Reactive]
    public bool IsRegistrationMode { get; set; }

    /// <summary>
    /// Имя пользователя (логин)
    /// </summary>
    [Reactive]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email (для регистрации)
    /// </summary>
    [Reactive]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя (для регистрации)
    /// </summary>
    [Reactive]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия пользователя (для регистрации)
    /// </summary>
    [Reactive]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Reactive]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение пароля (для регистрации)
    /// </summary>
    [Reactive]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Выбранная роль пользователя (для регистрации)
    /// </summary>
    [Reactive]
    public Role? SelectedRole { get; set; }

    /// <summary>
    /// Доступные роли для регистрации
    /// </summary>
    [Reactive]
    public ObservableCollection<Role> AvailableRoles { get; set; } = new();

    /// <summary>
    /// Флаг, указывающий на процесс обработки
    /// </summary>
    [Reactive]
    public bool IsProcessing { get; set; } = false;

    /// <summary>
    /// Флаг загрузки ролей
    /// </summary>
    [Reactive]
    public bool IsLoadingRoles { get; set; } = false;

    /// <summary>
    /// Текст кнопки действия
    /// </summary>
    [ObservableAsProperty]
    public string? ActionButtonText { get; }

    /// <summary>
    /// Команда основного действия (вход/регистрация)
    /// </summary>
    public ReactiveCommand<Unit, Unit> ActionCommand { get; private set; }

    /// <summary>
    /// Команда переключения на режим входа
    /// </summary>
    public ReactiveCommand<Unit, Unit> SwitchToLoginCommand { get; private set; }

    /// <summary>
    /// Команда переключения на режим регистрации
    /// </summary>
    public ReactiveCommand<Unit, Unit> SwitchToRegisterCommand { get; private set; }

    /// <summary>
    /// Создает новый экземпляр ViewModel для авторизации
    /// </summary>
    public AuthenticationViewModel(IAuthService authService, IUnifiedNavigationService navigationService, IRoleService roleService, IScreen hostScreen)
        : base(hostScreen)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

        InitializeCommands();
        SetupSubscriptions();

        LogInfo("AuthenticationViewModel initialized");
    }

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Команды переключения режимов
        SwitchToLoginCommand = CreateSyncCommand(() =>
        {
            IsRegistrationMode = false;
            ClearErrors();
        }, null, "Ошибка переключения на режим входа");

        SwitchToRegisterCommand = CreateSyncCommand(() =>
        {
            IsRegistrationMode = true;
            ClearErrors();
            // Загружаем роли при переключении на режим регистрации
            _ = LoadRolesAsync();
        }, null, "Ошибка переключения на режим регистрации");

        // Проверка возможности выполнения команды действия
        var canExecuteAction = this.WhenAnyValue(
            x => x.Username,
            x => x.Password,
            x => x.IsRegistrationMode,
            x => x.Email,
            x => x.FirstName,
            x => x.LastName,
            x => x.ConfirmPassword,
            x => x.SelectedRole,
            x => x.IsProcessing,
            (username, password, isReg, email, firstName, lastName, confirmPassword, selectedRole, isProcessing) =>
            {
                if (isProcessing || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return false;

                if (isReg)
                {
                    return !string.IsNullOrWhiteSpace(email) &&
                           !string.IsNullOrWhiteSpace(firstName) &&
                           !string.IsNullOrWhiteSpace(lastName) &&
                           password == confirmPassword &&
                           selectedRole != null &&
                           password.Length >= 6;
                }

                return true;
            });

        // Используем стандартизированные методы создания команд из ViewModelBase
        ActionCommand = CreateCommand(ExecuteActionAsync, canExecuteAction, "Ошибка выполнения действия");
    }

    /// <summary>
    /// Настраивает подписки на изменения свойств
    /// </summary>
    private void SetupSubscriptions()
    {
        // Динамический текст кнопки действия
        this.WhenAnyValue(x => x.IsRegistrationMode)
            .Select(isReg => isReg ? "Зарегистрироваться" : "Войти")
            .ToPropertyEx(this, x => x.ActionButtonText);
    }

    /// <summary>
    /// Загружает список доступных ролей
    /// </summary>
    private async Task LoadRolesAsync()
    {
        try
        {
            IsLoadingRoles = true;
            ShowInfo("Загрузка ролей...");

            var roles = await _roleService.GetAllRolesAsync();
            LogInfo("Получено ролей: {RoleCount}", roles.Count());

            AvailableRoles.Clear();
            foreach (var role in roles)
            {
                AvailableRoles.Add(role);
                LogDebug("Добавлена роль: {RoleName}", role.Name);
            }

            // Устанавливаем роль студента по умолчанию
            SelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == "Student");
            LogInfo("Пользователь по умолчанию: {DefaultRole}", SelectedRole?.Name ?? "null");
            ShowSuccess("Роли загружены успешно");
        }
        catch (Exception ex)
        {
            var errorMessage = $"Ошибка загрузки ролей: {ex.Message}";
            SetError(errorMessage, ex);
        }
        finally
        {
            IsLoadingRoles = false;
            LogDebug("Загрузка ролей завершена");
        }
    }

    /// <summary>
    /// Выполняет основное действие (вход или регистрация)
    /// </summary>
    private async Task ExecuteActionAsync()
    {
        try
        {
            IsProcessing = true;
            ClearError();

            if (IsRegistrationMode)
            {
                await ExecuteRegistrationAsync();
            }
            else
            {
                await ExecuteLoginAsync();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Ошибка: {ex.Message}";
            SetError(errorMessage, ex);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Выполняет вход в систему
    /// </summary>
    private async Task ExecuteLoginAsync()
    {
        LogInfo("Attempting login for user: {Username}", Username);
        ShowInfo("Выполняется вход в систему...");

        var result = await _authService.AuthenticateAsync(Username, Password);

        if (result.Success)
        {
            ShowSuccess($"Добро пожаловать, {result.User?.Email}!");
            LogInfo("Login successful for user: {UserEmail}", result.User?.Email);
            // Навигация теперь проходит через MainViewModel при изменении CurrentUserObservable
        }
        else
        {
            var errorMessage = result.ErrorMessage ?? "Неверное имя пользователя или пароль";
            SetError(errorMessage);
            ShowWarning(errorMessage);
            LogWarning("Login failed for user {Username}: {ErrorMessage}", Username, errorMessage);
        }
    }

    /// <summary>
    /// Выполняет регистрацию нового пользователя
    /// </summary>
    private async Task ExecuteRegistrationAsync()
    {
        if (SelectedRole == null)
        {
            SetError("Необходимо выбрать роль");
            return;
        }

        LogInfo("Attempting registration for user: {Username}", Username);
        ShowInfo("Выполняется регистрация...");

        var result = await _authService.RegisterAsync(Username, Email, Password, FirstName, LastName, SelectedRole.Uid);

        if (result.Success)
        {
            ShowSuccess("Регистрация прошла успешно! Теперь вы можете войти в систему.");
            LogInfo("Registration successful for user: {UserEmail}", result.User?.Email);
            IsRegistrationMode = false; // Переключаемся на режим входа
            ClearForm();
        }
        else
        {
            var errorMessage = result.ErrorMessage ?? "Ошибка при регистрации";
            SetError(errorMessage);
            ShowWarning(errorMessage);
            LogWarning("Registration failed for user {Username}: {ErrorMessage}", Username, errorMessage);
        }
    }

    /// <summary>
    /// Очищает форму
    /// </summary>
    private void ClearForm()
    {
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        // Остаемся с пустыми полями для удобства пользователя
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.ViewModels.Bases.Navigations;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для управления предметами
    /// </summary>
    [Route("subjects", 
        DisplayName = "Предметы", 
        IconKey = "BookMultiple", 
        Order = 5,
        Group = "Образование",
        ShowInMenu = true,
        Description = "Управление учебными предметами")]
    public class SubjectsViewModel : RoutableViewModelBase
    {
        private readonly ISubjectService _subjectService;
        private readonly IDepartmentService _departmentService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;

        [Reactive] public ObservableCollection<SubjectViewModel> Subjects { get; set; } = new();
        [Reactive] public SubjectViewModel? SelectedSubject { get; set; }
        [Reactive] public string SearchTerm { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }

        // Команды
        public ReactiveCommand<Unit, Unit> LoadSubjectsCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CreateSubjectCommand { get; private set; }
        public ReactiveCommand<SubjectViewModel, Unit> EditSubjectCommand { get; private set; }
        public ReactiveCommand<SubjectViewModel, Unit> DeleteSubjectCommand { get; private set; }
        public ReactiveCommand<SubjectViewModel, Unit> ViewSubjectDetailsCommand { get; private set; }
        public ReactiveCommand<string, Unit> SearchCommand { get; private set; }

        public SubjectsViewModel(
            IScreen hostScreen,
            ISubjectService subjectService,
            IDepartmentService departmentService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService)
            : base(hostScreen)
        {
            _subjectService = subjectService;
            _departmentService = departmentService;
            _dialogService = dialogService;
            _statusService = statusService;
            _notificationService = notificationService;

            InitializeCommands();
            
            // Автоматическая загрузка при инициализации
            this.WhenActivated(disposables =>
            {
                LoadSubjectsCommand.Execute().Subscribe().DisposeWith(disposables);
            });
        }

        private void InitializeCommands()
        {
            LoadSubjectsCommand = CreateCommand(LoadSubjectsAsync, null, "Ошибка загрузки предметов");
            RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
            CreateSubjectCommand = CreateCommand(CreateSubjectAsync, null, "Ошибка создания предмета");
            EditSubjectCommand = CreateCommand<SubjectViewModel>(EditSubjectAsync, null, "Ошибка редактирования предмета");
            DeleteSubjectCommand = CreateCommand<SubjectViewModel>(DeleteSubjectAsync, null, "Ошибка удаления предмета");
            ViewSubjectDetailsCommand = CreateCommand<SubjectViewModel>(ViewSubjectDetailsAsync, null, "Ошибка просмотра деталей предмета");
            SearchCommand = CreateCommand<string>(SearchSubjectsAsync, null, "Ошибка поиска предметов");
        }

        private async Task LoadSubjectsAsync()
        {
            LogInfo("Loading subjects");
            IsLoading = true;

            try
            {
                var subjects = await _subjectService.GetAllSubjectsAsync();
                Subjects.Clear();
                
                foreach (var subject in subjects)
                {
                    Subjects.Add(new SubjectViewModel(subject));
                }

                LogInfo("Loaded {SubjectCount} subjects", Subjects.Count);
                ShowSuccess($"Загружено {Subjects.Count} предметов");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to load subjects");
                ShowError("Не удалось загрузить список предметов");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshAsync()
        {
            LogInfo("Refreshing subjects data");
            IsRefreshing = true;
            
            await LoadSubjectsAsync();
            ShowSuccess("Данные обновлены");
            
            IsRefreshing = false;
        }

        private async Task CreateSubjectAsync()
        {
            LogInfo("Creating new subject");
            
            var newSubject = new Subject
            {
                Uid = Guid.NewGuid(),
                Name = string.Empty,
                Code = string.Empty,
                Description = string.Empty,
                IsActive = true,
                Credits = 3,
                LessonsPerWeek = 2
            };

            var dialogResult = await _dialogService.ShowSubjectEditDialogAsync(newSubject);
            if (dialogResult == null)
            {
                LogDebug("Subject creation cancelled by user");
                return;
            }

            try
            {
                await _subjectService.AddSubjectAsync(dialogResult);
                Subjects.Add(new SubjectViewModel(dialogResult));
                
                LogInfo("Subject created successfully: {SubjectName}", dialogResult.Name);
                ShowSuccess($"Предмет '{dialogResult.Name}' успешно создан");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to create subject");
                ShowError("Не удалось создать предмет");
            }
        }

        private async Task EditSubjectAsync(SubjectViewModel subjectVm)
        {
            if (subjectVm == null) return;

            LogInfo("Editing subject: {SubjectName}", subjectVm.Name);

            try
            {
                var subject = await _subjectService.GetSubjectAsync(subjectVm.Uid);
                if (subject == null)
                {
                    ShowError("Предмет не найден");
                    return;
                }

                var dialogResult = await _dialogService.ShowSubjectEditDialogAsync(subject);
                if (dialogResult == null)
                {
                    LogDebug("Subject editing cancelled by user");
                    return;
                }

                await _subjectService.UpdateSubjectAsync(dialogResult);
                
                // Обновляем элемент в коллекции
                var index = Subjects.IndexOf(subjectVm);
                if (index >= 0)
                {
                    Subjects[index] = new SubjectViewModel(dialogResult);
                }

                LogInfo("Subject updated successfully: {SubjectName}", dialogResult.Name);
                ShowSuccess($"Предмет '{dialogResult.Name}' успешно обновлен");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to edit subject");
                ShowError("Не удалось обновить предмет");
            }
        }

        private async Task DeleteSubjectAsync(SubjectViewModel subjectVm)
        {
            if (subjectVm == null) return;

            LogInfo("Deleting subject: {SubjectName}", subjectVm.Name);

            var confirmResult = await _dialogService.ShowConfirmationDialogAsync(
                "Удаление предмета",
                $"Вы уверены, что хотите удалить предмет '{subjectVm.Name}'?\n\nЭто действие нельзя отменить.",
                "Удалить",
                "Отмена");

            if (!confirmResult)
            {
                LogDebug("Subject deletion cancelled by user");
                return;
            }

            try
            {
                var success = await _subjectService.DeleteSubjectAsync(subjectVm.Uid);
                if (success)
                {
                    Subjects.Remove(subjectVm);
                    if (SelectedSubject == subjectVm)
                    {
                        SelectedSubject = null;
                    }

                    LogInfo("Subject deleted successfully: {SubjectName}", subjectVm.Name);
                    ShowSuccess($"Предмет '{subjectVm.Name}' успешно удален");
                }
                else
                {
                    ShowError("Не удалось удалить предмет");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to delete subject");
                ShowError("Не удалось удалить предмет");
            }
        }

        private async Task ViewSubjectDetailsAsync(SubjectViewModel subjectVm)
        {
            if (subjectVm == null) return;

            LogInfo("Viewing subject details: {SubjectName}", subjectVm.Name);

            try
            {
                var subject = await _subjectService.GetSubjectAsync(subjectVm.Uid);
                if (subject == null)
                {
                    ShowError("Предмет не найден");
                    return;
                }

                await _dialogService.ShowSubjectDetailsDialogAsync(subject);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to view subject details");
                ShowError("Не удалось загрузить детали предмета");
            }
        }

        private async Task SearchSubjectsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                await LoadSubjectsAsync();
                return;
            }

            LogInfo("Searching subjects with term: {SearchTerm}", searchTerm);

            try
            {
                var subjects = await _subjectService.SearchSubjectsAsync(searchTerm);
                Subjects.Clear();
                
                foreach (var subject in subjects)
                {
                    Subjects.Add(new SubjectViewModel(subject));
                }

                LogInfo("Found {SubjectCount} subjects matching search term", Subjects.Count);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to search subjects");
                ShowError("Ошибка при поиске предметов");
            }
        }

        private new void ShowSuccess(string message)
        {
            _notificationService.ShowSuccess(message);
        }

        private new void ShowError(string message)
        {
            _notificationService.ShowError(message);
        }
    }
} 
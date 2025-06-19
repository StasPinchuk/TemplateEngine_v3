using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.UsersServices;
using TemplateEngine_v3.UserControls;
using TemplateEngine_v3.Views.Pages;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.VM.Windows
{
    /// <summary>
    /// ViewModel главного окна приложения, отвечает за навигацию и основные команды.
    /// </summary>
    public class MainWindowVM : BaseNotifyPropertyChanged
    {
        private readonly ITemplateManager _templateManager;
        private readonly IBranchManager _branchManager;
        private readonly ITechnologiesManager _technologiesManager;
        private readonly UserManager _userManager;
        private readonly ColumnDefinition _sideBar;
        private readonly Frame _mainFrame;

        private Visibility _isAdminVisibility = Visibility.Collapsed;

        public Visibility IsAdminVisibility
        {
            get => _isAdminVisibility;
            set => SetValue(ref _isAdminVisibility, value, nameof(IsAdminVisibility));
        }

        /// <summary>
        /// Коллекция элементов меню для боковой панели.
        /// </summary>
        public ObservableCollection<PageModel> MenuItems { get; } = new();

        /// <summary>
        /// Команда открытия страницы.
        /// </summary>
        public ICommand OpenPageCommand { get; set; }

        /// <summary>
        /// Команда сохранения данных в JSON файлы.
        /// </summary>
        public ICommand SaveToJsonCommand { get; set; }

        /// <summary>
        /// Команда для замены значений в шаблонах.
        /// </summary>
        public ICommand ReplaceValueInTemplateCommand { get; set; }

        /// <summary>
        /// Команда для открытия списка типов деталей.
        /// </summary>
        public ICommand DetailTypeListCommand { get; set; }

        /// <summary>
        /// Команда для открытия окна настроек.
        /// </summary>
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenLogsCommand { get; set; }

        /// <summary>
        /// Конструктор ViewModel главного окна.
        /// </summary>
        /// <param name="referenceManager">Менеджер ссылок для получения сервисов.</param>
        /// <param name="userManager">Менеджер пользователей.</param>
        /// <param name="mainFrame">Фрейм для навигации страниц.</param>
        /// <param name="sideBar">Колонка сайдбара для управления разметкой.</param>
        public MainWindowVM(ReferenceManager referenceManager, UserManager userManager, Frame mainFrame, ColumnDefinition sideBar)
        {
            _templateManager = referenceManager.TemplateManager;
            _branchManager = referenceManager.BranchManager;
            _technologiesManager = referenceManager.TechnologiesManager;
            _userManager = userManager;
            _sideBar = sideBar;
            _mainFrame = mainFrame;
            MenuHistory.MainFrame = mainFrame;

            var menuItems = new List<PageModel>
            {
                new PageModel
                {
                    Title = "Шаблоны",
                    Icon = PackIconKind.DocumentSign,
                    PageType = typeof(ReferencePage),
                    IsSelected = true,
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _templateManager, _technologiesManager, _branchManager, _userManager, TemplateClass.Ready, sideBar }
                },
                new PageModel
                {
                    Title = "Черновики",
                    Icon = PackIconKind.Draft,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _templateManager, _technologiesManager, _branchManager, _userManager, TemplateClass.Draft, sideBar }
                },
                new PageModel
                {
                    Title = "Корзина",
                    Icon = PackIconKind.TrashCan,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _templateManager, _branchManager, _userManager, TemplateClass.TrashCan, sideBar }
                },
                new PageModel
                {
                    Title = "Тех. процессы",
                    Icon = PackIconKind.Wrench,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _technologiesManager, _userManager, sideBar }
                },
                new PageModel
                {
                    Title = "Филиалы",
                    Icon = PackIconKind.SourceBranch,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _branchManager, _userManager, sideBar }
                },
                new PageModel
                {
                    Title = "Пользователи",
                    Icon = PackIconKind.User,
                    PageType = typeof(UsersPage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _userManager, sideBar },
                    IsEnabled = userManager.CurrentUser.IsAdmin,
                }
            };

            IsAdminVisibility = userManager.CurrentUser.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

            foreach (var item in menuItems)
                MenuItems.Add(item);

            // Навигация на первую страницу меню по умолчанию
            var currentPage = MenuItems.First().ModelPage;
            mainFrame.Navigate(currentPage);
            currentPage = null;

            InitializeCommand();
        }

        /// <summary>
        /// Инициализация команд.
        /// </summary>
        private void InitializeCommand()
        {
            OpenPageCommand = new RelayCommand(OnPageOpen);
            SaveToJsonCommand = new RelayCommand(SaveToJson);
            ReplaceValueInTemplateCommand = new RelayCommand(ReplaceValueInTemplate);
            DetailTypeListCommand = new RelayCommand(DetailTypeList);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenLogsCommand = new RelayCommand(OpenLogs);
        }

        /// <summary>
        /// Обработчик команды открытия страницы.
        /// Очищает историю меню, навигирует на выбранную страницу и сбрасывает состояние менеджеров.
        /// </summary>
        /// <param name="parameter">Ожидается объект PageModel.</param>
        private void OnPageOpen(object parameter)
        {
            if (parameter is PageModel pageModel)
            {
                if (MenuHistory.VisiblePageHistory.Count > 0)
                    _sideBar.Width = GridLength.Auto;

                MenuHistory.Clear();
                pageModel.ClearPage();
                _mainFrame.Navigate(pageModel.ModelPage);

                _templateManager.ClearTemplate();
                _technologiesManager.CurrentTechnologies = null;
            }
        }

        /// <summary>
        /// Сохраняет данные различных менеджеров в JSON файлы в соответствующие папки.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        private void SaveToJson(object parameter)
        {
            FileService.WriteToFolder("saveTemplate\\Ready", _templateManager.GetReadyTemplate(),
                t => t.Name, t => t.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\Draft", _templateManager.GetDraftTemplates(),
                t => t.Name, t => t.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\TrahCan", _templateManager.GetTrashCanTemplates(),
                t => t.Name, t => t.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\Technologies", _technologiesManager.GetAllTechnologies(),
                tech => tech.Name, tech => tech.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\Branch", _branchManager.GetAllBranches(),
                b => b.Name, b => b.ObjectStruct);
        }

        /// <summary>
        /// Открывает диалог замены значений в шаблонах.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        private async void ReplaceValueInTemplate(object parameter)
        {
            var dialog = new ReplaceChoiceDialog(_templateManager.GetReadyTemplate(), _templateManager);
            await DialogHost.Show(dialog, "MainDialog");
        }

        /// <summary>
        /// Открывает диалог выбора типа деталей.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        private async void DetailTypeList(object parameter)
        {
            var dialog = new DeteilTypeChoiceDialog();
            await DialogHost.Show(dialog, "MainDialog");
        }

        /// <summary>
        /// Открывает окно настроек приложения.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        private async void OpenSettings(object parameter)
        {
            var dialog = new SettingsChoiceDialog();
            await DialogHost.Show(dialog, "MainDialog");
        }

        private async void OpenLogs(object parameter)
        {
            var dialog = new LogsChoiceDialog();
            await DialogHost.Show(dialog, "MainDialog");
        }
    }
}

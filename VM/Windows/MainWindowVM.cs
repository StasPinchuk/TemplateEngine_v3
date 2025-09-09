using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;
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
        private TemplateManager _templateManager;
        private BranchManager _branchManager;
        private TechnologiesManager _technologiesManager;
        private TemplateStageService _stageService;
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
        public ObservableCollection<NavigationTabs> TabsItems { get; set; } = new();

        private NavigationTabs _selectedTabs = new();

        public NavigationTabs SelectedTab
        {
            get => _selectedTabs;
            set
            {
                if (Equals(_selectedTabs, value) || value == null)
                    return;

                ApplySelectedTab(value);
            }
        }

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
        /// Команда открытия окна логов.
        /// </summary>
        public ICommand OpenLogsCommand { get; set; }

        /// <summary>
        /// Команда открытия диалога этапов шаблонов.
        /// </summary>
        public ICommand OpenTemplateStagesCommand { get; set; }

        /// <summary>
        /// Команда закрытия вкладки.
        /// </summary>
        public ICommand CloseTabCommand { get; set; }


        /// <summary>
        /// Инициализирует новый экземпляр <see cref="MainWindowVM"/> —
        /// ViewModel главного окна приложения, настраивая менеджеры данных,
        /// сервисы, навигацию и элементы интерфейса.
        /// </summary>
        /// <param name="referenceManager">
        /// Центральный менеджер ссылок, предоставляющий доступ к сервисам и менеджерам данных
        /// (шаблоны, филиалы, технологии, этапы).
        /// </param>
        /// <param name="userManager">
        /// Менеджер пользователей, содержащий информацию о текущем пользователе и его правах.
        /// </param>
        /// <param name="mainFrame">
        /// Основной фрейм для загрузки и отображения страниц приложения.
        /// </param>
        /// <param name="sideBar">
        /// Определение колонки сайдбара, используемое для управления его отображением и шириной.
        /// </param>
        public MainWindowVM(ReferenceManager referenceManager, UserManager userManager, Frame mainFrame, ColumnDefinition sideBar)
        {
            _templateManager = referenceManager.TemplateManager;
            _branchManager = referenceManager.BranchManager;
            _technologiesManager = referenceManager.TechnologiesManager;
            _stageService = referenceManager.TemplateStageService;
            _userManager = userManager;
            _sideBar = sideBar;
            _mainFrame = mainFrame;
            NavigationService.SetMainFrame(mainFrame);

            _stageService.SetStageList();

            CreateMenuList();
            CreateTabsList();

            InitializeCommand();
        }

        /// <summary>
        /// Метод создания списка разделов меню
        /// </summary>
        private void CreateMenuList()
        {
            bool isAdmin = _userManager.CurrentUser.IsAdmin;

            var items = new[]
            {
                CreatePage(
                    title: "Шаблоны",
                    icon: PackIconKind.DocumentSign,
                    pageType: typeof(ReferencePage),
                    isSelected: true,
                    isEnabled: true,
                    _templateManager, _technologiesManager, _branchManager, _userManager, _stageService, TemplateClass.Ready, _sideBar
                ),
                CreatePage(
                    title: "Архив",
                    icon: PackIconKind.Archive,
                    pageType: typeof(ReferencePage),
                    isSelected: false,
                    isEnabled: true,
                    _templateManager, _branchManager, _userManager, _stageService, TemplateClass.TrashCan, _sideBar
                ),
                CreatePage(
                    title: "Тех. процессы",
                    icon: PackIconKind.Wrench,
                    pageType: typeof(ReferencePage),
                    isSelected: false,
                    isEnabled: true,
                    _technologiesManager, _userManager, _stageService, _sideBar
                ),
                CreatePage(
                    title: "Филиалы",
                    icon: PackIconKind.SourceBranch,
                    pageType: typeof(ReferencePage),
                    isSelected: false,
                    isEnabled: true,
                    _branchManager, _userManager, _stageService, _sideBar
                ),
                CreatePage(
                    title: "Пользователи",
                    icon: PackIconKind.User,
                    pageType: typeof(UsersPage),
                    isSelected: false,
                    isEnabled: isAdmin,
                    _userManager, _sideBar
                )
            };

            IsAdminVisibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            foreach (var item in items)
                MenuItems.Add(item);
        }

        /// <summary>
        /// Метод создания страницы
        /// </summary>
        /// <param name="title">Название страницы</param>
        /// <param name="icon">Иконка для отображения в меню</param>
        /// <param name="pageType">Тип страницы</param>
        /// <param name="isSelected">Активна ли страница сейчас</param>
        /// <param name="isEnabled">Блокировка раздела</param>
        /// <param name="constructorParameters">Параметры страницы</param>
        /// <returns></returns>
        private PageModel CreatePage(string title, PackIconKind icon, Type pageType, bool isSelected, bool isEnabled, params object[] constructorParameters)
        {
            return new PageModel
            {
                Title = title,
                Icon = icon,
                PageType = pageType,
                GroupName = "MainSideBar",
                IsSelected = isSelected,
                ConstructorParameters = constructorParameters,
                IsEnabled = isEnabled
            };
        }

        /// <summary>
        /// Метод создания списка вкладок
        /// </summary>
        private void CreateTabsList()
        {
            TabsItems = NavigationService.GetTabs();

            TabsItems.Add(
                    new NavigationTabs()
                    {
                        Title = MenuItems.First().Title,
                        Page = MenuItems.First(),
                        PageHistory = new()
                    }
                );

            SelectedTab = TabsItems.First();
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
            OpenLogsCommand = new RelayCommand(OpenLogs);
            CloseTabCommand = new RelayCommand(CloseTab);
            OpenTemplateStagesCommand = new RelayCommand(OpenTemplateStages);
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
                var findPage = TabsItems.FirstOrDefault(tab => tab.Title.Equals(pageModel.Title));

                if (findPage != null)
                {
                    findPage.Page.ClearPage();
                    SelectedTab = findPage;
                    _sideBar.Width = GridLength.Auto;
                    UpdateManagers(findPage.Page);
                }
                else
                {
                    pageModel.ClearPage();
                    TabsItems.Add(
                            new NavigationTabs()
                            {
                                Title = pageModel.Title,
                                Page = pageModel,
                                PageHistory = new()
                            }
                        );
                    SelectedTab = TabsItems.Last();
                }
            }
        }

        /// <summary>
        /// Обновляет текущие менеджеры на основе переданной страницы.
        /// </summary>
        /// <param name="page">Модель страницы для анализа.</param>
        private void UpdateManagers(PageModel page)
        {
            var findTemplateManager = page.ConstructorParameters.FirstOrDefault(param => param is TemplateManager) as TemplateManager;
            var findTechnologiesManager = page.ConstructorParameters.FirstOrDefault(param => param is TechnologiesManager) as TechnologiesManager;
            if (findTemplateManager != null)
                _templateManager = findTemplateManager;

            if (findTechnologiesManager != null)
                _technologiesManager = findTechnologiesManager;
        }

        /// <summary>
        /// Сохраняет данные различных менеджеров в JSON файлы в соответствующие папки.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        private void SaveToJson(object parameter)
        {
            FileService.WriteToFolder("saveTemplate\\AllTemplate", _templateManager.GetReadyTemplate(),
                t => t.Name.Replace('\"', '_'), t => t.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\TrahCan", _templateManager.GetTrashCanTemplates(),
                t => t.Name.Replace('\"', '_'), t => t.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\Technologies", _technologiesManager.GetAllTechnologies(),
                tech => tech.Name.Replace('\"', '_'), tech => tech.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\Branch", _branchManager.GetAllBranches(),
                b => b.Name.Replace('\"', '_'), b => b.ObjectStruct);

            MessageBox.Show("Все данные успешно сохранены в файлы", "Сохранение в файлы");
        }

        /// <summary>
        /// Открывает диалог замены значений в шаблонах.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        private async void ReplaceValueInTemplate(object parameter)
        {
            var dialog = new ReplaceChoiceDialog(_templateManager);
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
        /// Обрабатывает команду открытия окна логов.
        /// </summary>
        /// <param name="parameter">Параметр команды.</param>
        private async void OpenLogs(object parameter)
        {
            var dialog = new LogsChoiceDialog();
            await DialogHost.Show(dialog, "MainDialog");
        }

        /// <summary>
        /// Обрабатывает команду открытия окна этапов шаблонов.
        /// </summary>
        /// <param name="parameter">Параметр команды.</param>
        private async void OpenTemplateStages(object parameter)
        {
            var dialog = new TemplateStageChoiceDialog(_stageService);
            await DialogHost.Show(dialog, "MainDialog");
        }

        /// <summary>
        /// Закрывает указанную вкладку, очищает ресурсы и обновляет текущую вкладку.
        /// </summary>
        /// <param name="parameter">Вкладка, которую требуется закрыть.</param>
        private void CloseTab(object parameter)
        {
            if (parameter is NavigationTabs tabPanel)
            {
                var page = tabPanel.Page;

                int tabIndex = TabsItems.IndexOf(tabPanel);
                if (tabIndex == -1)
                    return;

                var templateManager = page.ConstructorParameters
                    .OfType<TemplateManager>()
                    .FirstOrDefault();
                templateManager?.ClearTemplate();

                var technologiesManager = page.ConstructorParameters
                    .OfType<TechnologiesManager>()
                    .FirstOrDefault();
                if (technologiesManager != null)
                    technologiesManager.CurrentTechnologies = null;

                if (TabsItems.Count == 1)
                {
                    TabsItems.Add(new NavigationTabs
                    {
                        Title = MenuItems.First().Title,
                        Page = MenuItems.First(),
                        PageHistory = new()
                    });
                    SelectedTab = TabsItems.Last();
                }
                else
                {
                    int newIndex = tabIndex > 0 ? tabIndex - 1 : 1;
                    SelectedTab = TabsItems[newIndex];
                }

                tabPanel.PageHistory.Clear();
                TabsItems.Remove(tabPanel);
            }
        }

        /// <summary>
        /// Применяет выбранную вкладку навигации, обновляя состояние меню, 
        /// менеджеров и выполняя переход на соответствующую страницу.
        /// </summary>
        /// <param name="newTab">
        /// Вкладка навигации, которая должна быть активирована.
        /// Содержит связанную страницу, историю переходов и метаданные.
        /// </param>
        private void ApplySelectedTab(NavigationTabs newTab)
        {
            var selectedPage = MenuItems.FirstOrDefault(item => item.Title == newTab.Title);
            if (selectedPage != null)
            {
                selectedPage.IsSelected = true;
                _sideBar.Width = GridLength.Auto;
            }

            newTab.Page.ClearPage();

            SetValue(ref _selectedTabs, newTab, nameof(SelectedTab));

            NavigationService.SetSelectedTab(newTab);
            UpdateManagers(newTab.Page);

            _mainFrame.Navigate(newTab.Page.ModelPage);

            if (newTab.PageHistory.Count > 0)
                NavigationService.SetPageInSecondaryFrame();
        }
    }
}

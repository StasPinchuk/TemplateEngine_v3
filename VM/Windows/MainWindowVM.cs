using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
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
        public ObservableCollection<NavigationTabs> TabsItem { get; } = new();

        private NavigationTabs _selectedTabs = new();

        public NavigationTabs SelectedTab
        {
            get => _selectedTabs;
            set
            {
                if (value != null && MenuItems.Any(item => item.Title.Equals(value.Title)))
                    _sideBar.Width = GridLength.Auto;

                var selectedPage = MenuItems.FirstOrDefault(item => item.Title.Equals(value.Title));
                if (selectedPage != null)
                    selectedPage.IsSelected = true;

                value.Page.ClearPage();

                SetValue(ref _selectedTabs, value, nameof(SelectedTab));

                NavigationService.SetSelectedTab(value);

                UpdateManagers(value.Page);

                _mainFrame.Navigate(value.Page.ModelPage);
                if (value.PageHistory.Count > 0)
                {
                    NavigationService.SetPageInSecondaryFrame();
                }

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
        /// Команда для открытия окна настроек.
        /// </summary>
        public ICommand OpenSettingsCommand { get; set; }

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
            _stageService = referenceManager.TemplateStageService;
            _userManager = userManager;
            _sideBar = sideBar;
            _mainFrame = mainFrame;
            NavigationService.SetMainFrame(mainFrame);

            _stageService.SetStageList();

            TabsItem = NavigationService.GetTabs();

            var menuItems = new List<PageModel>
            {
                new PageModel
                {
                    Title = "Шаблоны",
                    Icon = PackIconKind.DocumentSign,
                    PageType = typeof(ReferencePage),
                    IsSelected = true,
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _templateManager, _technologiesManager, _branchManager, _userManager, _stageService, TemplateClass.Ready, sideBar }
                },
                new PageModel
                {
                    Title = "Архив",
                    Icon = PackIconKind.Archive,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _templateManager, _branchManager, _userManager, _stageService, TemplateClass.TrashCan, sideBar }
                },
                new PageModel
                {
                    Title = "Тех. процессы",
                    Icon = PackIconKind.Wrench,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _technologiesManager, _userManager, _stageService, sideBar }
                },
                new PageModel
                {
                    Title = "Филиалы",
                    Icon = PackIconKind.SourceBranch,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _branchManager, _userManager, _stageService, sideBar }
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

            TabsItem.Add(
                    new NavigationTabs()
                    {
                        Title = MenuItems.First().Title,
                        Page = MenuItems.First(),
                        PageHistory = new()
                    }
                );

            SelectedTab = TabsItem.First();

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
                var findPage = TabsItem.FirstOrDefault(tab => tab.Title.Equals(pageModel.Title));

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
                    TabsItem.Add(
                            new NavigationTabs()
                            {
                                Title = pageModel.Title,
                                Page = pageModel,
                                PageHistory = new()
                            }
                        );
                    SelectedTab = TabsItem.Last();
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

        private void ClearManager()
        {
            _templateManager.ClearTemplate();
            _technologiesManager.CurrentTechnologies = null;
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
        /// Открывает окно настроек приложения.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        private async void OpenSettings(object parameter)
        {
            var dialog = new SettingsChoiceDialog();
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

                int tabIndex = TabsItem.IndexOf(tabPanel);
                if (tabIndex == -1)
                    return; // защита от некорректного элемента

                // Очистка ресурсов страницы
                var templateManager = page.ConstructorParameters
                    .OfType<TemplateManager>()
                    .FirstOrDefault();
                templateManager?.ClearTemplate();

                var technologiesManager = page.ConstructorParameters
                    .OfType<TechnologiesManager>()
                    .FirstOrDefault();
                if (technologiesManager != null)
                    technologiesManager.CurrentTechnologies = null;

                // Выбор новой активной вкладки до удаления
                if (TabsItem.Count == 1)
                {
                    // Удаляем последнюю вкладку, создаём новую
                    TabsItem.Add(new NavigationTabs
                    {
                        Title = MenuItems.First().Title,
                        Page = MenuItems.First(),
                        PageHistory = new()
                    });
                    SelectedTab = TabsItem.Last();
                }
                else
                {
                    // Выбираем соседнюю вкладку
                    int newIndex = tabIndex > 0 ? tabIndex - 1 : 1;
                    SelectedTab = TabsItem[newIndex];
                }

                // Очистка истории и удаление вкладки
                tabPanel.PageHistory.Clear();
                TabsItem.Remove(tabPanel);
            }

        }
    }
}

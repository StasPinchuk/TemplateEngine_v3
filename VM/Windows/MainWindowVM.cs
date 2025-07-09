using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Services.UsersServices;
using TemplateEngine_v3.UserControls;
using TemplateEngine_v3.Views.Pages;
using TemplateEngine_v3.VM.Pages;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

                var tabs = NavigationService.GetTabs();

                var tab = tabs.FirstOrDefault(tabPage => tabPage.Title.Equals(value.Title));

                tab.Page.ClearPage();

                SetValue(ref _selectedTabs, tab, nameof(SelectedTab));

                NavigationService.SetSelectedTab(tab);

                _mainFrame.Navigate(value.Page.ModelPage);
                if (tab.PageHistory.Count > 0)
                {
                    NavigationService.SetPageInSecondaryFrame();
                }

                UpdateManagers(SelectedTab.Page);
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
        public ICommand OpenLogsCommand { get; set; }
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
            _userManager = userManager;
            _sideBar = sideBar;
            _mainFrame = mainFrame;
            NavigationService.SetMainFrame(mainFrame);

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
                /* if (MenuHistory.VisiblePageHistory.Count > 0)
                     _sideBar.Width = GridLength.Auto;

                 MenuHistory.Clear();
                 pageModel.ClearPage();
                 _mainFrame.Navigate(pageModel.ModelPage);

                 _templateManager.ClearTemplate();
                 _technologiesManager.CurrentTechnologies = null;*/

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

        private void UpdateManagers(PageModel page)
        {
            var findTemplateManager = page.ConstructorParameters.FirstOrDefault(param => param is TemplateManager) as TemplateManager;
            var findTechnologiesManager = page.ConstructorParameters.FirstOrDefault(param => param is TechnologiesManager) as TechnologiesManager;
            if (findTemplateManager != null)
                _templateManager = findTemplateManager;

            if (findTechnologiesManager != null)
                _technologiesManager = findTechnologiesManager;

            _templateManager.ClearTemplate();
            _technologiesManager.CurrentTechnologies = null;
        }

        /// <summary>
        /// Сохраняет данные различных менеджеров в JSON файлы в соответствующие папки.
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется).</param>
        private void SaveToJson(object parameter)
        {
            FileService.WriteToFolder("saveTemplate\\Ready", _templateManager.GetReadyTemplate(),
                t => t.Name.Replace('\"', '_'), t => t.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\Draft", _templateManager.GetDraftTemplates(),
                t => t.Name.Replace('\"', '_'), t => t.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\TrahCan", _templateManager.GetTrashCanTemplates(),
                t => t.Name.Replace('\"', '_'), t => t.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\Technologies", _technologiesManager.GetAllTechnologies(),
                tech => tech.Name.Replace('\"', '_'), tech => tech.ObjectStruct);

            FileService.WriteToFolder("saveTemplate\\Branch", _branchManager.GetAllBranches(),
                b => b.Name.Replace('\"', '_'), b => b.ObjectStruct);
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

        private async void OpenLogs(object parameter)
        {
            var dialog = new LogsChoiceDialog();
            await DialogHost.Show(dialog, "MainDialog");
        }

        private void CloseTab(object parameter)
        {
            if(parameter is NavigationTabs tabPanel)
            {
                var page = tabPanel.Page;

                int tabIndex = TabsItem.IndexOf(tabPanel);

                if(TabsItem.Count > 1)
                {
                    var templateManager = page.ConstructorParameters.FirstOrDefault(param => param is TemplateManager) as TemplateManager;
                    var technologiesManager = page.ConstructorParameters.FirstOrDefault(param => param is TechnologiesManager) as TechnologiesManager;
                    if (templateManager != null)
                        templateManager.ClearTemplate();

                    if (technologiesManager != null)
                        technologiesManager.CurrentTechnologies = null;
                    if(tabIndex != 0 && tabIndex <= TabsItem.Count)
                        SelectedTab = TabsItem[tabIndex - 1];
                    else
                        SelectedTab = TabsItem[tabIndex + 1];
                        tabPanel.PageHistory.Clear();
                    TabsItem.Remove(tabPanel);
                }
            }
        }
    }
}

using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.UserControls;
using TemplateEngine_v3.Views.Pages;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.VM.Windows
{
    public class MainWindowVM : BaseNotifyPropertyChanged
    {
        private readonly ITemplateManager _templateManager;
        private readonly IBranchManager _branchManager;
        private readonly ITechnologiesManager _technologiesManager;
        private readonly OperationNamesManager _operationNamesManager;
        private ColumnDefinition _sideBar;
        private Frame _mainFrame;

        public ObservableCollection<PageModel> MenuItems { get; } = new();

        public ICommand OpenPageCommand { get; set; }
        public ICommand SaveToJsonCommand { get; set; }
        public ICommand ReplaceValueInTemplateCommand { get; set; }

        public MainWindowVM(ReferenceManager referenceManager, Frame mainFrame, ColumnDefinition sideBar)
        {
            _templateManager = referenceManager.TemplateManager;
            _branchManager = referenceManager.BranchManager;
            _technologiesManager = referenceManager.TechnologiesManager;
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
                    ConstructorParameters = new object[] { _templateManager, _technologiesManager, TemplateClass.Ready, sideBar }
                },
                new PageModel
                {
                    Title = "Черновики",
                    Icon = PackIconKind.Draft,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _templateManager, _technologiesManager, TemplateClass.Draft, sideBar }
                },
                new PageModel
                {
                    Title = "Корзина",
                    Icon = PackIconKind.TrashCan,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _templateManager, TemplateClass.TrashCan, sideBar }
                },
                new PageModel
                {
                    Title = "Тех. процессы",
                    Icon = PackIconKind.Wrench,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _technologiesManager, sideBar }
                },
                new PageModel
                {
                    Title = "Филиалы",
                    Icon = PackIconKind.SourceBranch,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _branchManager, sideBar }
                },
                new PageModel
                {
                    Title = "Пользователи",
                    Icon = PackIconKind.User,
                    PageType = typeof(ReferencePage),
                    GroupName = "MainSideBar",
                    ConstructorParameters = new object[] { _templateManager, TemplateClass.TrashCan, sideBar }
                }
            };

            foreach (var item in menuItems)
                MenuItems.Add(item);

            var currentPage = MenuItems.First().ModelPage;
            mainFrame.Navigate(currentPage);
            currentPage = null;

            OpenPageCommand = new RelayCommand(OnPageOpen);
            SaveToJsonCommand = new RelayCommand(SaveToJson);
            ReplaceValueInTemplateCommand = new RelayCommand(ReplaceValueInTemplate);
        }

        private void OnPageOpen(object parameter)
        {
            if (parameter is PageModel pageModel)
            {
                if(MenuHistory.VisiblePageHistory.Count > 0)
                    _sideBar.Width = GridLength.Auto;
                MenuHistory.Clear();
                pageModel.ClearPage();
                _mainFrame.Navigate(pageModel.ModelPage);
                _templateManager.ClearTemplate();
                _technologiesManager.CurrentTechnologies = null;
            }
        }

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

        private async void ReplaceValueInTemplate(object parameter)
        {
            var dialog = new ReplaceChoiceDialog(_templateManager.GetReadyTemplate(), _templateManager);
            await DialogHost.Show(dialog, "MainDialog");
        }

    }
}

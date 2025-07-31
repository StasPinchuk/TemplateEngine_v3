using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Services
{
    public static class NavigationService
    {
        public static Action UpdateHistory;

        private static NavigationTabs _selectedTab = new();

        private static ObservableCollection<NavigationTabs> _tabs { get; set; } = new();

        private static Frame _mainFrame { get; set; }

        private static Frame _secondaryFrame { get; set; }

        public static void SetSelectedTab(NavigationTabs tab)
        {
            _selectedTab = tab;
            UpdateHistory?.Invoke();
        }

        public static NavigationTabs GetSelectedTab() => _selectedTab;

        public static void SetMainFrame(Frame frame) => _mainFrame = frame;

        public static void SetSecondaryFrame(Frame frame) => _secondaryFrame = frame;

        public static void SetTabs(ObservableCollection<NavigationTabs> tabs) => _tabs = tabs;

        public static ObservableCollection<NavigationTabs> GetTabs() => _tabs;

        public static void RenameSelectedTab(string tabName) => _selectedTab.Title = tabName;

        public static void AddPageToPageHistory(PageModel page)
        {
            if (!_selectedTab.PageHistory.Any(p => p.Title.Equals(page.Title) && !p.Title.Equals("Детали")))
            {
                _selectedTab.PageHistory.Add(page);
            }
        }

        public static void RemovePageToPageHistory(PageModel page)
        {
            var nodeManager = page.ConstructorParameters.FirstOrDefault(param => param is NodeManager) as NodeManager;

            if (nodeManager != null)
                nodeManager.ClearAction();

            var templateManager = page.ConstructorParameters.FirstOrDefault(param => param is TemplateManager) as TemplateManager;
            _selectedTab.PageHistory.Remove(page);
        }

        public static ObservableCollection<PageModel> GetPageHistory() => _selectedTab.PageHistory;

        public static void SetPageInMainFrame()
        {
            if (_selectedTab.PageHistory.Count > 0)
                _mainFrame?.Navigate(_selectedTab.PageHistory.Last().ModelPage);
            else
                _mainFrame?.Navigate(_selectedTab.Page.ModelPage);
        }

        public static void SetPageInMainFrame(PageModel pageModel)
        {
            _selectedTab.Page = pageModel;
            _mainFrame?.Navigate(pageModel.ModelPage);

        }

        public static void SetPageInSecondaryFrame()
        {
            _secondaryFrame?.Navigate(_selectedTab.PageHistory.Last().ModelPage);
        }


    }
}

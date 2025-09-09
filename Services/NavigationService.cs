using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Services
{
    /// <summary>
    /// Статический сервис для управления навигацией между вкладками и страницами приложения.
    /// Хранит состояние выбранной вкладки, историю страниц и управляет основным и дополнительным фреймами.
    /// </summary>
    public static class NavigationService
    {
        /// <summary>
        /// Делегат, вызываемый при обновлении истории страниц.
        /// </summary>
        public static Action UpdateHistory;

        /// <summary>
        /// Текущая выбранная вкладка.
        /// </summary>
        private static NavigationTabs _selectedTab = new();

        /// <summary>
        /// Коллекция всех вкладок навигации.
        /// </summary>
        private static ObservableCollection<NavigationTabs> _tabs { get; set; } = new();

        /// <summary>
        /// Основной фрейм для отображения страниц.
        /// </summary>
        private static Frame _mainFrame { get; set; }

        /// <summary>
        /// Дополнительный фрейм для отображения вспомогательных страниц.
        /// </summary>
        private static Frame _secondaryFrame { get; set; }

        /// <summary>
        /// Устанавливает выбранную вкладку и вызывает обновление истории.
        /// </summary>
        /// <param name="tab">Вкладка, которую нужно выбрать.</param>
        public static void SetSelectedTab(NavigationTabs tab)
        {
            _selectedTab = tab;
            UpdateHistory?.Invoke();
        }

        /// <summary>
        /// Возвращает текущую выбранную вкладку.
        /// </summary>
        public static NavigationTabs GetSelectedTab() => _selectedTab;

        /// <summary>
        /// Устанавливает основной фрейм.
        /// </summary>
        /// <param name="frame">Экземпляр фрейма.</param>
        public static void SetMainFrame(Frame frame) => _mainFrame = frame;

        /// <summary>
        /// Устанавливает дополнительный фрейм.
        /// </summary>
        /// <param name="frame">Экземпляр фрейма.</param>
        public static void SetSecondaryFrame(Frame frame) => _secondaryFrame = frame;

        /// <summary>
        /// Устанавливает коллекцию вкладок.
        /// </summary>
        /// <param name="tabs">Коллекция вкладок.</param>
        public static void SetTabs(ObservableCollection<NavigationTabs> tabs) => _tabs = tabs;

        /// <summary>
        /// Возвращает коллекцию вкладок.
        /// </summary>
        public static ObservableCollection<NavigationTabs> GetTabs() => _tabs;

        /// <summary>
        /// Переименовывает текущую выбранную вкладку.
        /// </summary>
        /// <param name="tabName">Новое имя вкладки.</param>
        public static void RenameSelectedTab(string tabName) => _selectedTab.Title = tabName;

        /// <summary>
        /// Добавляет страницу в историю текущей вкладки, если она там ещё не присутствует.
        /// </summary>
        /// <param name="page">Добавляемая страница.</param>
        public static void AddPageToPageHistory(PageModel page)
        {
            if (!_selectedTab.PageHistory.Any(p => p.Title.Equals(page.Title) && !p.Title.Equals("Детали")))
            {
                _selectedTab.PageHistory.Add(page);
            }
        }

        /// <summary>
        /// Удаляет страницу из истории текущей вкладки и очищает связанные ресурсы.
        /// </summary>
        /// <param name="page">Удаляемая страница.</param>
        public static void RemovePageToPageHistory(PageModel page)
        {
            var nodeManager = page.ConstructorParameters.FirstOrDefault(param => param is NodeManager) as NodeManager;

            if (nodeManager != null)
                nodeManager.ClearAction();

            var templateManager = page.ConstructorParameters.FirstOrDefault(param => param is TemplateManager) as TemplateManager;
            _selectedTab.PageHistory.Remove(page);
        }

        /// <summary>
        /// Возвращает историю страниц текущей вкладки.
        /// </summary>
        public static ObservableCollection<PageModel> GetPageHistory() => _selectedTab.PageHistory;

        /// <summary>
        /// Устанавливает в основной фрейм последнюю страницу из истории или главную страницу вкладки.
        /// </summary>
        public static void SetPageInMainFrame()
        {
            if (_selectedTab.PageHistory.Count > 0)
                _mainFrame?.Navigate(_selectedTab.PageHistory.Last().ModelPage);
            else
                _mainFrame?.Navigate(_selectedTab.Page.ModelPage);
        }

        /// <summary>
        /// Устанавливает указанную страницу в основной фрейм и делает её текущей для вкладки.
        /// </summary>
        /// <param name="pageModel">Модель страницы для отображения.</param>
        public static void SetPageInMainFrame(PageModel pageModel)
        {
            _selectedTab.Page = pageModel;
            _mainFrame?.Navigate(pageModel.ModelPage);
        }

        /// <summary>
        /// Устанавливает в дополнительный фрейм последнюю страницу из истории текущей вкладки.
        /// </summary>
        public static void SetPageInSecondaryFrame()
        {
            _secondaryFrame?.Navigate(_selectedTab.PageHistory.Last().ModelPage);
        }
    }
}

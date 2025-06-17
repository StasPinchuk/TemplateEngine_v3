using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using TemplateEngine_v3.Models.PageCollection;

namespace TemplateEngine_v3.Services
{
    /// <summary>
    /// Класс для управления историей навигации по страницам приложения.
    /// Поддерживает работу с двумя фреймами: основным и второстепенным.
    /// </summary>
    public static class MenuHistory
    {
        /// <summary>
        /// Основной фрейм, в который происходит навигация.
        /// </summary>
        public static Frame MainFrame { get; set; }

        /// <summary>
        /// Вторичный фрейм (например, для отображения подстраниц или всплывающих блоков).
        /// </summary>
        public static Frame SecondaryFrame { get; set; }

        /// <summary>
        /// Текущая страница, на которой находится пользователь.
        /// </summary>
        private static PageModel _currentPage;

        /// <summary>
        /// Полная история посещённых страниц.
        /// </summary>
        private static ObservableCollection<PageModel> _pageHistory = new();

        /// <summary>
        /// Отображаемая история (для UI, с возможным сокращением).
        /// </summary>
        private static ObservableCollection<PageModel> _visiblePageHistory = new();

        /// <summary>
        /// Событие, вызываемое при обновлении истории. Используется для обновления UI.
        /// </summary>
        public static Action UpdateHistory;

        /// <summary>
        /// Получение/установка полной истории страниц.
        /// </summary>
        public static ObservableCollection<PageModel> PageHistory
        {
            get => _pageHistory;
            set => _pageHistory = value;
        }

        /// <summary>
        /// Получение/установка отображаемой истории страниц.
        /// </summary>
        public static ObservableCollection<PageModel> VisiblePageHistory
        {
            get => _visiblePageHistory;
            set => _visiblePageHistory = value;
        }

        /// <summary>
        /// Переход на следующую страницу. Добавляет страницу в историю и выполняет навигацию.
        /// </summary>
        /// <param name="nextPage">Модель следующей страницы.</param>
        /// <param name="mainFrame">Флаг, указывающий, использовать ли основной фрейм.</param>
        public static void NextPage(PageModel nextPage, bool mainFrame = false)
        {
            Frame frame = mainFrame ? MainFrame : SecondaryFrame;

            if (nextPage == null || nextPage.PageType == null)
                return;

            // Добавляем страницу в историю
            PageHistory.Add(nextPage);
            _currentPage = nextPage;

            // Создаём страницу из модели
            Page newPage = nextPage.ModelPage;

            // Обновляем видимую историю
            UpdateVisibleHistory();

            // Навигация и очистка истории назад
            frame.Navigate(newPage);
            while (frame.CanGoBack)
                frame.RemoveBackEntry();

            newPage = null;
        }

        /// <summary>
        /// Возвращение к предыдущей странице в истории.
        /// </summary>
        /// <param name="prevPage">Целевая страница для возврата.</param>
        /// <param name="mainFrame">Флаг, указывающий, использовать ли основной фрейм.</param>
        public static void PrevPage(PageModel prevPage, bool mainFrame = false)
        {
            Frame frame = mainFrame ? MainFrame : SecondaryFrame;

            if (prevPage == null || prevPage.PageType == null)
                return;

            // Проходимся по истории в обратном порядке и удаляем все после целевой
            for (int i = PageHistory.Count - 1; i >= 0; i--)
            {
                if (PageHistory[i] == prevPage)
                {
                    _currentPage = PageHistory[i];
                    break;
                }

                var model = PageHistory[i];

                if (model is PageModel pageModel)
                    pageModel.ClearPage(); // Очистка ресурсов страницы

                PageHistory.RemoveAt(i);
            }

            UpdateVisibleHistory();

            var newPage = _currentPage.ModelPage;

            frame.Navigate(newPage);
            while (frame.CanGoBack)
                frame.RemoveBackEntry();
        }

        /// <summary>
        /// Обновляет отображаемую историю, сокращая длинный список до первых и последних записей.
        /// </summary>
        private static void UpdateVisibleHistory()
        {
            _visiblePageHistory.Clear();

            if (_pageHistory.Count > 3)
            {
                _visiblePageHistory.Add(_pageHistory.First());
                _visiblePageHistory.Add(new PageModel("...")); // Страница-заглушка
                _visiblePageHistory.Add(_pageHistory.Last());
            }
            else
            {
                _visiblePageHistory = new(_pageHistory);
            }

            UpdateHistory?.Invoke(); // Уведомление UI
        }

        /// <summary>
        /// Полная очистка истории страниц и их ресурсов.
        /// </summary>
        public static void Clear()
        {
            Parallel.ForEach(PageHistory, page => page.ClearPage());
            Parallel.ForEach(VisiblePageHistory, page => page.ClearPage());

            PageHistory.Clear();
            VisiblePageHistory.Clear();
        }
    }
}

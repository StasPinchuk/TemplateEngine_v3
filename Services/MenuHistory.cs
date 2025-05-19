using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using TemplateEngine_v3.Models.PageCollection;

namespace TemplateEngine_v3.Services
{
    public static class MenuHistory
    {
        public static Frame MainFrame { get; set; }
        public static Frame SecondaryFrame { get; set; }

        private static PageModel _currentPage;

        private static ObservableCollection<PageModel> _pageHistory = new();
        private static ObservableCollection<PageModel> _visiblePageHistory = new();
        public static Action UpdateHistory;

        public static ObservableCollection<PageModel> PageHistory
        {
            get => _pageHistory; 
            set => _pageHistory = value;
        }

        public static ObservableCollection<PageModel> VisiblePageHistory
        {
            get => _visiblePageHistory; 
            set => _visiblePageHistory = value;
        }

        public static void NextPage(PageModel nextPage, bool mainFrame = false)
        {
            Frame frame = mainFrame ? MainFrame : SecondaryFrame;
            if (nextPage == null || nextPage.PageType == null)
                return;

            PageHistory.Add(nextPage);
            _currentPage = nextPage;
            Page newPage = nextPage.ModelPage;
            UpdateVisibleHistory();

            frame.Navigate(newPage);

            while (frame.CanGoBack)
                frame.RemoveBackEntry();
            newPage = null;
        }

        public static void PrevPage(PageModel prevPage, bool mainFrame = false)
        {
            Frame frame = mainFrame ? MainFrame : SecondaryFrame;
            if (prevPage == null || prevPage.PageType == null)
                return;

            for (int i = PageHistory.Count-1; i >= 0; i--)
            {
                if (PageHistory[i] == prevPage)
                {
                    _currentPage = PageHistory[i];
                    break;
                }

                var model = PageHistory[i];

                if (model is PageModel pageModel)
                    pageModel.ClearPage();

                PageHistory.RemoveAt(i);
            }

            UpdateVisibleHistory();

            var newPage = _currentPage.ModelPage;

            frame.Navigate(newPage);

            while (frame.CanGoBack)
                frame.RemoveBackEntry();
        }


        private static void UpdateVisibleHistory()
        {
            _visiblePageHistory.Clear();

            if (_pageHistory.Count > 3)
            {
                _visiblePageHistory.Add(_pageHistory.First());
                _visiblePageHistory.Add(new PageModel("..."));
                _visiblePageHistory.Add(_pageHistory.Last());
            }
            else
            {
                _visiblePageHistory = new(_pageHistory);
            }
            UpdateHistory?.Invoke();
        }

        public static void Clear()
        {
            Parallel.ForEach(PageHistory, page => page.ClearPage());
            Parallel.ForEach(VisiblePageHistory, page => page.ClearPage());
            PageHistory.Clear();
            VisiblePageHistory.Clear();
        }
    }
}

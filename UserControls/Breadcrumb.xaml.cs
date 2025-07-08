using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для Breadcrumb.xaml
    /// </summary>
    public partial class Breadcrumb : UserControl
    {
        public static readonly DependencyProperty HistoryProperty =
            DependencyProperty.Register(
                    nameof(History),
                    typeof(ObservableCollection<PageModel>),
                    typeof(Breadcrumb),
                    new PropertyMetadata(new ObservableCollection<PageModel>())
                );

        public static readonly DependencyProperty PrevSelectedPageCommandProperty =
            DependencyProperty.Register(
                    nameof(PrevSelectedPageCommand),
                    typeof(ICommand),
                    typeof(Breadcrumb),
                    new PropertyMetadata(null)
                );

        public static readonly DependencyProperty PrevPageCommandProperty =
            DependencyProperty.Register(
                    nameof(PrevPageCommand),
                    typeof(ICommand),
                    typeof(Breadcrumb),
                    new PropertyMetadata(null)
                );

        public static readonly DependencyProperty ButtonVisibilityProperty =
            DependencyProperty.Register(
                    nameof(ButtonVisibility), 
                    typeof(Visibility), 
                    typeof(Breadcrumb),
                    new PropertyMetadata(Visibility.Collapsed)
                );

        public ObservableCollection<PageModel> History
        {
            get => (ObservableCollection<PageModel>)GetValue(HistoryProperty);
            set => SetValue(HistoryProperty, value);
        }

        public Visibility ButtonVisibility
        {
            get => (Visibility)GetValue(ButtonVisibilityProperty);
            set => SetValue(ButtonVisibilityProperty, value);
        }

        public ICommand PrevPageCommand
        {
            get => (ICommand)GetValue(PrevPageCommandProperty);
            set => SetValue(PrevPageCommandProperty, value);
        }

        public ICommand PrevSelectedPageCommand
        {
            get => (ICommand)GetValue(PrevSelectedPageCommandProperty);
            set => SetValue(PrevSelectedPageCommandProperty, value);
        }

        public Breadcrumb()
        {
            InitializeComponent();
            PrevSelectedPageCommand = new RelayCommand(PrevSelectedPage);
            PrevPageCommand = new RelayCommand(PrevPage);

        }

        private void PrevSelectedPage(object parameter)
        {
            if(parameter is PageModel page)
            {
                var pageHistory = NavigationService.GetPageHistory();
                if (pageHistory.Last() != page)
                {
                    int currentPageIndex = pageHistory.IndexOf(page);
                    for (int i = pageHistory.Count-1; i > currentPageIndex; i--)
                        NavigationService.RemovePageToPageHistory(pageHistory[i]);
                    NavigationService.SetPageInSecondaryFrame();
                }
            }
        }

        private void PrevPage(object parameter)
        {
            var lastPage = NavigationService.GetPageHistory().Last();
            NavigationService.RemovePageToPageHistory(lastPage);
            NavigationService.SetPageInSecondaryFrame();
        }

    }
}

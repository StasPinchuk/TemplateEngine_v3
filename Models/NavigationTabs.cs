using System.Collections.ObjectModel;
using TemplateEngine_v3.Models.PageCollection;

namespace TemplateEngine_v3.Models
{
    public class NavigationTabs : BaseNotifyPropertyChanged
    {
        private string _title;

        public string Title
        {
            get => _title;
            set => SetValue(ref _title, value, nameof(Title));
        }

        private PageModel _page;

        public PageModel Page
        {
            get => _page;
            set => SetValue(ref _page, value, nameof(Page));
        }

        public ObservableCollection<PageModel> PageHistory { get; set; } = new();
    }
}

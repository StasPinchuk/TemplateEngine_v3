using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.UserControls;
using TemplateEngine_v3.Views.Pages;

namespace TemplateEngine_v3.VM.Pages
{
    public class TemplateEditPageVM : BaseNotifyPropertyChanged
    {
        private readonly ITemplateManager _templateManager;
        private readonly IBranchManager _branchManager;

        private ObservableCollection<PageModel> _pagesHistory = new();
        public ObservableCollection<PageModel> PagesHistory
        {
            get => _pagesHistory;
            set => SetValue(ref _pagesHistory, value, nameof(PagesHistory));
        }

        public ICommand SaveCommand { get; set; }
        public ICommand PreviewTemplateCommand { get; set; }

        public TemplateEditPageVM(ITechnologiesManager technologiesManager, ITemplateManager templateManager, IBranchManager branchManager, Frame frame)
        {
            _branchManager = branchManager;
            string pageName = MenuHistory.PageHistory.First().Title;
            _templateManager = templateManager;
            MenuHistory.SecondaryFrame = frame;
            MenuHistory.UpdateHistory += SetPageHistory;
            MenuHistory.Clear();
            var mainTemplateInfo = new PageModel(pageName, typeof(MainTemplateInfoPage), new object[] { technologiesManager, _templateManager });
            MenuHistory.NextPage(mainTemplateInfo);

            SaveCommand = new RelayCommand(Save);
            PreviewTemplateCommand = new RelayCommand(PreviewTemplate);
        }

        private bool _isUpdatingHistory;

        private void SetPageHistory()
        {
            if (_isUpdatingHistory) return;
            _isUpdatingHistory = true;
            if (PagesHistory == null)
                PagesHistory = new();
            PagesHistory.Clear();
            foreach (PageModel page in MenuHistory.VisiblePageHistory)
                PagesHistory.Add(page);

            _isUpdatingHistory = false;
        }

        private async void Save()
        {
            var dialog = new SaveChoiceDialog();
            var result = await DialogHost.Show(dialog, "RootDialog");
            bool isSave = false;

            if (result is string choice)
            {
                if (!choice.Equals("cancel"))
                    isSave = await _templateManager.SaveTemplate(choice);
            }

            if (isSave)
                MessageBox.Show("Шаблон успешно сохранен!");
        }

        private void PreviewTemplate()
        {
            var mainTemplateInfo = new PageModel("Предварительный просмотр шаблона", typeof(TemplatePreviewPage), new object[] { _templateManager, _branchManager});
            MenuHistory.NextPage(mainTemplateInfo);
        }
    }
}

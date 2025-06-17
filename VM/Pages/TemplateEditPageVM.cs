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
using TemplateEngine_v3.UserControls;
using TemplateEngine_v3.Views.Pages;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel страницы редактирования шаблона.
    /// Управляет историей страниц, сохранением и предпросмотром шаблона.
    /// </summary>
    public class TemplateEditPageVM : BaseNotifyPropertyChanged
    {
        private readonly ITemplateManager _templateManager;
        private readonly IBranchManager _branchManager;

        private ObservableCollection<PageModel> _pagesHistory = new();

        /// <summary>
        /// История страниц, отображаемая в UI.
        /// </summary>
        public ObservableCollection<PageModel> PagesHistory
        {
            get => _pagesHistory;
            set => SetValue(ref _pagesHistory, value, nameof(PagesHistory));
        }

        /// <summary>
        /// Команда сохранения шаблона.
        /// </summary>
        public ICommand SaveCommand { get; set; }

        /// <summary>
        /// Команда предварительного просмотра шаблона.
        /// </summary>
        public ICommand PreviewTemplateCommand { get; set; }

        /// <summary>
        /// Конструктор ViewModel.
        /// Инициализирует менеджеры, подписывается на обновления истории и устанавливает начальную страницу.
        /// </summary>
        /// <param name="technologiesManager">Менеджер технологических процессов.</param>
        /// <param name="templateManager">Менеджер шаблонов.</param>
        /// <param name="branchManager">Менеджер филиалов.</param>
        /// <param name="frame">Фрейм для отображения страниц.</param>
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

        /// <summary>
        /// Обновляет коллекцию истории страниц для отображения в UI.
        /// </summary>
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

        /// <summary>
        /// Асинхронно сохраняет текущий шаблон.
        /// Открывает диалог выбора способа сохранения и выводит сообщение о результате.
        /// </summary>
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

            var msg = isSave ? "Шаблон успешно сохранен!" : "Шаблон не сохранен!";
            MessageBox.Show(msg, "Сохранение шаблона");
        }

        /// <summary>
        /// Открывает страницу предварительного просмотра шаблона.
        /// </summary>
        private void PreviewTemplate()
        {
            var mainTemplateInfo = new PageModel("Предварительный просмотр шаблона", typeof(TemplatePreviewPage), new object[] { _templateManager, _branchManager });
            MenuHistory.NextPage(mainTemplateInfo);
        }
    }
}

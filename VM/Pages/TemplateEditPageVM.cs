using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
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
        /// <summary>
        /// Фрейм, в котором отображаются страницы шаблона.
        /// </summary>
        private Frame _frame;

        /// <summary>
        /// Менеджер шаблонов.
        /// </summary>
        private readonly TemplateManager _templateManager;

        /// <summary>
        /// Сервис этапов шаблона.
        /// </summary>
        private readonly TemplateStageService _stageService;

        /// <summary>
        /// Менеджер филиалов.
        /// </summary>
        private readonly BranchManager _branchManager;

        /// <summary>
        /// Коллекция с историей открытых страниц.
        /// </summary>
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
        public TemplateEditPageVM(TemplateManager templateManager, TechnologiesManager technologiesManager, BranchManager branchManager, TemplateStageService stageService, Frame frame)
        {
            _branchManager = branchManager;
            _templateManager = templateManager;
            _stageService = stageService;

            NavigationService.UpdateHistory += UpdatePageHistory;
            NavigationService.SetSecondaryFrame(frame);

            PagesHistory = NavigationService.GetPageHistory();
            if (_templateManager.GetSelectedTemplate() != null)
            {
                var mainTemplateInfo = new PageModel(_templateManager.GetSelectedTemplate().Name, typeof(MainTemplateInfoPage), new object[] { technologiesManager, _templateManager });

                NavigationService.AddPageToPageHistory(mainTemplateInfo);
                NavigationService.SetPageInSecondaryFrame();
            }

            SaveCommand = new RelayCommand(Save);
            PreviewTemplateCommand = new RelayCommand(PreviewTemplate);
        }

        private bool _isUpdatingHistory;

        /// <summary>
        /// Асинхронно сохраняет текущий шаблон.
        /// Открывает диалог выбора способа сохранения и выводит сообщение о результате.
        /// </summary>
        private async void Save()
        {
            var dialog = new SaveChoiceDialog(_templateManager, _stageService);
            var result = await DialogHost.Show(dialog, "RootDialog");
            bool isSave = false;

            if (result is string choice)
            {
                if (!choice.Equals("cancel"))
                    isSave = await _templateManager.SaveTemplate(choice, _templateManager.SelectedTemplate);
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
            NavigationService.AddPageToPageHistory(mainTemplateInfo);
            NavigationService.SetPageInSecondaryFrame();
        }

        /// <summary>
        /// Обновляет коллекцию истории страниц.
        /// Вызывается при изменении истории навигации.
        /// </summary>
        private void UpdatePageHistory()
        {
            PagesHistory = NavigationService.GetPageHistory();
            OnPropertyChanged(nameof(PagesHistory));
        }
    }
}

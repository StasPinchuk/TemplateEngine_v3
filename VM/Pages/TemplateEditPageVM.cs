using MaterialDesignThemes.Wpf;
using Microsoft.Xaml.Behaviors.Core;
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
    /// <summary>
    /// ViewModel страницы редактирования шаблона.
    /// Управляет историей страниц, сохранением и предпросмотром шаблона.
    /// </summary>
    public class TemplateEditPageVM : BaseNotifyPropertyChanged
    {
        private Frame _frame;

        private readonly TemplateManager _templateManager;
        private readonly BranchManager _branchManager;

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
        public TemplateEditPageVM(TemplateManager templateManager, TechnologiesManager technologiesManager, BranchManager branchManager, Frame frame)
        {
            _branchManager = branchManager;
            _templateManager = templateManager;

            NavigationService.UpdateHistory += UpdatePageHistory;
            NavigationService.SetSecondaryFrame(frame);

            PagesHistory = NavigationService.GetPageHistory();
            if(_templateManager.GetSelectedTemplate() != null)
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
            NavigationService.AddPageToPageHistory(mainTemplateInfo);
            NavigationService.SetPageInSecondaryFrame();
        }

        private void UpdatePageHistory()
        {
            PagesHistory = NavigationService.GetPageHistory();
            OnPropertyChanged(nameof(PagesHistory));
        }
    }
}

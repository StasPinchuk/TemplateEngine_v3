using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.PageCollection;
using TemplateEngine_v3.Models.TemplateModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Views.Pages;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel для страницы информации по основному шаблону.
    /// Управляет текущим шаблоном, отношениями шаблона, страницами и командами взаимодействия.
    /// </summary>
    public class MainTemplateInfoPageVM : BaseNotifyPropertyChanged
    {
        private readonly TechnologiesManager _technologiesManager;
        private readonly NodeManager _nodeManager;
        private readonly TemplateManager _templateManager;

        private Template _currentTemplate;

        /// <summary>
        /// Текущий выбранный шаблон.
        /// При изменении автоматически обновляет текущие отношения шаблона.
        /// </summary>
        public Template CurrentTemplate
        {
            get => _currentTemplate;
            set
            {
                SetValue(ref _currentTemplate, value, nameof(CurrentTemplate));
                if (_currentTemplate != null)
                    CurrentRelation = _currentTemplate.TemplateRelations.FirstOrDefault();
            }
        }

        private TemplateRelations _currentRelation;

        /// <summary>
        /// Текущая выбранная группа отношений шаблона.
        /// </summary>
        public TemplateRelations CurrentRelation
        {
            get => _currentRelation;
            set => SetValue(ref _currentRelation, value, nameof(CurrentRelation));
        }

        /// <summary>
        /// Контекстное меню для текстовых полей.
        /// Получается из MenuHelper текущего NodeManager.
        /// </summary>
        public ContextMenu TextBoxMenu => _nodeManager.MenuHelper.GetContextMenu() ?? new ContextMenu();

        /// <summary>
        /// Коллекция страниц, отображаемых в интерфейсе.
        /// </summary>
        public ObservableCollection<PageModel> PagesCollection { get; private set; } = new();
        public ObservableCollection<ExampleMarking> ExampleMarkings { get; set; } = new();

        /// <summary>
        /// Команда перехода на следующую страницу.
        /// </summary>
        public ICommand NextPageCommand { get; private set; }

        /// <summary>
        /// Команда установки текущих отношений шаблона.
        /// </summary>
        public ICommand SetRelationsCommand { get; private set; }

        /// <summary>
        /// Команда добавления новой группы обозначений (TemplateRelations).
        /// </summary>
        public ICommand AddDesignationCommand { get; private set; }

        /// <summary>
        /// Команда удаления выбранной группы обозначений.
        /// </summary>
        public ICommand RemoveDesignationCommand { get; private set; }

        /// <summary>
        /// Команда добавления примера обозначения.
        /// </summary>
        public ICommand AddExampleMarkingsCommand { get; private set; }

        /// <summary>
        /// Команда удаления примера обозначения.
        /// </summary>
        public ICommand RemoveExampleMarkingsCommand { get; private set; }

        /// <summary>
        /// Конструктор VM.
        /// Инициализирует менеджеры, текущий шаблон, коллекцию страниц и команды.
        /// </summary>
        /// <param name="technologiesManager">Менеджер технологий.</param>
        /// <param name="templateManager">Менеджер шаблонов.</param>
        public MainTemplateInfoPageVM(TechnologiesManager technologiesManager, TemplateManager templateManager)
        {
            _technologiesManager = technologiesManager;
            _templateManager = templateManager;
            _nodeManager = new NodeManager()
            {
                MenuHelper = _templateManager.MenuHelper,
                TableManager = templateManager.TableService
            };
            CurrentTemplate = _templateManager.GetSelectedTemplate();

            foreach(string marking in CurrentTemplate.ExampleMarkings)
            {
                ExampleMarkings.Add(new ExampleMarking() { Text = marking });
            }

            technologiesManager.CurrentTechnologies = CurrentRelation?.Technologies ?? null;
            technologiesManager.MenuHelper = _templateManager.MenuHelper;

            var pageCollection = new ObservableCollection<PageModel>()
            {
                new PageModel("Процесс изготовления", typeof(TechnologiesPage), new object[] { technologiesManager }),
                new PageModel("Состав изделия", typeof(PartsListPage), new object[] { _technologiesManager, _nodeManager, _templateManager }),
            };

            foreach (PageModel page in pageCollection)
            {
                PagesCollection.Add(page);
            }

            InitializeCommand();
        }

        /// <summary>
        /// Инициализация команд.
        /// </summary>
        private void InitializeCommand()
        {
            NextPageCommand = new RelayCommand(NextPage, CanNextPage);
            SetRelationsCommand = new RelayCommand(SetRelations);
            AddDesignationCommand = new RelayCommand(AddDesignation);
            RemoveDesignationCommand = new RelayCommand(RemoveDesignationAsync);
            AddExampleMarkingsCommand = new RelayCommand(AddExampleMarkingsAsync);
            RemoveExampleMarkingsCommand = new RelayCommand(RemoveExampleMarkingsAsync);
        }

        /// <summary>
        /// Проверяет возможность перехода на следующую страницу.
        /// </summary>
        /// <param name="parameters">Параметры команды (не используются).</param>
        /// <returns>True, если текущие отношения не равны null.</returns>
        private bool CanNextPage(object parameters)
        {
            return CurrentRelation != null;
        }

        /// <summary>
        /// Выполняет переход на следующую страницу.
        /// Обновляет текущие технологии и ноды в менеджерах.
        /// </summary>
        /// <param name="parameters">Параметры команды. Ожидается PageModel следующей страницы.</param>
        private void NextPage(object parameters)
        {
            if (parameters is PageModel nextPage)
            {
                _technologiesManager.CurrentTechnologies = CurrentRelation.Technologies;
                _nodeManager.Nodes = CurrentRelation.Nodes;
                EvaluatorManager evaluatorManager = new EvaluatorManager(_templateManager.GetSelectedTemplate(), CurrentRelation);

                _nodeManager.EvaluatorManager = evaluatorManager;

                NavigationService.AddPageToPageHistory(nextPage);
                NavigationService.SetPageInSecondaryFrame();

                nextPage.IsSelected = false;
                PagesCollection.First().IsSelected = false;
            }
        }

        /// <summary>
        /// Устанавливает текущие отношения шаблона.
        /// Обновляет менеджеры технологий и нод.
        /// </summary>
        /// <param name="parameters">Параметры команды. Ожидается TemplateRelations.</param>
        private void SetRelations(object parameters)
        {
            if (parameters is TemplateRelations relations)
            {
                CurrentRelation = relations;
                _technologiesManager.CurrentTechnologies = relations.Technologies;
                _nodeManager.Nodes = relations.Nodes;
            }
        }

        /// <summary>
        /// Добавляет новую группу обозначений в текущий шаблон.
        /// </summary>
        /// <param name="parameters">Параметры команды (не используются).</param>
        private void AddDesignation(object parameters)
        {
            CurrentTemplate.TemplateRelations.Add(new());
            UpdateContextMenu();
        }

        /// <summary>
        /// Удаляет выбранную группу обозначений из текущего шаблона.
        /// Обновляет текущие отношения и контекстное меню.
        /// </summary>
        /// <param name="parameters">Параметры команды. Ожидается TemplateRelations.</param>
        private void RemoveDesignationAsync(object parameters)
        {
            if (parameters is TemplateRelations relations)
                CurrentTemplate.TemplateRelations.Remove(relations);
            if (CurrentTemplate.TemplateRelations.Count == 0)
                CurrentRelation = null;
            else
            {
                CurrentRelation = CurrentTemplate.TemplateRelations.LastOrDefault();
            }
            UpdateContextMenu();
        }

        /// <summary>
        /// Добавляет новый пустой пример маркировки в текущий шаблон.
        /// Обновляет контекстное меню.
        /// </summary>
        /// <param name="parameters">Параметры команды (не используются).</param>
        private void AddExampleMarkingsAsync(object parameters)
        {
            ExampleMarkings.Add(new ExampleMarking());
            UpdateContextMenu();
        }

        /// <summary>
        /// Удаляет пример маркировки из текущего шаблона.
        /// Обновляет контекстное меню.
        /// </summary>
        /// <param name="parameters">Параметры команды. Ожидается строка примера маркировки.</param>
        private void RemoveExampleMarkingsAsync(object parameters)
        {
            if (parameters is string exampleMarkings)
                CurrentTemplate.ExampleMarkings.Remove(exampleMarkings);
            UpdateContextMenu();
        }

        /// <summary>
        /// Асинхронно обновляет контекстное меню через MenuHelper.
        /// </summary>
        public async void UpdateContextMenu()
        {
            await _nodeManager.MenuHelper.UpdateContextMenuAsync();
        }

        public void UpdateExampleMarking(int index, string value)
        {
            if (index <= ExampleMarkings.Count-1)
            {
                if(index > CurrentTemplate.ExampleMarkings.Count - 1)
                    CurrentTemplate.ExampleMarkings.Add(ExampleMarkings[index].Text);
                else
                    CurrentTemplate.ExampleMarkings[index] = ExampleMarkings[index].Text;
                CurrentTemplate.ProductMarkingAttributes.Clear();
                foreach (string exampleMerking in CurrentTemplate.ExampleMarkings)
                {
                    string[] markings = exampleMerking.Split(new string[] { "-", "*" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(string marking in markings)
                    {
                        CurrentTemplate.ProductMarkingAttributes.Add(marking);
                    }
                }
            }
            UpdateContextMenu();
        }

    }
}

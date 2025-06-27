using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel для страницы редактирования технологического процесса.
    /// Управляет текущим технологическим процессом, операциями и филиалами.
    /// </summary>
    public class TechnologiesPageVM : BaseNotifyPropertyChanged
    {
        private readonly ITechnologiesManager _technologiesManager;
        private readonly OperationNamesManager _operationNamesManager;
        private Technologies _currentTechnologies;

        /// <summary>
        /// Текущий технологический процесс, отображаемый и редактируемый в UI.
        /// </summary>
        public Technologies CurrentTechnologies
        {
            get => _currentTechnologies;
            set => SetValue(ref _currentTechnologies, value, nameof(CurrentTechnologies));
        }

        private BranchDivisionDetails _selectedBranch;

        /// <summary>
        /// Выбранный филиал из списка филиалов текущей операции.
        /// </summary>
        public BranchDivisionDetails SelectedBranch
        {
            get => _selectedBranch;
            set
            {
                IsBranchSelected = value != null;
                SetValue(ref _selectedBranch, value, nameof(SelectedBranch));
            }
        }

        private bool _isEnableBranchList;

        /// <summary>
        /// Флаг, определяющий, доступен ли список филиалов для выбора.
        /// </summary>
        public bool IsEnableBranchList
        {
            get => _isEnableBranchList;
            set => SetValue(ref _isEnableBranchList, value, nameof(IsEnableBranchList));
        }

        private Operation _currentOperation = new();

        /// <summary>
        /// Текущая операция в технологическом процессе.
        /// При изменении обновляет доступность списка филиалов.
        /// </summary>
        public Operation CurrentOperation
        {
            get => _currentOperation;
            set
            {
                SetValue(ref _currentOperation, value, nameof(CurrentOperation));
                IsEnableBranchList = value != null;
                SetBranchesList();
            }
        }

        private bool _isBranchSelected = false;

        public bool IsBranchSelected
        {
            get => _isBranchSelected;
            set => SetValue(ref _isBranchSelected, value, nameof(IsBranchSelected));
        }

        /// <summary>
        /// Контекстное меню для выбора названий материалов.
        /// </summary>
        public ContextMenu MaterialsNameMenu => _technologiesManager.MenuHelper?.GetContextMenu();

        /// <summary>
        /// Список доступных названий операций.
        /// </summary>
        public ObservableCollection<string> OperationNames { get; private set; } = new();

        /// <summary>
        /// Список всех технологических процессов для выбора.
        /// </summary>
        public ObservableCollection<ReferenceModelInfo> Technologies { get; private set; } = new();

        /// <summary>
        /// Список филиалов, доступных для добавления в текущую операцию.
        /// </summary>
        public ObservableCollection<ReferenceModelInfo> Branches { get; private set; } = new();

        /// <summary>
        /// Команда создания новой операции.
        /// </summary>
        public ICommand CreateNewOperationCommand { get; private set; }

        /// <summary>
        /// Команда удаления выбранной операции.
        /// </summary>
        public ICommand RemoveOperationCommand { get; private set; }

        /// <summary>
        /// Команда добавления филиала в текущую операцию.
        /// </summary>
        public ICommand SetBranchCommand { get; private set; }

        /// <summary>
        /// Команда удаления филиала из текущей операции.
        /// </summary>
        public ICommand RemoveBranchCommand { get; private set; }

        /// <summary>
        /// Команда замены текущего технологического процесса.
        /// </summary>
        public ICommand SetCurrentTechnologiesCommand { get; private set; }

        /// <summary>
        /// Команда сохранения или редактирования текущего технологического процесса.
        /// </summary>
        public ICommand SaveOrEditCurrentTechnologiesCommand { get; private set; }

        /// <summary>
        /// Конструктор ViewModel, инициализирует менеджеры и подписывается на события.
        /// </summary>
        /// <param name="technologiesManager">Менеджер работы с технологическими процессами.</param>
        public TechnologiesPageVM(ITechnologiesManager technologiesManager)
        {
            _technologiesManager = technologiesManager;
            _operationNamesManager = _technologiesManager.GetOperationNamesManager();

            _technologiesManager.CurrentTechnologiesChanged += OnCurrentTechnologiesChanged;

            CurrentTechnologies = _technologiesManager.CurrentTechnologies;
            OperationNames = new ObservableCollection<string>(_operationNamesManager.OperationNameList);
            Technologies = _technologiesManager.GetAllTechnologies();
            
            InitializeCommand();
        }

        /// <summary>
        /// Инициализирует команды для UI.
        /// </summary>
        private void InitializeCommand()
        {
            CreateNewOperationCommand = new RelayCommand(CreateNewOperation);
            RemoveOperationCommand = new RelayCommand(RemoveOperation);
            SetBranchCommand = new RelayCommand(SetBranch);
            RemoveBranchCommand = new RelayCommand(RemoveBranch);
            SetCurrentTechnologiesCommand = new RelayCommand(SetCurrentTechnologies);
            SaveOrEditCurrentTechnologiesCommand = new RelayCommand(SaveOrEditCurrentTechnologies, CanSaveOrEditCurrentTechnologies);
        }

        /// <summary>
        /// Обработчик события изменения текущего технологического процесса.
        /// Обновляет привязанное свойство.
        /// </summary>
        /// <param name="technologies">Новый технологический процесс.</param>
        private void OnCurrentTechnologiesChanged(Technologies technologies)
        {
            CurrentTechnologies = _technologiesManager.CurrentTechnologies;
            OnPropertyChanged(nameof(CurrentTechnologies));
        }

        /// <summary>
        /// Обновляет список филиалов, которые еще не добавлены в текущую операцию.
        /// </summary>
        private void SetBranchesList()
        {
            Branches.Clear();
            if (CurrentOperation != null)
            {
                foreach (var branch in _technologiesManager.Branches)
                {
                    if (!_currentOperation.BranchDivisionDetails.Any(branchDivision =>
                        branchDivision.Branch != null &&
                        branchDivision.Branch.Name == branch?.Name))
                    {
                        Branches.Add(branch);
                    }
                }
            }
        }

        /// <summary>
        /// Создает новую операцию с первым доступным названием и добавляет в текущий технологический процесс.
        /// </summary>
        private void CreateNewOperation()
        {
            var newOperation = new Operation()
            {
                Name = OperationNames.FirstOrDefault(),
            };
            CurrentTechnologies.Operations.Add(newOperation);
        }

        /// <summary>
        /// Удаляет операцию из текущего технологического процесса с подтверждением.
        /// </summary>
        /// <param name="parameter">Операция для удаления.</param>
        private void RemoveOperation(object parameter)
        {
            if (parameter is Operation operationToRemove)
            {
                var result = MessageBox.Show(
                    $"Удалить операцию \"{operationToRemove.Name}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    CurrentTechnologies.Operations.Remove(operationToRemove);
                    CurrentOperation = null;
                }
            }
        }

        /// <summary>
        /// Добавляет филиал в текущую операцию.
        /// </summary>
        /// <param name="parameter">Филиал в виде ReferenceModelInfo.</param>
        private void SetBranch(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModelInfo)
            {
                var newBranchDivision = new BranchDivisionDetails()
                {
                    Branch = new JsonSerializer().Deserialize<Branch>(referenceModelInfo.ObjectStruct)
                };

                CurrentOperation.BranchDivisionDetails.Add(newBranchDivision);
                SetBranchesList();
            }
        }

        /// <summary>
        /// Удаляет филиал из текущей операции с подтверждением.
        /// </summary>
        /// <param name="parameter">Филиал для удаления.</param>
        private void RemoveBranch(object parameter)
        {
            if (parameter is BranchDivisionDetails branchDivision)
            {
                var result = MessageBox.Show(
                    $"Удалить филиал \"{branchDivision.Branch?.Name}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    CurrentOperation.BranchDivisionDetails.Remove(branchDivision);
                    SelectedBranch = CurrentOperation.BranchDivisionDetails.LastOrDefault();
                }
            }
        }

        /// <summary>
        /// Заменяет текущий технологический процесс новым с подтверждением.
        /// </summary>
        /// <param name="parameter">Выбранный технологический процесс.</param>
        private void SetCurrentTechnologies(object parameter)
        {
            if (parameter is ReferenceModelInfo referenceModel)
            {
                var result = MessageBox.Show(
                    "Заменить текущий тех. процесс новым?",
                    "Подтверждение загрузки",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var technologies = new JsonSerializer().Deserialize<Technologies>(referenceModel.ObjectStruct);
                    CurrentTechnologies.SetValue(technologies);
                }
            }
        }

        private bool CanSaveOrEditCurrentTechnologies(object parameter)
        {
            if(CurrentTechnologies != null)
                return !string.IsNullOrEmpty(CurrentTechnologies.Name);
            return true;
        }

        /// <summary>
        /// Сохраняет или редактирует текущий технологический процесс через менеджер.
        /// </summary>
        private async void SaveOrEditCurrentTechnologies(object parameter)
        {
            var selectedTech = Technologies.FirstOrDefault(tech => tech.Id.Equals(CurrentTechnologies.Id));
            bool isSave = false;
            if (selectedTech != null)
            {
                isSave = await _technologiesManager.EditTechnologies(CurrentTechnologies);
            }
            else
            {
                isSave = await _technologiesManager.AddTechnologies(CurrentTechnologies);
            }

            if (isSave)
                MessageBox.Show("ТП сохранено");
        }
    }
}

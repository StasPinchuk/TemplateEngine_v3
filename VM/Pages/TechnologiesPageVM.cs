using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.VM.Pages
{
    public class TechnologiesPageVM : BaseNotifyPropertyChanged
    {
        private readonly ITechnologiesManager _technologiesManager;
        private readonly OperationNamesManager _operationNamesManager;
        private Technologies _currentTechnologies;
        public Technologies CurrentTechnologies
        {
            get => _currentTechnologies;
            set => SetValue(ref _currentTechnologies, value, nameof(CurrentTechnologies));
        }

        private BranchDivisionDetails _selectedBranch;
        public BranchDivisionDetails SelectedBranch
        {
            get => _selectedBranch;
            set => SetValue(ref _selectedBranch, value, nameof(SelectedBranch));
        }

        private bool _isEnableBranchList;
        public bool IsEnableBranchList
        {
            get => _isEnableBranchList;
            set => SetValue(ref _isEnableBranchList, value, nameof(IsEnableBranchList));
        }

        private Operation _currentOperation = new();
        public Operation CurrentOperation
        {
            get => _currentOperation;
            set
            {
                SetValue(ref _currentOperation, value, nameof(CurrentOperation));
                if (value == null)
                    IsEnableBranchList = false;
                else
                    IsEnableBranchList = true;
                SetBranchesList();
            }
        }

        public ContextMenu UsageConditionMenu
        {
            get => _technologiesManager.MenuHelper?.GetContextMenuCopy();
        }

        public ContextMenu MaterialsNameMenu
        {
            get => _technologiesManager.MenuHelper?.GetContextMenuCopy();
        }

        public ContextMenu ConsumptionMenu
        {
            get => _technologiesManager.MenuHelper?.GetContextMenuCopy();
        }

        public ObservableCollection<string> OperationNames { get; private set; } = new();
        public ObservableCollection<ReferenceModelInfo> Technologies { get; private set; } = new();
        public ObservableCollection<ReferenceModelInfo> Branches { get; private set; } = new();

        public ICommand CreateNewOperationCommand { get; private set; }
        public ICommand RemoveOperationCommand { get; private set; }
        public ICommand SetBranchCommand { get; private set; }
        public ICommand RemoveBranchCommand { get; private set; }
        public ICommand SetCurrentTechnologiesCommand { get; private set; }

        public TechnologiesPageVM(ITechnologiesManager technologiesManager)
        {
            _technologiesManager = technologiesManager;
            _operationNamesManager = _technologiesManager.GetOperationNamesManager();

            _technologiesManager.CurrentTechnologiesChanged += OnCurrentTechnologiesChanged;

            CurrentTechnologies = _technologiesManager.CurrentTechnologies;
            OperationNames = new ObservableCollection<string>(_operationNamesManager.OperationNameList);
            Technologies = _technologiesManager.GetAllTechnologies();

            CreateNewOperationCommand = new RelayCommand(CreateNewOperation);
            RemoveOperationCommand = new RelayCommand(RemoveOperation);
            SetBranchCommand = new RelayCommand(SetBranch);
            RemoveBranchCommand = new RelayCommand(RemoveBranch);
            SetCurrentTechnologiesCommand = new RelayCommand(SetCurrentTechnologies);
        }

        private void OnCurrentTechnologiesChanged(Technologies technologies)
        {
            CurrentTechnologies = _technologiesManager.CurrentTechnologies;
            OnPropertyChanged(nameof(CurrentTechnologies));
        }

        private void SetBranchesList()
        {
            Branches.Clear();
            if(CurrentOperation != null)
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

        private void CreateNewOperation()
        {
            var newOperation = new Operation()
            {
                Name = OperationNames.FirstOrDefault(),
            };
            CurrentTechnologies.Operations.Add(newOperation);
            newOperation = null;
        }

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


        private void SetBranch(object parameter)
        {
            if(parameter is ReferenceModelInfo referenceModelInfo)
            {
                var newBranchDivision = new BranchDivisionDetails()
                {
                    Branch = new JsonSerializer().Deserialize<Branch>(referenceModelInfo.ObjectStruct)
                };
                
                CurrentOperation.BranchDivisionDetails.Add(newBranchDivision);

                newBranchDivision = null;
                SetBranchesList();
            }
        }

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
                    technologies = null;
                }
            }
        }

    }
}

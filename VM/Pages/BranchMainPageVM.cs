using System.Windows;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.VM.Pages
{
    public class BranchMainPageVM : BaseNotifyPropertyChanged
    {
        private readonly IBranchManager _branchManager;

        private Branch _currentBranch = new();
        public Branch CurrentBranch
        {
            get => _currentBranch;
            set => SetValue(ref _currentBranch, value, nameof(CurrentBranch));
        }

        private string _buttonText = "Добавить";
        public string ButtonText
        {
            get => _buttonText;
            set => SetValue(ref _buttonText, value, nameof(ButtonText));
        }

        public ICommand ModifyCommand { get; set; }

        public BranchMainPageVM(IBranchManager branchManager, Branch branch = null)
        {
            _branchManager = branchManager;
            if (branch != null)
            {
                CurrentBranch = branch;
                ModifyCommand = new RelayCommand(Edit);
                ButtonText = "Изменить";
            }
            else
            {
                CurrentBranch = new();
                ModifyCommand = new RelayCommand(Create);
                ButtonText = "Добавить";
            }
        }

        private async void Edit()
        {
            bool IsEdit = await _branchManager.EditBranch(CurrentBranch);
            if (IsEdit)
            {
                MessageBox.Show($"Для филиала {CurrentBranch.Name} применены изменения!");
            }
        }

        private async void Create()
        {
            bool IsCreate = await _branchManager.AddBranch(CurrentBranch);
            if (IsCreate)
            {
                MessageBox.Show($"Филиал {CurrentBranch.Name} успешно добавлен!");
            }
        }
    }
}

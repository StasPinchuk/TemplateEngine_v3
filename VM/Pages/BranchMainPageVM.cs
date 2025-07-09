using System.Windows;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.VM.Pages
{
    /// <summary>
    /// ViewModel для главной страницы управления филиалом.
    /// </summary>
    public class BranchMainPageVM : BaseNotifyPropertyChanged
    {
        private readonly BranchManager _branchManager;

        private Branch _currentBranch = new();

        /// <summary>
        /// Текущий редактируемый или добавляемый филиал.
        /// </summary>
        public Branch CurrentBranch
        {
            get => _currentBranch;
            set => SetValue(ref _currentBranch, value, nameof(CurrentBranch));
        }

        private string _buttonText = "Добавить";

        /// <summary>
        /// Текст на кнопке, отображающий действие (добавить или изменить).
        /// </summary>
        public string ButtonText
        {
            get => _buttonText;
            set => SetValue(ref _buttonText, value, nameof(ButtonText));
        }

        /// <summary>
        /// Команда, выполняющая добавление или изменение филиала.
        /// </summary>
        public ICommand ModifyCommand { get; set; }

        /// <summary>
        /// Конструктор ViewModel. Устанавливает режим редактирования или добавления в зависимости от параметра <paramref name="branch"/>.
        /// </summary>
        /// <param name="branchManager">Менеджер для работы с филиалами.</param>
        /// <param name="branch">Филиал, который необходимо редактировать. Если это новый филиал — будет создан новый объект.</param>
        public BranchMainPageVM(BranchManager branchManager, Branch branch = null)
        {
            _branchManager = branchManager;
            if (!branch.Name.Equals("Новый филиал"))
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

        /// <summary>
        /// Асинхронное редактирование филиала. Показывает сообщение об успешном изменении.
        /// </summary>
        private async void Edit()
        {
            bool IsEdit = await _branchManager.EditBranch(CurrentBranch);
            if (IsEdit)
            {
                MessageBox.Show($"Для филиала {CurrentBranch.Name} применены изменения!", "Изменение филиала");
            }
        }

        /// <summary>
        /// Асинхронное добавление нового филиала. Показывает сообщение об успешном добавлении.
        /// </summary>
        private async void Create()
        {
            bool IsCreate = await _branchManager.AddBranch(CurrentBranch);
            if (IsCreate)
            {
                MessageBox.Show($"Филиал {CurrentBranch.Name} успешно добавлен!", "Добавление филиала");
            }
        }
    }
}

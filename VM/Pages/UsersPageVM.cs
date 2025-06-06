using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.UsersServices;
using TemplateEngine_v3.UserControls;

namespace TemplateEngine_v3.VM.Pages
{
    public class UsersPageVM : BaseNotifyPropertyChanged
    {
        private readonly UserManager _userManager;
        private readonly ColumnDefinition _sideBar;

        public ObservableCollection<UserModel> AlloweUsers { get; set; }

        public ICommand SetUserPermissionsCommand { get; set; }
        public ICommand DeleteAllPermissionCommand { get; set; }

        public UsersPageVM(UserManager userManager, ColumnDefinition sideBar)
        {
            _userManager = userManager;
            _sideBar = sideBar;

            AlloweUsers = new(_userManager.GetAlloweUsers());

            SetUserPermissionsCommand = new RelayCommand(SetUserPermissions);
            DeleteAllPermissionCommand = new RelayCommand(DeleteAllPermission);
        }

        private async void SetUserPermissions(object parameter)
        {
            UserModel currentUser = null;
            if (parameter is UserModel selectedUser)
            {
                currentUser = selectedUser;
            }
            var dialog = new UserPermissionChoiceDialog(_userManager, currentUser);
            var result = await DialogHost.Show(dialog, "MainDialog");

            if (result is string choice)
            {
                if (choice.Equals("closed"))
                {
                    AlloweUsers.Clear();
                    foreach (var user in _userManager.GetAlloweUsers())
                    {
                        AlloweUsers.Add(user);
                    }
                }
            }
        }

        private void DeleteAllPermission(object parameter)
        {
            if (parameter is UserModel selectedUser)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить все права у пользователя {selectedUser.FullName}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                if (!_userManager.RemoveAllosedUser(selectedUser))
                    return;

                AlloweUsers.Clear();
                foreach (var user in _userManager.GetAlloweUsers())
                {
                    AlloweUsers.Add(user);
                }
            }
        }
    }
}

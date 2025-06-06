using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.UsersServices;

namespace TemplateEngine_v3.UserControls
{


    /// <summary>
    /// Логика взаимодействия для UserPermissionChoiceDialog.xaml
    /// </summary>
    public partial class UserPermissionChoiceDialog : UserControl
    {
        public static DependencyProperty AllUsersProperty =
            DependencyProperty.Register(
                "AllUsers",
                typeof(ObservableCollection<UserModel>),
                typeof(UserPermissionChoiceDialog),
                new PropertyMetadata(new ObservableCollection<UserModel>())
            );

        public static DependencyProperty PermissionCollectionProperty =
            DependencyProperty.Register(
                "PermissionCollection",
                typeof(ObservableCollection<PermissionModel>),
                typeof(UserPermissionChoiceDialog),
                new PropertyMetadata(new ObservableCollection<PermissionModel>())
            );

        public static DependencyProperty UserPermissionsListProperty =
            DependencyProperty.Register(
                "UserPermissionsList",
                typeof(ObservableCollection<UserPermission>),
                typeof(UserPermissionChoiceDialog),
                new PropertyMetadata(new ObservableCollection<UserPermission>())
            );

        public static DependencyProperty SelectedUserProperty =
            DependencyProperty.Register(
                "SelectedUser",
                typeof(UserModel),
                typeof(UserPermissionChoiceDialog),
                new PropertyMetadata(new UserModel())
            );

        public static DependencyProperty SelPermissionGroupCommandProperty =
            DependencyProperty.Register(
                "SelPermissionGroupCommand",
                typeof(ICommand),
                typeof(UserPermissionChoiceDialog),
                new PropertyMetadata(null)
            );

        public static DependencyProperty SetPermissionCommandProperty =
            DependencyProperty.Register(
                "SetPermissionCommand",
                typeof(ICommand),
                typeof(UserPermissionChoiceDialog),
                new PropertyMetadata(null)
            );

        public static DependencyProperty AddOrEditAllowedUserCommandProperty =
            DependencyProperty.Register(
                "AddOrEditAllowedUserCommand",
                typeof(ICommand),
                typeof(UserPermissionChoiceDialog),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty UserButtonTextProperty =
            DependencyProperty.Register(
                "UserButtonText",
                typeof(string),
                typeof(ReferenceBlock),
                new PropertyMetadata("Изменить"));

        public ObservableCollection<UserModel> AllUsers
        {
            get => (ObservableCollection<UserModel>)GetValue(AllUsersProperty);
            set => SetValue(AllUsersProperty, value);
        }

        public ObservableCollection<PermissionModel> PermissionCollection
        {
            get => (ObservableCollection<PermissionModel>)GetValue(PermissionCollectionProperty);
            set => SetValue(PermissionCollectionProperty, value);
        }

        public UserModel SelectedUser
        {
            get => (UserModel)GetValue(SelectedUserProperty);
            set => SetValue(SelectedUserProperty, value);
        }

        public ObservableCollection<UserPermission> UserPermissionsList
        {

            get => (ObservableCollection<UserPermission>)GetValue(UserPermissionsListProperty);
            set => SetValue(UserPermissionsListProperty, value);
        }

        public ICommand SelPermissionGroupCommand
        {
            get => (ICommand)GetValue(SelPermissionGroupCommandProperty);
            set => SetValue(SelPermissionGroupCommandProperty, value);
        }

        public ICommand SetPermissionCommand
        {
            get => (ICommand)GetValue(SetPermissionCommandProperty);
            set => SetValue(SetPermissionCommandProperty, value);
        }

        public ICommand AddOrEditAllowedUserCommand
        {
            get => (ICommand)GetValue(AddOrEditAllowedUserCommandProperty);
            set => SetValue(AddOrEditAllowedUserCommandProperty, value);
        }

        public string UserButtonText
        {
            get => (string)GetValue(UserButtonTextProperty);
            set => SetValue(UserButtonTextProperty, value);
        }

        private UserManager _userManager;

        public UserPermissionChoiceDialog(UserManager userManager, UserModel userModel)
        {
            InitializeComponent();
            _userManager = userManager;
            AllUsers = new(userManager.GetAllUsers());
            if(userModel != null)
            {
                var matchedUser = AllUsers.FirstOrDefault(u => u.Id == userModel.Id);
                if (matchedUser != null)
                    SelectedUser = matchedUser;
                else
                {
                    AllUsers.Add(userModel);
                    SelectedUser = userModel;
                }
                UserButtonText = "Изменить";
            }
            else
            {
                SelectedUser = new();
                UserButtonText = "Добавить";
            }

            ObservableCollection<PermissionModel> permissionList = new()
            {
                new()
                {
                    PermissionPageName = "Шаблоны",
                    PermissionList = new()
                    {
                        new(){
                            Key = "Создавать шаблоны",
                            Permission = TemplatePermissions.Create,
                            IsChecked = SelectedUser.TemplatePermission.HasFlag(TemplatePermissions.Create)
                        },
                        new(){
                            Key = "Копировать шаблоны",
                            Permission = TemplatePermissions.Copy,
                            IsChecked = SelectedUser.TemplatePermission.HasFlag(TemplatePermissions.Copy)
                        },
                        new(){
                            Key = "Редактировать шаблоны",
                            Permission = TemplatePermissions.Edit,
                            IsChecked = SelectedUser.TemplatePermission.HasFlag(TemplatePermissions.Edit)
                        },
                        new(){
                            Key = "Удалять шаблоны",
                            Permission = TemplatePermissions.Delete,
                            IsChecked = SelectedUser.TemplatePermission.HasFlag(TemplatePermissions.Delete)
                        }
                    },
                    IsSelected = true
                },
                new()
                {
                    PermissionPageName = "Филиалы",
                    PermissionList = new()
                    {
                        new(){
                            Key = "Создавать филиалы",
                            Permission = BranchPermissions.Create,
                            IsChecked = SelectedUser.BranchPermission.HasFlag(BranchPermissions.Create)
                        },
                        new(){
                            Key = "Редактировать филиалы",
                            Permission = BranchPermissions.Edit,
                            IsChecked = SelectedUser.BranchPermission.HasFlag(BranchPermissions.Edit)
                        },
                        new(){
                            Key = "Копирование филиалов",
                            Permission = BranchPermissions.Copy,
                            IsChecked = SelectedUser.BranchPermission.HasFlag(BranchPermissions.Copy)
                        },
                        new(){
                            Key = "Удалять филиалы",
                            Permission = BranchPermissions.Delete,
                            IsChecked = SelectedUser.BranchPermission.HasFlag(BranchPermissions.Delete)
                        }
                    }
                },
                new()
                {
                    PermissionPageName = "ТП",
                    PermissionList = new()
                    {
                        new(){
                            Key = "Создавать тп",
                            Permission = TechnologiesPermissions.Create,
                            IsChecked = SelectedUser.TechnologiesPermission.HasFlag(TechnologiesPermissions.Create)
                        },
                        new(){
                            Key = "Редактировать тп",
                            Permission = TechnologiesPermissions.Edit,
                            IsChecked = SelectedUser.TechnologiesPermission.HasFlag(TechnologiesPermissions.Edit)
                        },
                        new(){
                            Key = "Копировать тп",
                            Permission = TechnologiesPermissions.Copy,
                            IsChecked = SelectedUser.TechnologiesPermission.HasFlag(TechnologiesPermissions.Copy)
                        },
                        new(){
                            Key = "Удалять тп",
                            Permission = TechnologiesPermissions.Delete,
                            IsChecked = SelectedUser.TechnologiesPermission.HasFlag(TechnologiesPermissions.Delete)
                        }
                    }
                }
            };

            PermissionCollection = permissionList;
            UserPermissionsList = new(PermissionCollection.First().PermissionList);

            SelPermissionGroupCommand = new RelayCommand(SelPermisionGroup);
            SetPermissionCommand = new RelayCommand(SetPermission);
            AddOrEditAllowedUserCommand = new RelayCommand(AddOrEditAllowedUser);
        }

        private void SelPermisionGroup(object parameter)
        {
            if (parameter is PermissionModel permissionModel)
            {
                UserPermissionsList = new(permissionModel.PermissionList);
            }
        }

        private void SetPermission(object parameter)
        {
            if (parameter is UserPermission permission)
            {
                if (permission.Permission is TemplatePermissions templatePermission)
                {
                    if (permission.IsChecked)
                        SelectedUser.TemplatePermission |= templatePermission;
                    else
                        SelectedUser.TemplatePermission &= ~templatePermission;
                }
                if (permission.Permission is BranchPermissions branchPermissions)
                {
                    if (permission.IsChecked)
                        SelectedUser.BranchPermission |= branchPermissions;
                    else
                        SelectedUser.BranchPermission &= ~branchPermissions;
                }
                if (permission.Permission is TechnologiesPermissions techProcessPermissions)
                {
                    if (permission.IsChecked)
                        SelectedUser.TechnologiesPermission |= techProcessPermissions;
                    else
                        SelectedUser.TechnologiesPermission &= ~techProcessPermissions;
                }

            }
        }

        private void AddOrEditAllowedUser(object parameter)
        {
            if (SelectedUser.IsAdmin == false && SelectedUser.TemplatePermission == TemplatePermissions.None &&
                SelectedUser.BranchPermission == BranchPermissions.None && SelectedUser.TechnologiesPermission == TechnologiesPermissions.None)
                return;

            var currentUser = _userManager.GetAlloweUsers().FirstOrDefault(user => user.FullName.Equals(SelectedUser.FullName));
            if (currentUser == null)
                _userManager.AddAllowedUser(SelectedUser);
            else
                _userManager.EditAllowedUser(SelectedUser);
        }
    }
}

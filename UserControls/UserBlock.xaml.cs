using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.TemplateModels;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для UserBlock.xaml
    /// </summary>
    public partial class UserBlock : UserControl
    {
        public static DependencyProperty UserProperty =
            DependencyProperty.Register(
                "User",
                typeof(UserModel),
                typeof(UserBlock),
                new PropertyMetadata(default, OnSetUser)
                );

        public static DependencyProperty PermissionsTextProperty =
            DependencyProperty.Register(
                "PermissionsText",
                typeof(string),
                typeof(UserBlock),
                new PropertyMetadata("Нет прав")
                );

        public static DependencyProperty MyPopupProperty =
            DependencyProperty.Register(
                "MyPopup",
                typeof(Popup),
                typeof(UserBlock),
                new PropertyMetadata(default)
                );

        public static DependencyProperty SelectUserCommandProperty =
            DependencyProperty.Register(
                "SelectUserCommand",
                typeof(ICommand),
                typeof(UserBlock),
                new PropertyMetadata(default)
                );

        public static DependencyProperty DeleteAllPermissionCommandProperty =
            DependencyProperty.Register(
                "DeleteAllPermissionCommand",
                typeof(ICommand),
                typeof(UserBlock),
                new PropertyMetadata(default)
                );

        public UserModel User
        {
            get => (UserModel)GetValue(UserProperty);
            set => SetValue(UserProperty, value);
        }

        public string PermissionsText
        {
            get => (string)GetValue(PermissionsTextProperty);
            set => SetValue(PermissionsTextProperty, value);
        }

        public Popup MyPopup
        {
            get => (Popup)GetValue(MyPopupProperty);
            set => SetValue(MyPopupProperty, value);
        }

        public ICommand SelectUserCommand
        {
            get => (ICommand)GetValue(SelectUserCommandProperty);
            set => SetValue(SelectUserCommandProperty, value);
        }

        public ICommand DeleteAllPermissionCommand
        {
            get => (ICommand)GetValue(DeleteAllPermissionCommandProperty);
            set => SetValue(DeleteAllPermissionCommandProperty, value);
        }

        public UserBlock()
        {
            InitializeComponent();
        }

        private static void OnSetUser(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as UserBlock;
            if (control != null)
            {
                control.PermissionsText = string.Empty;
                control.CheckPermissions(control.User);
            }
        }

        public void CheckPermissions<T>(T obj) where T : class
        {
            PermissionsText = string.Empty;
            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                if (prop.PropertyType.IsEnum && prop.PropertyType.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    var enumValue = (Enum)prop.GetValue(obj);

                    foreach (var value in Enum.GetValues(enumValue.GetType()))
                    {
                        var flag = (Enum)value;
                        if (enumValue.HasFlag(flag) && !flag.Equals(Enum.Parse(enumValue.GetType(), "None")))
                        {
                            string permissionString = SetPermissionType(enumValue, Enum.GetName(enumValue.GetType(), flag));
                            if(!string.IsNullOrEmpty(permissionString))
                                PermissionsText += $"- {permissionString}\n";
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(PermissionsText))
            {
                PermissionsText = "Нет прав";
            }

        }

        private string SetPermissionType(Enum enumValue, string permissionName)
        {
            if (enumValue is TemplatePermissions template)
            {
                return permissionName switch
                {
                    "Edit" => "Право на редактирование шаблонов",
                    "Delete" => "Право на удаление шаблонов",
                    "Create" => "Право на создание шаблонов",
                    "Copy" => "Право на копирование шаблонов",
                    _ => string.Empty,
                };
            }
            else if (enumValue is BranchPermissions branch)
            {
                return permissionName switch
                {
                    "Edit" => "Право на редактирование филиалов",
                    "Delete" => "Право на удаление филиалов",
                    "Create" => "Право на создание филиалов",
                    "Copy" => "Право на копирование филиалов",
                    _ => string.Empty,
                };
            }
            else if (enumValue is TechnologiesPermissions techProcess)
            {
                return permissionName switch
                {
                    "Edit" => "Право на редактирование тп",
                    "Delete" => "Право на удаление тп",
                    "Create" => "Право на создание тп",
                    "Copy" => "Право на копирование тп",
                    _ => string.Empty,
                };
            }
            else
            {
                return "Неизвестное перечисление";
            }
        }
    }
}

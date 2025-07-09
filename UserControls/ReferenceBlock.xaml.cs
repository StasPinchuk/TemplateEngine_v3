using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ReferenceBlock.xaml
    /// </summary>
    public partial class ReferenceBlock : UserControl
    {
        public static DependencyProperty CurrentReferenceModelProperty =
            DependencyProperty.Register(
                    "CurrentReferenceModel",
                    typeof(ReferenceModelInfo),
                    typeof(ReferenceBlock),
                    new PropertyMetadata(null, OnSetReferenceObject)
                );

        public static DependencyProperty CurrentPermissionProperty =
            DependencyProperty.Register(
                    "CurrentPermission",
                    typeof(object),
                    typeof(ReferenceBlock),
                    new PropertyMetadata(null, OnSetCurrentPermission)
                );

        public static readonly DependencyProperty NewIndicateProperty =
            DependencyProperty.Register(
                "NewIndicate",
                typeof(Visibility),
                typeof(ReferenceBlock),
                new PropertyMetadata(Visibility.Hidden));

        public static readonly DependencyProperty EditIndicateProperty =
            DependencyProperty.Register(
                "EditIndicate",
                typeof(Visibility),
                typeof(ReferenceBlock),
                new PropertyMetadata(Visibility.Hidden));

        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(
                "RemoveCommand",
                typeof(ICommand),
                typeof(ReferenceBlock),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CloneCommandProperty =
            DependencyProperty.Register(
                "CloneCommand",
                typeof(ICommand),
                typeof(ReferenceBlock),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register(
                "EditCommand",
                typeof(ICommand),
                typeof(ReferenceBlock),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register(
                "IsLocked",
                typeof(bool),
                typeof(ReferenceBlock),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanCopyProperty =
            DependencyProperty.Register(
                "CanCopy",
                typeof(bool),
                typeof(ReferenceBlock),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanEditProperty =
            DependencyProperty.Register(
                "CanEdit",
                typeof(bool),
                typeof(ReferenceBlock),
                new PropertyMetadata(false));

        public static readonly DependencyProperty CanRemoveProperty =
            DependencyProperty.Register(
                "CanRemove",
                typeof(bool),
                typeof(ReferenceBlock),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(
                "ButtonText",
                typeof(string),
                typeof(ReferenceBlock),
                new PropertyMetadata("Изменить"));

        public static readonly DependencyProperty BlockVisibilityProperty =
            DependencyProperty.Register(
                "BlockVisibility",
                typeof(Visibility),
                typeof(ReferenceBlock),
                new PropertyMetadata(Visibility.Visible));

        public ReferenceModelInfo CurrentReferenceModel
        {
            get => (ReferenceModelInfo)GetValue(CurrentReferenceModelProperty);
            set => SetValue(CurrentReferenceModelProperty, value);
        }

        public object CurrentPermission
        {
            get => (object)GetValue(CurrentPermissionProperty);
            set => SetValue(CurrentPermissionProperty, value);
        }

        public Visibility NewIndicate
        {
            get => (Visibility)GetValue(NewIndicateProperty);
            set => SetValue(NewIndicateProperty, value);
        }

        public Visibility EditIndicate
        {
            get => (Visibility)GetValue(EditIndicateProperty);
            set => SetValue(EditIndicateProperty, value);
        }

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public ICommand CloneCommand
        {
            get => (ICommand)GetValue(CloneCommandProperty);
            set => SetValue(CloneCommandProperty, value);
        }

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public bool CanCopy
        {
            get => (bool)GetValue(CanCopyProperty);
            set => SetValue(CanCopyProperty, value);
        }

        public bool CanEdit
        {
            get => (bool)GetValue(CanEditProperty);
            set => SetValue(CanEditProperty, value);
        }

        public bool CanRemove
        {
            get => (bool)GetValue(CanRemoveProperty);
            set => SetValue(CanRemoveProperty, value);
        }

        public bool IsLocked
        {
            get => (bool)GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        public Visibility BlockVisibility
        {
            get => (Visibility)GetValue(BlockVisibilityProperty);
            set => SetValue(BlockVisibilityProperty, value);
        }

        public ReferenceBlock()
        {
            InitializeComponent();
        }

        private static void OnSetReferenceObject(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ReferenceBlock)d;
            if (control != null)
            {
                if (e.NewValue is ReferenceModelInfo reference)
                {
                    if(NavigationService.GetTabs().Any(tab => tab.Title.Equals(reference.Name)))
                    {
                        control.BlockVisibility = Visibility.Collapsed;
                    }
                    if (reference.CreateDate.Date == DateTime.Now.Date)
                        control.NewIndicate = Visibility.Visible;
                    if (reference.LastEditDate.Date == DateTime.Now.Date && reference.LastEditDate != reference.CreateDate)
                        control.EditIndicate = Visibility.Visible;

                    if (reference.Type.Name.Equals("Корзина"))
                        control.ButtonText = "Восстановить";

                    control.IsLocked = reference.IsLocked;
                }
                else
                    control.NewIndicate = Visibility.Collapsed;
            }
        }

        private static void OnSetCurrentPermission(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ReferenceBlock)d;
            if (control != null)
            {
                if (e.NewValue is TemplatePermissions templatePermission)
                {
                    control.CanCopy = (templatePermission.HasFlag(TemplatePermissions.All) || templatePermission.HasFlag(TemplatePermissions.Copy)) && !control.IsLocked;
                    control.CanEdit = (templatePermission == TemplatePermissions.All || templatePermission.HasFlag(TemplatePermissions.Edit)) && !control.IsLocked;
                    control.CanRemove = (templatePermission == TemplatePermissions.All || templatePermission.HasFlag(TemplatePermissions.Delete)) && !control.IsLocked;
                }
                if (e.NewValue is BranchPermissions branchPermissions)
                {
                    control.CanCopy = (branchPermissions == BranchPermissions.All || branchPermissions.HasFlag(BranchPermissions.Copy)) && !control.IsLocked;
                    control.CanEdit = (branchPermissions == BranchPermissions.All || branchPermissions.HasFlag(BranchPermissions.Edit)) && !control.IsLocked;
                    control.CanRemove = (branchPermissions == BranchPermissions.All || branchPermissions.HasFlag(BranchPermissions.Delete)) && !control.IsLocked;
                }
                if (e.NewValue is TechnologiesPermissions technologiesPermissions)
                {
                    control.CanCopy = (technologiesPermissions == TechnologiesPermissions.All || technologiesPermissions.HasFlag(TechnologiesPermissions.Copy)) && !control.IsLocked;
                    control.CanEdit = (technologiesPermissions == TechnologiesPermissions.All || technologiesPermissions.HasFlag(TechnologiesPermissions.Edit)) && !control.IsLocked;
                    control.CanRemove = (technologiesPermissions == TechnologiesPermissions.All || technologiesPermissions.HasFlag(TechnologiesPermissions.Delete)) && !control.IsLocked;
                }
            }
        }
    }
}

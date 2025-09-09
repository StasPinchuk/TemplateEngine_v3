using MaterialDesignThemes.Wpf;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Models.TemplateModels;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;

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

        public static readonly DependencyProperty ButtonIconProperty =
            DependencyProperty.Register(
                "ButtonIcon",
                typeof(PackIconKind),
                typeof(ReferenceBlock),
                new PropertyMetadata(PackIconKind.Edit));

        public static readonly DependencyProperty ButtonToolTipProperty =
            DependencyProperty.Register(
                "ButtonToolTip",
                typeof(string),
                typeof(ReferenceBlock),
                new PropertyMetadata("Изменить"));

        public static readonly DependencyProperty ButtonRemoveToolTipProperty =
            DependencyProperty.Register(
                "ButtonRemoveToolTip",
                typeof(string),
                typeof(ReferenceBlock),
                new PropertyMetadata("Архивировать"));

        public static readonly DependencyProperty ButtonRemoveIconProperty =
            DependencyProperty.Register(
                "ButtonRemoveIcon",
                typeof(PackIconKind),
                typeof(ReferenceBlock),
                new PropertyMetadata(PackIconKind.ArchiveAdd));

        public static readonly DependencyProperty StageServiceProperty =
            DependencyProperty.Register(
                "StageService",
                typeof(TemplateStageService),
                typeof(ReferenceBlock),
                new PropertyMetadata(null, OnSetStageService));

        public static readonly DependencyProperty BlockVisibilityProperty =
            DependencyProperty.Register(
                "BlockVisibility",
                typeof(Visibility),
                typeof(ReferenceBlock),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty CopyButtonVisibilityProperty =
            DependencyProperty.Register(
                "CopyButtonVisibility",
                typeof(Visibility),
                typeof(ReferenceBlock),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty StageProperty =
            DependencyProperty.Register(
                "Stage",
                typeof(StageModel),
                typeof(ReferenceBlock),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ColumnsCountProperty =
            DependencyProperty.Register(
                "ColumnsCount",
                typeof(int),
                typeof(ReferenceBlock),
                new PropertyMetadata(3));

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

        public PackIconKind ButtonIcon
        {
            get => (PackIconKind)GetValue(ButtonIconProperty);
            set => SetValue(ButtonIconProperty, value);
        }

        public string ButtonToolTip
        {
            get => (string)GetValue(ButtonToolTipProperty);
            set => SetValue(ButtonToolTipProperty, value);
        }

        public PackIconKind ButtonRemoveIcon
        {
            get => (PackIconKind)GetValue(ButtonRemoveIconProperty);
            set => SetValue(ButtonRemoveIconProperty, value);
        }

        public Visibility CopyButtonVisibility
        {
            get => (Visibility)GetValue(CopyButtonVisibilityProperty);
            set => SetValue(CopyButtonVisibilityProperty, value);
        }

        public string ButtonRemoveToolTip
        {
            get => (string)GetValue(ButtonRemoveToolTipProperty);
            set => SetValue(ButtonRemoveToolTipProperty, value);
        }

        public Visibility BlockVisibility
        {
            get => (Visibility)GetValue(BlockVisibilityProperty);
            set => SetValue(BlockVisibilityProperty, value);
        }

        public TemplateStageService StageService
        {
            get => (TemplateStageService)GetValue(StageServiceProperty);
            set => SetValue(StageServiceProperty, value);
        }

        public StageModel Stage
        {
            get => (StageModel)GetValue(StageProperty);
            set => SetValue(StageProperty, value);
        }

        public int ColumnsCount
        {
            get => (int)GetValue(ColumnsCountProperty);
            set => SetValue(ColumnsCountProperty, value);
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
                    if (NavigationService.GetTabs().Any(tab => tab.Title.Equals(reference.Name)))
                    {
                        control.BlockVisibility = Visibility.Collapsed;
                    }

                    if (reference.Type.Name.Equals("Корзина"))
                    {
                        control.ButtonIcon = PackIconKind.BackupRestore;
                        control.ButtonToolTip = "Восстановить";
                    }

                }
            }
        }

        private static void OnSetCurrentPermission(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ReferenceBlock)d;
            if (control != null)
            {
                if (e.NewValue is TemplatePermissions templatePermission)
                {
                    control.CanCopy = (templatePermission.HasFlag(TemplatePermissions.All) || templatePermission.HasFlag(TemplatePermissions.Copy));
                    control.CanEdit = (templatePermission == TemplatePermissions.All || templatePermission.HasFlag(TemplatePermissions.Edit));
                    control.CanRemove = (templatePermission == TemplatePermissions.All || templatePermission.HasFlag(TemplatePermissions.Delete));
                }
                if (e.NewValue is BranchPermissions branchPermissions)
                {
                    control.CanCopy = (branchPermissions == BranchPermissions.All || branchPermissions.HasFlag(BranchPermissions.Copy));
                    control.CanEdit = (branchPermissions == BranchPermissions.All || branchPermissions.HasFlag(BranchPermissions.Edit));
                    control.CanRemove = (branchPermissions == BranchPermissions.All || branchPermissions.HasFlag(BranchPermissions.Delete));
                }
                if (e.NewValue is TechnologiesPermissions technologiesPermissions)
                {
                    control.CanCopy = (technologiesPermissions == TechnologiesPermissions.All || technologiesPermissions.HasFlag(TechnologiesPermissions.Copy));
                    control.CanEdit = (technologiesPermissions == TechnologiesPermissions.All || technologiesPermissions.HasFlag(TechnologiesPermissions.Edit));
                    control.CanRemove = (technologiesPermissions == TechnologiesPermissions.All || technologiesPermissions.HasFlag(TechnologiesPermissions.Delete));
                }
            }
        }

        private static void OnSetStageService(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ReferenceBlock)d;
            if (control != null)
            {
                if (e.NewValue != null)
                {
                    TemplateStageService stageService = e.NewValue as TemplateStageService;

                    var stageId = control.CurrentReferenceModel.Stage;
                    var stage = stageService.StageList.FirstOrDefault(st => st.ID.Equals(stageId));

                    if (stage != null)
                    {
                        control.Stage = stage;


                        bool canRemove = stage.StageType == StatusType.Archive ||
                                         control.CurrentReferenceModel.Type.ToString() == "ТП" ||
                                         control.CurrentReferenceModel.Type.ToString() == "Филиалы";

                        control.ButtonRemoveIcon = canRemove ? PackIconKind.TrashCan : PackIconKind.ArchiveAdd;
                        control.ButtonRemoveToolTip = canRemove ? "Удалить" : "Архивировать";

                        control.CopyButtonVisibility = stage.StageType == StatusType.Archive ? Visibility.Collapsed : Visibility.Visible;
                        control.ColumnsCount = stage.StageType == StatusType.Archive ? 2 : 3;
                    }
                    else
                    {
                        stage = stageService.StageList.FirstOrDefault(st => st.StageType == StatusType.Final);

                        if (stage != null)
                            control.Stage = stage;

                        bool canRemove = stage.StageType == StatusType.Archive ||
                                         control.CurrentReferenceModel.Type.ToString() == "ТП" ||
                                         control.CurrentReferenceModel.Type.ToString() == "Филиалы";

                        control.ButtonRemoveIcon = canRemove ? PackIconKind.TrashCan : PackIconKind.ArchiveAdd;
                        control.ButtonRemoveToolTip = canRemove ? "Удалить" : "Архивировать";

                        control.CopyButtonVisibility = stage.StageType == StatusType.Archive ? Visibility.Collapsed : Visibility.Visible;
                        control.ColumnsCount = stage.StageType == StatusType.Archive ? 2 : 3;
                    }
                }
            }
        }
    }
}

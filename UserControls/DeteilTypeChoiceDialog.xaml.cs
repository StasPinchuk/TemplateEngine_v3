using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для DeteilTypeChoiceDialog.xaml
    /// </summary>
    public partial class DeteilTypeChoiceDialog : UserControl
    {
        public static DependencyProperty DeteilTypeListProperty =
            DependencyProperty.Register(
                "DeteilTypeList",
                typeof(ObservableCollection<string>),
                typeof(DeteilTypeChoiceDialog),
                new PropertyMetadata(new ObservableCollection<string>())
            );

        public static DependencyProperty DeteilTypyProperty =
            DependencyProperty.Register(
                "DeteilType",
                typeof(string),
                typeof(DeteilTypeChoiceDialog),
                new PropertyMetadata(string.Empty)
            );

        public static DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(
                "ButtonText",
                typeof(string),
                typeof(DeteilTypeChoiceDialog),
                new PropertyMetadata("Добавить")
            );

        public static readonly DependencyProperty ModifyTypeCommandProperty =
            DependencyProperty.Register(
                "ModifyTypeCommand",
                typeof(ICommand),
                typeof(DeteilTypeChoiceDialog),
                new PropertyMetadata(null)
            );

        public static DependencyProperty RemoveTypeCommandProperty =
            DependencyProperty.Register(
                "RemoveTypeCommand",
                typeof(ICommand),
                typeof(DeteilTypeChoiceDialog),
                new PropertyMetadata(null)
            );

        public static DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(
                "CancelCommand",
                typeof(ICommand),
                typeof(DeteilTypeChoiceDialog),
                new PropertyMetadata(null)
            );

        public ObservableCollection<string> DeteilTypeList
        {
            get => (ObservableCollection<string>)GetValue(DeteilTypeListProperty);
            set => SetValue(DeteilTypeListProperty, value);
        }

        public string DeteilType
        {
            get => (string)GetValue(DeteilTypyProperty);
            set => SetValue(DeteilTypyProperty, value);
        }

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        public ICommand ModifyTypeCommand
        {
            get => (ICommand)GetValue(ModifyTypeCommandProperty);
            set => SetValue(ModifyTypeCommandProperty, value);
        }

        public ICommand RemoveTypeCommand
        {
            get => (ICommand)GetValue(RemoveTypeCommandProperty);
            set => SetValue(RemoveTypeCommandProperty, value);
        }

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        string _lastTypeName = string.Empty;

        public DeteilTypeChoiceDialog()
        {
            InitializeComponent();

            SetValue(DeteilTypeListProperty, NodeTypeManager.NodeTypes);

            ModifyTypeCommand = new RelayCommand(AddNewType, CanAddNewType);
            RemoveTypeCommand = new RelayCommand(RemoveType, CanRemoveType);
            CancelCommand = new RelayCommand(Cancel, CanCancel);
        }

        private bool CanAddNewType()
        {
            return !string.IsNullOrWhiteSpace(DeteilType);
        }

        private void AddNewType()
        {
            bool IsAdd = NodeTypeManager.AddNodeType(DeteilType);
            if (IsAdd)
            {
                SetValue(DeteilTypyProperty, string.Empty);
                SetValue(DeteilTypeListProperty, NodeTypeManager.NodeTypes);
            }
        }

        private bool CanEditType()
        {
            return !string.IsNullOrWhiteSpace(DeteilType);
        }

        private void EditType()
        {
            bool IsEdit = NodeTypeManager.EditNodeType(_lastTypeName, DeteilType);
            if (IsEdit)
            {
                SetValue(DeteilTypyProperty, string.Empty);
                SetValue(DeteilTypeListProperty, NodeTypeManager.NodeTypes);
                SetValue(ButtonTextProperty, "Добавить");
                ModifyTypeCommand = new RelayCommand(AddNewType, CanAddNewType);
            }
        }

        private bool CanRemoveType()
        {
            return !string.IsNullOrWhiteSpace(DeteilType);
        }

        private void RemoveType()
        {
            bool IsRemove = NodeTypeManager.RemoveNodeType(DeteilType);
            if (IsRemove)
            {
                SetValue(DeteilTypyProperty, string.Empty);
                SetValue(DeteilTypeListProperty, NodeTypeManager.NodeTypes);
                SetValue(ButtonTextProperty, "Добавить");
                ModifyTypeCommand = new RelayCommand(AddNewType, CanAddNewType);
            }
        }

        private bool CanCancel()
        {
            return !string.IsNullOrWhiteSpace(DeteilType);
        }

        private void Cancel()
        {
            SetValue(DeteilTypyProperty, string.Empty);
        }

        private void ListBox_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is string selectedType)
            {
                _lastTypeName = selectedType;
                SetValue(DeteilTypyProperty, selectedType);
                SetValue(ButtonTextProperty, "Изменить");
                ModifyTypeCommand = new RelayCommand(EditType, CanEditType);
            }
        }
    }
}

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TemplateEngine_v3.Command;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для TemplateStageChoiceDialog.xaml
    /// </summary>
    public partial class TemplateStageChoiceDialog : UserControl
    {
        public ObservableCollection<Brush> StatusColors { get; set; } = new();
        public ObservableCollection<string> IconOptions { get; set; } = new() { "📝", "📄", "⚙️", "✅" };

        public string StatusName { get; set; }
        public string SelectedIcon { get; set; }

        public Color IconColor { get; set; } = Colors.Black;
        public Brush IconBrush => new SolidColorBrush(IconColor);

        public Color TempColor { get; set; }
        public bool IsColorPickerOpen { get; set; }

        public Brush SelectedColor => StatusColors.LastOrDefault() ?? Brushes.Gray;

        public ICommand OpenColorPickerCommand => new RelayCommand(() => IsColorPickerOpen = true);
        public ICommand CancelColorPickerCommand => new RelayCommand(() => IsColorPickerOpen = false);
        public ICommand ConfirmColorPickerCommand => new RelayCommand(() =>
        {
            StatusColors.Add(new SolidColorBrush(TempColor));
            IsColorPickerOpen = false;
        });

        public TemplateStageChoiceDialog()
        {
            InitializeComponent();
        }
    }
}

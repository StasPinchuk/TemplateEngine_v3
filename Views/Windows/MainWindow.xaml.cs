using System.Windows;
using TemplateEngine_v3.Services.ServerServices;
using TemplateEngine_v3.VM.Windows;

namespace TemplateEngine_v3.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(ServerManager serverManager)
        {
            InitializeComponent();
            DataContext = new MainWindowVM(serverManager.ReferenceManager, MainFrame, SideBar);
        }
    }
}

using System.Windows;
using TemplateEngine_v3.Services.ServerServices;
using TemplateEngine_v3.VM.Windows;

namespace TemplateEngine_v3.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        private readonly SingInVM _singInVM;

        public SignInWindow(ServerManager serverManager)
        {
            InitializeComponent();
            _singInVM = new(serverManager, this);
            DataContext = _singInVM;
        }
    }
}

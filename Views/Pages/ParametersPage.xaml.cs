using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ParametersPage.xaml
    /// </summary>
    public partial class ParametersPage : Page
    {
        public ParametersPage(INodeManager nodeManager)
        {
            InitializeComponent(); 

            DataContext = new ParametersPageVM(nodeManager);
        }
    }
}

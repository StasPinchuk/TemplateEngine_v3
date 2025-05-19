using ICSharpCode.AvalonEdit;
using System.Windows.Controls;
using System.Windows.Media;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.VM.Pages;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для FormulasAndTermsPage.xaml
    /// </summary>
    public partial class FormulasAndTermsPage : Page
    {
        public FormulasAndTermsPage(INodeManager nodeManager)
        {
            InitializeComponent();

            DataContext = new FormulasAndTermsPageVM(nodeManager);
        }
    }
}

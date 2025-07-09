using System.Windows.Controls;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TemplateEditPage.xaml
    /// </summary>
    public partial class TemplateEditPage : Page
    {
        public TemplateEditPage(TemplateManager templateManager, TechnologiesManager technologiesManager, BranchManager branchManager)
        {
            InitializeComponent();
            DataContext = new TemplateEditPageVM(templateManager, technologiesManager, branchManager, TemplateFrame);
        }
    }
}

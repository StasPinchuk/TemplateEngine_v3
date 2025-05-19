using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TemplateEditPage.xaml
    /// </summary>
    public partial class TemplateEditPage : Page
    {
        public TemplateEditPage(ITechnologiesManager technologiesManager, ITemplateManager templateManager)
        {
            InitializeComponent();
            DataContext = new TemplateEditPageVM(technologiesManager, templateManager, TemplateFrame);
        }
    }
}

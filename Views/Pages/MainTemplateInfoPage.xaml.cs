using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainTemplateInfoPage.xaml
    /// </summary>
    public partial class MainTemplateInfoPage : Page
    {
        public MainTemplateInfoPage(ITechnologiesManager technologiesManager, ITemplateManager templateManager)
        {
            InitializeComponent();
            DataContext = new MainTemplateInfoPageVM(technologiesManager, templateManager);
        }
    }
}

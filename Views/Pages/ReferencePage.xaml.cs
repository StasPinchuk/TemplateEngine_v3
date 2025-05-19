using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TemplatePage.xaml
    /// </summary>
    public partial class ReferencePage : Page
    {
        private ReferencePageVM vm;

        public ReferencePage(IBranchManager branchManager, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(branchManager, sideBar);
            DataContext = vm;
        }

        public ReferencePage(ITechnologiesManager technologiesManager, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(technologiesManager, sideBar);
            DataContext = vm;
        }

        public ReferencePage(ITemplateManager templateManager, IBranchManager branchManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(templateManager, branchManager, templateClass, sideBar);
            DataContext = vm;
        }

        public ReferencePage(ITemplateManager templateManager, ITechnologiesManager technologiesManager, IBranchManager branchManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(templateManager, technologiesManager, branchManager, templateClass, sideBar);
            DataContext = vm;
        }
    }
}

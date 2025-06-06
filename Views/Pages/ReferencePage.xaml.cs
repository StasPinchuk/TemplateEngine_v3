using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.UsersServices;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TemplatePage.xaml
    /// </summary>
    public partial class ReferencePage : Page
    {
        private ReferencePageVM vm;

        public ReferencePage(IBranchManager branchManager, UserManager userManager, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(branchManager, userManager, sideBar);
            DataContext = vm;
        }

        public ReferencePage(ITechnologiesManager technologiesManager, UserManager userManager, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(technologiesManager, userManager, sideBar);
            DataContext = vm;
        }

        public ReferencePage(ITemplateManager templateManager, IBranchManager branchManager, UserManager userManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(templateManager, branchManager, userManager, templateClass, sideBar);
            DataContext = vm;
        }

        public ReferencePage(ITemplateManager templateManager, ITechnologiesManager technologiesManager, IBranchManager branchManager, UserManager userManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(templateManager, technologiesManager, branchManager, userManager, templateClass, sideBar);
            DataContext = vm;
        }
    }
}

using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Services;
using TemplateEngine_v3.Services.ReferenceServices;
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

        public ReferencePage(BranchManager branchManager, UserManager userManager, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(branchManager, userManager, sideBar);
            DataContext = vm;
        }

        public ReferencePage(TechnologiesManager technologiesManager, UserManager userManager, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(technologiesManager, userManager, sideBar);
            DataContext = vm;
        }

        public ReferencePage(TemplateManager templateManager, BranchManager branchManager, UserManager userManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(templateManager, branchManager, userManager, templateClass, sideBar);
            DataContext = vm;
        }

        public ReferencePage(TemplateManager templateManager, TechnologiesManager technologiesManager, BranchManager branchManager, UserManager userManager, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(templateManager, technologiesManager, branchManager, userManager, templateClass, sideBar);
            DataContext = vm;
        }
    }
}

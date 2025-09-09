using System.Windows.Controls;
using TemplateEngine_v3.Models;
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

        public ReferencePage(BranchManager branchManager, UserManager userManager, TemplateStageService stageService, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(branchManager, userManager, stageService, sideBar);
            DataContext = vm;
        }

        public ReferencePage(TechnologiesManager technologiesManager, UserManager userManager, TemplateStageService stageService, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(technologiesManager, userManager, stageService, sideBar);
            DataContext = vm;
        }

        public ReferencePage(TemplateManager templateManager, BranchManager branchManager, UserManager userManager, TemplateStageService stageService, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(templateManager, branchManager, userManager, templateClass, stageService, sideBar);
            DataContext = vm;
        }

        public ReferencePage(TemplateManager templateManager, TechnologiesManager technologiesManager, BranchManager branchManager, UserManager userManager, TemplateStageService stageService, TemplateClass templateClass, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new ReferencePageVM(templateManager, technologiesManager, branchManager, userManager, templateClass, stageService, sideBar);
            DataContext = vm;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                vm.SearchReferenceModel(textBox.Text);
        }
    }
}

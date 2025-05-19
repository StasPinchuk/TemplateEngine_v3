using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для BranchMainPage.xaml
    /// </summary>
    public partial class BranchMainPage : Page
    {
        public BranchMainPage(IBranchManager branchManager, Branch branch = null)
        {
            InitializeComponent();
            DataContext = new BranchMainPageVM(branchManager, branch);
        }
    }
}

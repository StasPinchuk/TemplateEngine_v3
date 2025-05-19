using System.Windows.Controls;
using TemplateEngine_v3.Services.UsersServices;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        public UsersPage(UserManager userManager, ColumnDefinition sideBar)
        {
            InitializeComponent();
            DataContext = new UsersPageVM(userManager, sideBar);
        }
    }
}

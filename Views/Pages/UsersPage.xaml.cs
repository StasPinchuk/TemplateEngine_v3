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
        private UsersPageVM vm;

        public UsersPage(UserManager userManager, ColumnDefinition sideBar)
        {
            InitializeComponent();
            vm = new UsersPageVM(userManager, sideBar);
            DataContext = vm;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                vm.SearchUser(textBox.Text);
        }
    }
}

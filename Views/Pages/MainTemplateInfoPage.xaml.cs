using Aspose.Cells;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainTemplateInfoPage.xaml
    /// </summary>
    public partial class MainTemplateInfoPage : Page
    {
        private ObservableCollection<TextBox> textBoxes = [];

        MainTemplateInfoPageVM vm;

        public MainTemplateInfoPage(ITechnologiesManager technologiesManager, ITemplateManager templateManager)
        {
            InitializeComponent();
            vm = new MainTemplateInfoPageVM(technologiesManager, templateManager);
            DataContext = vm;
        }

        private void TextBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var menu = vm.TextBoxMenu;
            if (menu != null && menu.Items.Count > 0)
            {
                textBoxes.Add(((TextBox)sender));
                ((TextBox)sender).ContextMenu = menu;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            vm.UpdateContextMenu(); // обновляет vm.TextBoxMenu

            var menu = vm.TextBoxMenu;
            foreach(var textBox in textBoxes)
            {
                if(textBox != null)
                    textBox.ContextMenu = menu;
            }
        }

        private void EnabledTextBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggle && toggle.Tag is int index)
            {
                var vm = DataContext as MainTemplateInfoPageVM;
                if (vm?.ExampleMarkings != null && index >= 0 && index < vm.ExampleMarkings.Count)
                {
                    string currentValue = vm.ExampleMarkings[index].Text;

                    // Обновите значение — например, вызов сохранения
                    vm.UpdateExampleMarking(index, currentValue);
                }
            }
        }

    }
}

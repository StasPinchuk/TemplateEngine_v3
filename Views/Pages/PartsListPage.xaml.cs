using System.Windows;
using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PartsListPage.xaml
    /// </summary>
    public partial class PartsListPage : Page
    {
        private readonly PartsListPageVM vm;
        public PartsListPage(ITechnologiesManager technologiesManager, INodeManager nodeManager, ITemplateManager templateManager)
        {
            InitializeComponent();
            vm = new PartsListPageVM(technologiesManager, nodeManager, templateManager, NodePage);
            DataContext = vm;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is PartsListPageVM vm)
            {
                if (e.NewValue is Node selectedNode)
                {
                    vm.SelectedNode = selectedNode;
                }
            }
        }

    }
}

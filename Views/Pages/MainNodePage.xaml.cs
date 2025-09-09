using System;
using System.Windows;
using System.Windows.Controls;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainNodePage.xaml
    /// </summary>
    public partial class MainNodePage : Page
    {
        private readonly MainNodePageVM _vm;

        public MainNodePage(NodeManager nodeManager, Action updatePage, Action updateNodeGroup)
        {
            InitializeComponent();
            _vm = new MainNodePageVM(nodeManager, updatePage, updateNodeGroup);
            DataContext = _vm;

            Unloaded += MainNodePage_Unloaded;

        }

        private async void TextBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is TextBox tb && tb.ContextMenu != null)
            {
                tb.ContextMenu = await _vm.GetContextMenu();
                tb.ContextMenu.PlacementTarget = tb;
            }
        }

        private void MainNodePage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_vm is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

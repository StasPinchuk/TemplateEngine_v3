using System;
using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainNodePage.xaml
    /// </summary>
    public partial class MainNodePage : Page
    {
        public MainNodePage(INodeManager nodeManager, Action updatePage, Action updateNodeGroup)
        {
            InitializeComponent();
            DataContext = new MainNodePageVM(nodeManager, updatePage, updateNodeGroup);
        }
    }
}

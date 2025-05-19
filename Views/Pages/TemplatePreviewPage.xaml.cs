using Antlr.Runtime.Tree;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TemplatePreviewPage.xaml
    /// </summary>
    public partial class TemplatePreviewPage : Page
    {
        TemplatePreviewVM viewModel;


        public TemplatePreviewPage(ITemplateManager templateManager, IBranchManager branchManager)
        {
            InitializeComponent();
            viewModel = new TemplatePreviewVM(templateManager, branchManager, DrawerHost, RightDrawerContext);
            DataContext = viewModel;

        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalElement = e.OriginalSource as DependencyObject;
            var container = FindParent<TreeViewItem>(originalElement);

            if (container != null)
            {
                container.IsSelected = true;
                container.Focus();

                var contextMenu = new ContextMenu();

                if (container.DataContext is ConditionEvaluator item)
                {
                    var menuItemEdit = new MenuItem 
                    { 
                        Header = "Изменить параметр",
                        Command = viewModel.EditParameterCommand,
                        CommandParameter = new object[] { item, DrawerHost }
                    };

                    contextMenu.Items.Add(menuItemEdit);

                    var menuItemDelete = new MenuItem 
                    { 
                        Header = "Удалить параметр",
                        Command = viewModel.DeleteParameterCommand,
                        CommandParameter = item 
                    };

                    contextMenu.Items.Add(menuItemDelete);
                }
                else if (container.DataContext is Operation operation)
                {
                    var menuItemEdit = new MenuItem
                    {
                        Header = "Изменить операцию",
                        Command = viewModel.EditOperationCommand,
                        CommandParameter = new object[] { operation, DrawerHost }
                    };

                    contextMenu.Items.Add(menuItemEdit);

                    var menuItemDelete = new MenuItem
                    {
                        Header = "Удалить операцию",
                        Command = viewModel.DeleteOperationCommand,
                        CommandParameter = operation
                    };

                    contextMenu.Items.Add(menuItemDelete);
                }
                else if (container.DataContext is Node node)
                {
                    var menuItemEdit = new MenuItem 
                    { 
                        Header = "Изменить деталь",
                        Command = viewModel.EditNodeCommand,
                        CommandParameter = new object[] { node, DrawerHost }
                    };
                    contextMenu.Items.Add(menuItemEdit);

                    var menuItemDelete = new MenuItem 
                    { 
                        Header = "Удалить деталь",
                        Command = viewModel.DeleteNodeCommand,
                        CommandParameter = new object[] { node, DrawerHost }
                    };
                    contextMenu.Items.Add(menuItemDelete);
                }

                if (contextMenu.Items.Count > 0)
                {
                    container.ContextMenu = contextMenu;
                    contextMenu.PlacementTarget = container;
                    contextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
        }

        public static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
        {
            try
            {
                while (child != null)
                {
                    if (child is T parent)
                        return parent;

                    child = VisualTreeHelper.GetParent(child);
                }

                return null;
            }catch (Exception ex)
            {
                return null;
            }
        }

        private void DrawerHost_DrawerClosing(object sender, DrawerClosingEventArgs e)
        {
            viewModel.CreateSpec(null);
        }
    }
}

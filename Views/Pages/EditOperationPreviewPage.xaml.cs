using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditOperationPreviewEditPage.xaml
    /// </summary>
    public partial class EditOperationPreviewPage : Page
    {
        private Frame _frame;
        private IEvaluatorManager _evaluatorManager;
        private ContextMenuHelper _contextMenuHelper;
        private DrawerHost _drawerHost;

        EditOperationPreviewEditVM viewModel;
        public EditOperationPreviewPage(Operation operation, IEvaluatorManager evaluatorManager, ContextMenuHelper contextMenuHelper, DrawerHost drawerHost, Frame frame)
        {
            InitializeComponent();
            _frame = frame;
            _contextMenuHelper = contextMenuHelper;
            _evaluatorManager = evaluatorManager;
            _drawerHost = drawerHost;
            viewModel = new EditOperationPreviewEditVM(operation, evaluatorManager, contextMenuHelper, drawerHost);
            DataContext = viewModel;
        }

        public static object? GetPropertyValueByPath(object source, string path)
        {
            if (source == null || string.IsNullOrEmpty(path))
                return null;

            var currentObject = source;
            var parts = path.Split('.');

            // Проходим по частям пути, но не берем последнюю часть (.Value)
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var propInfo = currentObject.GetType().GetProperty(parts[i]);
                if (propInfo == null)
                    return null;

                currentObject = propInfo.GetValue(currentObject);
                if (currentObject == null)
                    return null;
            }

            return currentObject; // это будет объект перед последним свойством, например, Materials.Name
        }

        private void TextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var binding = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);

                var source = binding?.DataItem;
                var fullPath = binding?.ParentBinding.Path.Path; // например, "Materials.Name.Value"

                if (source != null && !string.IsNullOrEmpty(fullPath))
                {
                    // Получаем объект до последнего свойства
                    var objectBeforeLast = GetPropertyValueByPath(source, fullPath);
                    if (objectBeforeLast is ConditionEvaluator condition && condition.Parts.Count > 0)
                    {
                        viewModel.SetParts(condition);
                        var animation = new DoubleAnimation
                        {
                            To = 300,
                            Duration = TimeSpan.FromMilliseconds(500),
                            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                        };
                        Tree.BeginAnimation(FrameworkElement.WidthProperty, animation);
                    }
                    else
                    {
                        viewModel.ClearParts();
                        var animation = new DoubleAnimation
                        {
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(500),
                            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                        };
                        Tree.BeginAnimation(FrameworkElement.WidthProperty, animation);
                    }
                }
            }
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeEvaluator treeEvaluator)
            {
                _frame.Navigate(new EditParametersPreviewPage(treeEvaluator.ConditionEvaluator, _evaluatorManager, _drawerHost));

                // Снять выделение после навигации
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (sender is TreeView tree)
                    {
                        var container = GetTreeViewItem(tree, e.NewValue);
                        if (container != null)
                        {
                            container.IsSelected = false;
                        }
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }


        private TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container == null)
                return null;

            for (int i = 0; i < container.Items.Count; i++)
            {
                var child = container.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (child == null)
                {
                    container.UpdateLayout();
                    container.ItemContainerGenerator.StatusChanged += (s, e) =>
                    {
                        if (container.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                        {
                            child = container.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                        }
                    };
                    continue;
                }

                if (child.DataContext == item)
                    return child;

                var descendant = GetTreeViewItem(child, item);
                if (descendant != null)
                    return descendant;
            }

            return null;
        }


    }
}

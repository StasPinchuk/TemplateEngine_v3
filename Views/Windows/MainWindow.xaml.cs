using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TemplateEngine_v3.Services.ServerServices;
using TemplateEngine_v3.VM.Windows;

namespace TemplateEngine_v3.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ScrollViewer? _tabsScrollViewer;

        public MainWindow(ServerManager serverManager)
        {
            InitializeComponent();
            DataContext = new MainWindowVM(serverManager.ReferenceManager, serverManager.UserManager, MainFrame, SideBar);

            Loaded += (_, _) =>
            {
                _tabsScrollViewer = GetScrollViewer(TabsListBox);
                UpdateScrollButtons();
            };
        }

        private ScrollViewer? GetScrollViewer(DependencyObject parent)
        {
            if (parent is ScrollViewer sv)
                return sv;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }

            return null;
        }


        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            if (_tabsScrollViewer == null) return;

            for (int i = TabsListBox.Items.Count - 1; i >= 0; i--)
            {
                var container = TabsListBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (container == null) continue;

                var transform = container.TransformToAncestor(_tabsScrollViewer);
                var position = transform.Transform(new Point(0, 0));

                if (position.X < 0)
                {
                    _tabsScrollViewer.ScrollToHorizontalOffset(_tabsScrollViewer.HorizontalOffset + position.X - 5);
                    break;
                }
            }

            UpdateScrollButtons();
        }

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            if (_tabsScrollViewer == null) return;

            foreach (var item in TabsListBox.Items)
            {
                var container = TabsListBox.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container == null) continue;

                var transform = container.TransformToAncestor(_tabsScrollViewer);
                var position = transform.Transform(new Point(0, 0));

                if (position.X + container.ActualWidth > _tabsScrollViewer.ViewportWidth)
                {
                    _tabsScrollViewer.ScrollToHorizontalOffset(_tabsScrollViewer.HorizontalOffset + container.ActualWidth);
                    break;
                }
            }

            UpdateScrollButtons();
        }


        private void TabsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_tabsScrollViewer == null)
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (TabsListBox.SelectedItem is null)
                    return;

                var item = TabsListBox.ItemContainerGenerator.ContainerFromItem(TabsListBox.SelectedItem) as ListBoxItem;
                if (item == null)
                    return;

                var transform = item.TransformToAncestor(_tabsScrollViewer);
                var position = transform.Transform(new Point(0, 0));

                double itemLeft = position.X;
                double itemRight = itemLeft + item.ActualWidth;
                double viewportWidth = _tabsScrollViewer.ViewportWidth;
                double offset = _tabsScrollViewer.HorizontalOffset;

                if (itemLeft < 0)
                {
                    _tabsScrollViewer.ScrollToHorizontalOffset(offset + itemLeft - 5);
                }
                else if (itemRight > viewportWidth)
                {
                    double delta = itemRight - viewportWidth;
                    _tabsScrollViewer.ScrollToHorizontalOffset(offset + delta + 5);
                }

                UpdateScrollButtons();
            }), DispatcherPriority.Loaded);
        }


        private void UpdateScrollButtons()
        {
            if (_tabsScrollViewer == null)
                return;

            ScrollLeftButton.IsEnabled = _tabsScrollViewer.HorizontalOffset > 0;
            ScrollRightButton.IsEnabled = _tabsScrollViewer.HorizontalOffset < _tabsScrollViewer.ScrollableWidth;
        }
    }
}

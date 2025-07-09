using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TemplateEngine_v3.Services.ServerServices;
using TemplateEngine_v3.VM.Windows;

namespace TemplateEngine_v3.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(ServerManager serverManager)
        {
            InitializeComponent();
            DataContext = new MainWindowVM(serverManager.ReferenceManager, serverManager.UserManager, MainFrame, SideBar);
        }

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TabsListBox.Items)
            {
                var listBoxItem = (ListBoxItem)TabsListBox.ItemContainerGenerator.ContainerFromItem(item);
                if (listBoxItem == null)
                    continue;

                // Координаты элемента относительно ScrollViewer
                GeneralTransform transform = listBoxItem.TransformToAncestor(TabScrollViewer);
                Point position = transform.Transform(new Point(0, 0));
                double itemRight = position.X + listBoxItem.ActualWidth;

                // Если элемент выходит за правую границу
                if (itemRight > TabScrollViewer.ViewportWidth)
                {
                    // Прокручиваем до текущего смещения + (itemRight - ширина вьюпорта)
                    double delta = itemRight - TabScrollViewer.ViewportWidth;
                    TabScrollViewer.ScrollToHorizontalOffset(TabScrollViewer.HorizontalOffset + delta + 5); // +5 — немного с запасом
                    break;
                }
            }
        }


        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            for (int i = TabsListBox.Items.Count - 1; i >= 0; i--)
            {
                var item = TabsListBox.Items[i];
                var listBoxItem = (ListBoxItem)TabsListBox.ItemContainerGenerator.ContainerFromItem(item);
                if (listBoxItem == null)
                    continue;

                GeneralTransform transform = listBoxItem.TransformToAncestor(TabScrollViewer);
                Point position = transform.Transform(new Point(0, 0));

                // Если левый край элемента выходит за левую границу
                if (position.X < 0)
                {
                    TabScrollViewer.ScrollToHorizontalOffset(TabScrollViewer.HorizontalOffset + position.X);
                    break;
                }
            }
        }


    }
}

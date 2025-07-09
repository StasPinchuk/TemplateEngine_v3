using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.VM.Pages;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TechnologiesPage.xaml
    /// </summary>
    public partial class TechnologiesPage : Page
    {
        private Point _dragStartPoint;
        private TechnologiesPageVM vm;

        public TechnologiesPage(TechnologiesManager technologiesManager)
        {
            InitializeComponent();
            vm = new TechnologiesPageVM(technologiesManager);
            DataContext = vm;
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (sender is ListBox listBox && listBox.SelectedItem != null)
                    {
                        DragDrop.DoDragDrop(listBox, listBox.SelectedItem, DragDropEffects.Move);
                    }
                }
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Operation))) // замените OperationModel на вашу модель
            {
                var sourceItem = (Operation)e.Data.GetData(typeof(Operation));
                var listBox = (ListBox)sender;
                var targetItem = GetItemAtMousePosition(listBox, e.GetPosition(listBox));

                if (targetItem == null || sourceItem == targetItem)
                    return;

                var collection = vm.CurrentTechnologies.Operations;
                int oldIndex = collection.IndexOf(sourceItem);
                int newIndex = collection.IndexOf(targetItem);

                if (oldIndex != newIndex)
                {
                    collection.Move(oldIndex, newIndex);
                }
            }
        }

        private Operation GetItemAtMousePosition(ListBox listBox, Point position)
        {
            var element = listBox.InputHitTest(position) as DependencyObject;
            while (element != null && !(element is ListBoxItem))
                element = VisualTreeHelper.GetParent(element);

            return element != null ? (Operation)listBox.ItemContainerGenerator.ItemFromContainer(element) : null;
        }
    }
}

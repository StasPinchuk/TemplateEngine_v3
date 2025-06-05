using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ReplaceChoiceDialog.xaml
    /// </summary>
    public partial class ReplaceChoiceDialog : UserControl
    {
        public static DependencyProperty ReferencesListProperty =
            DependencyProperty.Register(
                "ReferencesList",
                typeof(ObservableCollection<ReferenceModelInfo>),
                typeof(ReplaceChoiceDialog),
                new PropertyMetadata(null)
            );

        public static DependencyProperty SelectedReferencesListProperty =
            DependencyProperty.Register(
                "SelectedReferencesList",
                typeof(ObservableCollection<ReferenceModelInfo>),
                typeof(ReplaceChoiceDialog),
                new PropertyMetadata(new ObservableCollection<ReferenceModelInfo>())
            );

        public static DependencyProperty FindStringProperty =
            DependencyProperty.Register(
                "FindString",
                typeof(string),
                typeof(ReplaceChoiceDialog),
                new PropertyMetadata(string.Empty)
            );

        public static DependencyProperty ReplaceStringProperty =
            DependencyProperty.Register(
                "ReplaceString",
                typeof(string),
                typeof(ReplaceChoiceDialog),
                new PropertyMetadata(string.Empty)
            );

        public static DependencyProperty ReplaceCommandProperty =
            DependencyProperty.Register(
                "ReplaceCommand",
                typeof(ICommand),
                typeof(ReplaceChoiceDialog),
                new PropertyMetadata(default)
            );

        public static DependencyProperty TemplateManagerProperty =
            DependencyProperty.Register(
                "TemplateManager",
                typeof(ITemplateManager),
                typeof(ReplaceChoiceDialog),
                new PropertyMetadata(default)
            );

        public ObservableCollection<ReferenceModelInfo> ReferencesList
        {
            get => (ObservableCollection<ReferenceModelInfo>)GetValue(ReferencesListProperty);
            set => SetValue(ReferencesListProperty, value);
        }

        public ObservableCollection<ReferenceModelInfo> SelectedReferencesList
        {
            get => (ObservableCollection<ReferenceModelInfo>)GetValue(SelectedReferencesListProperty);
            set => SetValue(SelectedReferencesListProperty, value);
        }

        public string FindString
        {
            get => (string)GetValue(FindStringProperty);
            set => SetValue(FindStringProperty, value);
        }

        public string ReplaceString
        {
            get => (string)GetValue(ReplaceStringProperty);
            set => SetValue(ReplaceStringProperty, value);
        }

        public ICommand ReplaceCommand
        {
            get => (ICommand)GetValue(ReplaceCommandProperty);
            set => SetValue(ReplaceCommandProperty, value);
        }

        public ITemplateManager TemplateManager
        {
            get => (ITemplateManager)GetValue(TemplateManagerProperty);
            set => SetValue(TemplateManagerProperty, value);
        }

        public ReplaceChoiceDialog(ObservableCollection<ReferenceModelInfo> referencesList, ITemplateManager templateManager)
        {
            InitializeComponent();
            ReferencesList = referencesList;
            TemplateManager = templateManager;
            ReplaceCommand = new RelayCommand(Replace, CanRepalce);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is CheckBox checkBox && checkBox.DataContext is ReferenceModelInfo selectedItem)
                {
                    if (!SelectedReferencesList.Contains(selectedItem))
                        SelectedReferencesList.Add(selectedItem);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is ReferenceModelInfo selectedItem)
            {
                if (SelectedReferencesList.Contains(selectedItem))
                    SelectedReferencesList.Remove(selectedItem);
            }
        }

        private bool CanRepalce(object parameter)
        {
            return SelectedReferencesList.Count > 0 && !string.IsNullOrWhiteSpace(FindString);
        }

        private async void Replace(object parameter)
        {
            string replaceInfo = string.Empty;
            foreach (var selected in SelectedReferencesList)
            {
                int replaceCount = 0;
                string result = Regex.Replace(selected.ObjectStruct, FindString, match =>
                {
                    replaceCount++;
                    return ReplaceString;
                });

                if (replaceCount > 0)
                {
                    selected.ObjectStruct = result;
                    if (!await TemplateManager.SetTemplateAsync(selected))
                        continue;
                    await TemplateManager.SaveTemplate("Ready");
                    replaceInfo += $"В шаблоне '{selected.Name}' заменено: {replaceCount}\n";
                }

            }
            if (!string.IsNullOrEmpty(replaceInfo))
            {
                MessageBox.Show(replaceInfo, "Замена значений в шаблонах");
                TemplateManager.ClearTemplate();
            }
            else
            {
                MessageBox.Show($"Строка '{FindString}' не найдена в выбранных шаблонах!!!", "Ошибка");
            }
        }

    }
}

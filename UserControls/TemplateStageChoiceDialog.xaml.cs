using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;
using TFlex.DOCs.Model.Stages;
using Xceed.Wpf.Toolkit;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для TemplateStageChoiceDialog.xaml
    /// </summary>
    public partial class TemplateStageChoiceDialog : UserControl
    {
        public static readonly DependencyProperty CurrentStageProperty =
            DependencyProperty.Register(
                "CurrentStage",
                typeof(StageModel),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(new StageModel())
            );

        public static readonly DependencyProperty StageListProperty =
            DependencyProperty.Register(
                "StageList",
                typeof(ObservableCollection<StageModel>),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                "Items",
                typeof(ObservableCollection<Button>),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty IconColorsProperty =
            DependencyProperty.Register(
                "IconColors",
                typeof(ObservableCollection<Button>),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                "SelectedColor",
                typeof(Color),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(Colors.Gray)
            );

        public static readonly DependencyProperty SelectedIconColorProperty =
            DependencyProperty.Register(
                "SelectedIconColor",
                typeof(Color),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(Colors.Black)
            );

        public static readonly DependencyProperty StageIconsListProperty =
            DependencyProperty.Register(
                "StageIconsList",
                typeof(ObservableCollection<PackIconKind>),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(new ObservableCollection<PackIconKind>())
            );

        public static readonly DependencyProperty TemplateStageServiceProperty =
            DependencyProperty.Register(
                "TemplateStageService",
                typeof(TemplateStageService),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(
                "ButtonText",
                typeof(string),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata("Добавить статус")
            );

        public StageModel CurrentStage
        {
            get => (StageModel)GetValue(CurrentStageProperty);
            set => SetValue(CurrentStageProperty, value);
        }

        public ObservableCollection<StageModel> StageList
        {
            get => (ObservableCollection<StageModel>)GetValue(StageListProperty);
            set => SetValue(StageListProperty, value);
        }

        public ObservableCollection<Button> Items
        {
            get => (ObservableCollection<Button>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public ObservableCollection<Button> IconColors
        {
            get => (ObservableCollection<Button>)GetValue(IconColorsProperty);
            set => SetValue(IconColorsProperty, value);
        }

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }


        public Color SelectedIconColor
        {
            get => (Color)GetValue(SelectedIconColorProperty);
            set => SetValue(SelectedIconColorProperty, value);
        }

        public ObservableCollection<PackIconKind> StageIconsList
        {
            get => (ObservableCollection<PackIconKind>)GetValue(StageIconsListProperty);
            set => SetValue(StageIconsListProperty, value);
        }

        public TemplateStageService TemplateStageService
        {
            get => (TemplateStageService)GetValue(TemplateStageServiceProperty);
            set => SetValue(TemplateStageServiceProperty, value);
        }

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        private bool isEdit = false;

        public ICommand ModifyStageCommand { get; set; }
        public ICommand RemoveStageCommand { get; set; }

        public TemplateStageChoiceDialog(TemplateStageService templateStageService)
        {
            InitializeComponent();
            InitializeCollections(templateStageService);
            InitializeVisibility();
            CreateBackgroundColorButton();
            CreateIconColorButton();

            ModifyStageCommand = new RelayCommand(ModifyStage, CanModifyStage);
            RemoveStageCommand = new RelayCommand(RemoveStage);
        }


        private void InitializeCollections(TemplateStageService templateStageService)
        {
            TemplateStageService = templateStageService;
            TemplateStageService.SetStageList();
            StageList = templateStageService.StageList;
            CurrentStage = new();
            StageIconsList = new(Enum.GetValues(typeof(PackIconKind)).Cast<PackIconKind>().ToList());
            Items = new ObservableCollection<Button>();
            IconColors = new ObservableCollection<Button>();
        }

        private void InitializeVisibility()
        {
            ColorPickerPanel.Visibility = Visibility.Collapsed;
            IconColorPickerPanel.Visibility = Visibility.Collapsed;
        }

        private void CreateBackgroundColorButton()
        {
            var btnBgList = new Button
            {
                Content = "+",
                Width = 35,
                Height = 35,
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.LightGray),
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = 30,
                Padding = new Thickness(0)
            };

            btnBgList.Click += OpenColorPickerPanel_Click;
            Items.Add(btnBgList);
        }

        private void CreateIconColorButton()
        {
            var btnIconsList = new Button
            {
                Content = "+",
                Width = 35,
                Height = 35,
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.LightGray),
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = 30,
                Padding = new Thickness(0)
            };

            btnIconsList.Click += OpenIconColorPickerPanel_Click;
            IconColors.Add(btnIconsList);
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var isContainsColor = Items.Any(btn => btn.Background is SolidColorBrush solid && solid.Color == SelectedColor);

            if (isContainsColor)
            {
                System.Windows.MessageBox.Show("Данный цвет уже добавлен!");
                return;
            }

            var button = new Button
            {
                Width = 35,
                Height = 35,
                Background = new SolidColorBrush(SelectedColor),
                BorderBrush = new SolidColorBrush(SelectedColor),
                Margin = new Thickness(4)
            };

            CurrentStage.BackgroundStageColor = new SolidColorBrush(SelectedColor);

            button.Click += SelectStageBackgroundColor_Click;

            Items.Insert(Items.Count - 1, button);
            SelectedColor = Colors.Gray;
            ColorPickerPanel.Visibility = Visibility.Collapsed;
        }

        private void AddIconColorButton_Click(object sender, RoutedEventArgs e)
        {
            var isContainsColor = IconColors.Any(btn => btn.Background is SolidColorBrush solid && solid.Color == SelectedColor);

            if (isContainsColor)
            {
                System.Windows.MessageBox.Show("Данный цвет уже добавлен!");
                return;
            }

            var button = new Button
            {
                Width = 35,
                Height = 35,
                Background = new SolidColorBrush(SelectedIconColor),
                BorderBrush = new SolidColorBrush(SelectedIconColor),
                Margin = new Thickness(4)
            };

            CurrentStage.IconStageColor = new SolidColorBrush(SelectedIconColor);

            button.Click += SelectIconColor_Click;

            IconColors.Insert(IconColors.Count - 1, button);
            SelectedIconColor = Colors.Gray;
            IconColorPickerPanel.Visibility = Visibility.Collapsed;
        }

        private void OpenColorPickerPanel_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerPanel.Visibility = Visibility.Visible;
        }

        private void OpenIconColorPickerPanel_Click(object sender, RoutedEventArgs e)
        {
            IconColorPickerPanel.Visibility = Visibility.Visible;
        }

        private void SelectStageBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Background is SolidColorBrush solidBrush)
            {
                CurrentStage.BackgroundStageColor = new SolidColorBrush(solidBrush.Color);
            }
            else
            {
                CurrentStage.BackgroundStageColor = new SolidColorBrush(Colors.LightGray);
            }
        }

        private void SelectIconColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Background is SolidColorBrush solidBrush)
            {
                CurrentStage.IconStageColor = new SolidColorBrush(solidBrush.Color);
            }
            else
            {
                CurrentStage.IconStageColor = new SolidColorBrush(Colors.LightGray);
            }
        }

        private void SelectedTextColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue != null)
                CurrentStage.TextStageColor = new SolidColorBrush(e.NewValue.Value);
        }

        private bool CanModifyStage(object parameter)
        {
            return !string.IsNullOrEmpty(CurrentStage?.StageName)
                   && !(CurrentStage?.StageName.Equals("Название статуса") ?? false);
        }

        private void ModifyStage(object parameter)
        {
            if(isEdit)
                TemplateStageService.EditStage(CurrentStage);
            else
                TemplateStageService.AddStage(CurrentStage);

            StageListBox.SelectedItem = null;

            isEdit = false;
            ButtonText = "Добавить статус";

            CurrentStage = new();
        }

        private void RemoveStage(object parameter)
        {
            if(parameter is StageModel stage)
            {
                var stageIndex = StageList.IndexOf(stage);
                if (stageIndex < StageList.Count - 1)
                    CurrentStage = StageList[stageIndex + 1];
                else if (stageIndex == StageList.Count - 1 && StageList.Count > 1)
                    CurrentStage = StageList[stageIndex - 1];
                else
                    CurrentStage = new();

                TemplateStageService.RemoveStage(stage);
            }
        }

        private void StageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StageListBox.SelectedItem is StageModel)
            {
                isEdit = true;
                ButtonText = "Изменить статус";
            }

        }
    }
}

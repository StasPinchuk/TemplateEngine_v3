using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

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

        public static readonly DependencyProperty SelectedTextColorProperty =
            DependencyProperty.Register(
                "SelectedTextColor",
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
                new PropertyMetadata("Добавить стадию")
            );

        public static readonly DependencyProperty StatusTypeStringProperty =
            DependencyProperty.Register(
                "StatusTypeString",
                typeof(string),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(string.Empty)
            );

        public static readonly DependencyProperty StatusTypeListProperty =
            DependencyProperty.Register(
                "StatusTypeList",
                typeof(List<string>),
                typeof(TemplateStageChoiceDialog),
                new PropertyMetadata(new List<string>())
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

        public Color SelectedTextColor
        {
            get => (Color)GetValue(SelectedTextColorProperty);
            set => SetValue(SelectedTextColorProperty, value);
        }

        public ObservableCollection<PackIconKind> StageIconsList
        {
            get => (ObservableCollection<PackIconKind>)GetValue(StageIconsListProperty);
            set => SetValue(StageIconsListProperty, value);
        }

        public TemplateStageService StageService
        {
            get => (TemplateStageService)GetValue(TemplateStageServiceProperty);
            set => SetValue(TemplateStageServiceProperty, value);
        }

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        public string StatusTypeString
        {
            get => (string)GetValue(StatusTypeStringProperty);
            set => SetValue(StatusTypeStringProperty, value);
        }

        public List<string> StatusTypeList
        {
            get => (List<string>)GetValue(StatusTypeListProperty);
            set => SetValue(StatusTypeListProperty, value);
        }

        private bool isEdit = false;

        public ICommand ModifyStageCommand { get; set; }
        public ICommand RemoveStageCommand { get; set; }
        public ICommand CancelStageCommand { get; set; }

        public TemplateStageChoiceDialog(TemplateStageService templateStageService)
        {
            InitializeComponent();
            InitializeCollections(templateStageService);
            InitializeVisibility();
            SetBackgroundAndIconColors();
            CreateBackgroundColorButton();
            CreateIconColorButton();

            ModifyStageCommand = new RelayCommand(ModifyStage, CanModifyStage);
            RemoveStageCommand = new RelayCommand(RemoveStage);
            CancelStageCommand = new RelayCommand(CancelStage, CanCancelStage);
        }


        private void InitializeCollections(TemplateStageService templateStageService)
        {
            StageService = templateStageService;
            StageService.SetStageList();
            StageList = templateStageService.StageList;
            StatusTypeList = templateStageService.GetStatusTypeList();
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

        private void SetBackgroundAndIconColors()
        {
            foreach (var stage in StageList)
            {
                CreateBackgroundColorButton(stage.BackgroundStageColor, stage.BackgroundStageColor, stage.TextStageColor, "");
                CreateIconColorButton(stage.IconStageColor, stage.IconStageColor, stage.TextStageColor, "");
            }
        }

        private void CreateBackgroundColorButton(Brush background = null, Brush border = null, Brush foreground = null, string content = "+")
        {
            background ??= new SolidColorBrush(Colors.White);
            border ??= new SolidColorBrush(Colors.LightGray);
            foreground ??= new SolidColorBrush(Colors.Black);

            var isContainsColor = Items.Any(btn => btn.Background is SolidColorBrush solid && solid.Color == ((SolidColorBrush)background).Color);

            if (isContainsColor)
            {
                return;
            }

            var btnBgList = new Button
            {
                Content = content,
                Width = 35,
                Height = 35,
                Background = background,
                BorderBrush = border,
                Foreground = foreground,
                FontSize = 30,
                Padding = new Thickness(0)
            };

            btnBgList.Click += OpenColorPickerPanel_Click;
            Items.Add(btnBgList);
        }

        private void CreateIconColorButton(Brush background = null, Brush border = null, Brush foreground = null, string content = "+")
        {
            background ??= new SolidColorBrush(Colors.White);
            border ??= new SolidColorBrush(Colors.LightGray);
            foreground ??= new SolidColorBrush(Colors.Black);

            var isContainsColor = IconColors.Any(btn => btn.Background is SolidColorBrush solid && solid.Color == ((SolidColorBrush)background).Color);

            if (isContainsColor)
            {
                return;
            }

            var btnIconsList = new Button
            {
                Content = content,
                Width = 35,
                Height = 35,
                Background = background,
                BorderBrush = border,
                Foreground = foreground,
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
            var isContainsColor = IconColors.Any(btn => btn.Background is SolidColorBrush solid && solid.Color == SelectedIconColor);

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
            if (ColorPickerPanel.Visibility == Visibility.Visible)
            {
                ColorPickerPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ColorPickerPanel.Visibility = Visibility.Visible;
            }
        }

        private void OpenIconColorPickerPanel_Click(object sender, RoutedEventArgs e)
        {
            if (IconColorPickerPanel.Visibility == Visibility.Visible)
            {
                IconColorPickerPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                IconColorPickerPanel.Visibility = Visibility.Visible;
            }
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
                   && !(CurrentStage?.StageName.Equals("Название стадии") ?? false);
        }

        private void ModifyStage(object parameter)
        {
            if (isEdit)
                StageService.EditStage(CurrentStage);
            else
                StageService.AddStage(CurrentStage);

            StageListBox.SelectedItem = null;

            isEdit = false;
            ButtonText = "Добавить стадию";

            CurrentStage = new();
        }

        private void RemoveStage(object parameter)
        {
            if (parameter is StageModel stage)
            {
                var stageIndex = StageList.IndexOf(stage);
                if (stageIndex < StageList.Count - 1)
                    CurrentStage = StageList[stageIndex + 1];
                else if (stageIndex == StageList.Count - 1 && StageList.Count > 1)
                    CurrentStage = StageList[stageIndex - 1];
                else
                    CurrentStage = new();

                StageService.RemoveStage(stage);

                SetBackgroundAndIconColors();
            }
        }

        private bool CanCancelStage(object parameter)
        {
            return !(CurrentStage?.StageName.Equals("Название стадии") ?? false);
        }

        private void CancelStage(object parameter)
        {

            StageListBox.SelectedItem = null;
            CurrentStage = new();
        }

        private void StageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StageListBox.SelectedItem is StageModel)
            {
                isEdit = true;

                StatusTypeString = StageService.GetEnumDescription(CurrentStage.StageType);
                SelectedTextColor = ((SolidColorBrush)CurrentStage.TextStageColor).Color;
                SelectedColor = ((SolidColorBrush)CurrentStage.BackgroundStageColor).Color;
                SelectedIconColor = ((SolidColorBrush)CurrentStage.IconStageColor).Color;

                ButtonText = "Изменить стадию";
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedStatus)
            {
                StatusType? result = StageService.GetEnumValueByDescription<StatusType>(selectedStatus);

                if (result.HasValue)
                    CurrentStage.StageType = result.Value;
            }
        }

    }
}

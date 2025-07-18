using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для SaveChoiceDialog.xaml
    /// </summary>
    public partial class SaveChoiceDialog : UserControl
    {
        public static readonly DependencyProperty StageListProperty =
            DependencyProperty.Register(
                "StageList",
                typeof(ObservableCollection<StageModel>),
                typeof(SaveChoiceDialog),
                new PropertyMetadata(new ObservableCollection<StageModel>())
            );

        public static readonly DependencyProperty CurrentStageProperty =
            DependencyProperty.Register(
                "CurrentStage",
                typeof(StageModel),
                typeof(SaveChoiceDialog),
                new PropertyMetadata(null)
            );

        public static readonly DependencyProperty TypeStringProperty =
            DependencyProperty.Register(
                "TypeString",
                typeof(string),
                typeof(SaveChoiceDialog),
                new PropertyMetadata(null)
            );

        public ObservableCollection<StageModel> StageList
        {
            get => (ObservableCollection<StageModel>)GetValue(StageListProperty);
            set => SetValue(StageListProperty, value);
        }

        public StageModel CurrentStage
        {
            get => (StageModel)GetValue(CurrentStageProperty);
            set => SetValue(CurrentStageProperty, value);
        }

        public string TypeString
        {
            get => (string)GetValue(TypeStringProperty);
            set => SetValue(TypeStringProperty, value);
        }

        private readonly TemplateStageService _stageService;
        private readonly TemplateManager _templateManager;

        public SaveChoiceDialog(TemplateManager templateManager, TemplateStageService stageService)
        {
            InitializeComponent();
            _stageService = stageService;
            _stageService.SetStageList();
            _templateManager = templateManager;
            SetStageList();
        }

        private void SetStageList()
        {
            try
            {
                Guid currentStageID = _templateManager.SelectedTemplate.Stage;
                var currentStage = _stageService.StageList.FirstOrDefault(stage => stage.ID.Equals(currentStageID));

                if (currentStage != null)
                {
                    StageList.Clear();
                    bool isVerification = currentStage.StageType != StatusType.Verification && currentStage.StageType != StatusType.Final;
                    var filterStage = _stageService.StageList
                        .Where(stage => isVerification ? stage.StageType != StatusType.Final && stage.StageType != StatusType.Archive : true).ToList();
                    foreach (var stage in filterStage)
                        StageList.Add(stage);

                    CurrentStage = currentStage;
                }
                else
                {
                    StageList.Clear();
                    var filterStage = _stageService.StageList.Where(stage => stage.StageType != StatusType.Final && stage.StageType != StatusType.Archive).ToList();
                    foreach (var stage in filterStage)
                        StageList.Add(stage);

                    CurrentStage = StageList.First();
                }
            }
            catch (Exception) { }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is StageModel stage)
            {
                _templateManager.SelectedTemplate.Stage = stage.ID;
                TypeString = stage.StageType.ToString();
            }
        }
    }
}

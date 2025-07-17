using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Media;

namespace TemplateEngine_v3.Models
{
    public class StageModel : BaseNotifyPropertyChanged
    {
        private Guid _id;
        public Guid ID
        {
            get => _id;
            set => SetValue(ref _id, value, nameof(ID));
        }

        private string _stageName = "Название статуса";
        public string StageName
        {
            get => _stageName;
            set => SetValue(ref _stageName, value, nameof(StageName));
        }

        private PackIconKind _stageIcon = PackIconKind.Abacus;
        public PackIconKind StageIcon
        {
            get => _stageIcon;
            set => SetValue(ref _stageIcon, value, nameof(StageIcon));
        }

        private Brush _backgroundStageColor = new SolidColorBrush(Colors.LightGray);
        public Brush BackgroundStageColor
        {
            get => _backgroundStageColor;
            set => SetValue(ref _backgroundStageColor, value, nameof(BackgroundStageColor));
        }

        private Brush _iconStageColor = new SolidColorBrush(Colors.Black);
        public Brush IconStageColor
        {
            get => _iconStageColor;
            set => SetValue(ref _iconStageColor, value, nameof(IconStageColor));
        }

        private Brush _textStageColor = new SolidColorBrush(Colors.Black);
        public Brush TextStageColor
        {
            get => _textStageColor;
            set => SetValue(ref _textStageColor, value, nameof(TextStageColor));
        }
    }
}

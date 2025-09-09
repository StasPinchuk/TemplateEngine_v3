using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Media;

namespace TemplateEngine_v3.Models
{
    /// <summary>
    /// Модель данных, описывающая этап шаблона.
    /// Содержит идентификатор, название, тип, иконку и параметры оформления.
    /// </summary>
    public class StageModel : BaseNotifyPropertyChanged
    {
        private Guid _id;

        /// <summary>
        /// Уникальный идентификатор этапа.
        /// </summary>
        public Guid ID
        {
            get => _id;
            set => SetValue(ref _id, value, nameof(ID));
        }

        private string _stageName = "Название стадии";

        /// <summary>
        /// Название этапа.
        /// </summary>
        public string StageName
        {
            get => _stageName;
            set => SetValue(ref _stageName, value, nameof(StageName));
        }

        private StatusType _stageType;

        /// <summary>
        /// Тип этапа (значение перечисления <see cref="StatusType"/>).
        /// </summary>
        public StatusType StageType
        {
            get => _stageType;
            set => SetValue(ref _stageType, value, nameof(StageType));
        }

        private PackIconKind _stageIcon = PackIconKind.Abacus;

        /// <summary>
        /// Иконка этапа (значение перечисления <see cref="PackIconKind"/> из MaterialDesignThemes).
        /// </summary>
        public PackIconKind StageIcon
        {
            get => _stageIcon;
            set => SetValue(ref _stageIcon, value, nameof(StageIcon));
        }

        private Brush _backgroundStageColor = new SolidColorBrush(Colors.LightGray);

        /// <summary>
        /// Цвет фона этапа.
        /// </summary>
        public Brush BackgroundStageColor
        {
            get => _backgroundStageColor;
            set => SetValue(ref _backgroundStageColor, value, nameof(BackgroundStageColor));
        }

        private Brush _iconStageColor = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// Цвет иконки этапа.
        /// </summary>
        public Brush IconStageColor
        {
            get => _iconStageColor;
            set => SetValue(ref _iconStageColor, value, nameof(IconStageColor));
        }

        private Brush _textStageColor = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// Цвет текста этапа.
        /// </summary>
        public Brush TextStageColor
        {
            get => _textStageColor;
            set => SetValue(ref _textStageColor, value, nameof(TextStageColor));
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для EvalutorBlock.xaml
    /// </summary>
    public partial class EvalutorBlock : UserControl
    {
        public static DependencyProperty EvalutorProperty =
            DependencyProperty.Register("Evalutor", typeof(ConditionEvaluator), typeof(EvalutorBlock), new PropertyMetadata(null));

        public static DependencyProperty CopyEvalutorCommandProperty =
            DependencyProperty.Register("CopyEvalutorCommand", typeof(ICommand), typeof(EvalutorBlock), new PropertyMetadata(null));

        public static DependencyProperty RemoveEvalutorCommandProperty =
            DependencyProperty.Register("RemoveEvalutorCommand", typeof(ICommand), typeof(EvalutorBlock), new PropertyMetadata(null));

        public ConditionEvaluator Evalutor
        {
            get => (ConditionEvaluator)GetValue(EvalutorProperty);
            set => SetValue(EvalutorProperty, value);
        }

        public ICommand CopyEvalutorCommand
        {
            get => (ICommand)GetValue(CopyEvalutorCommandProperty);
            set => SetValue(CopyEvalutorCommandProperty, value);
        }

        public ICommand RemoveEvalutorCommand
        {
            get => (ICommand)GetValue(RemoveEvalutorCommandProperty);
            set => SetValue(RemoveEvalutorCommandProperty, value);
        }

        public EvalutorBlock()
        {
            InitializeComponent();
        }
    }
}

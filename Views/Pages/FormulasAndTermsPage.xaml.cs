using ICSharpCode.AvalonEdit;
using System.Windows.Controls;
using System.Windows.Media;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.VM.Pages;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для FormulasAndTermsPage.xaml
    /// </summary>
    public partial class FormulasAndTermsPage : Page
    {
        public FormulasAndTermsPage(INodeManager nodeManager)
        {
            InitializeComponent();

            DataContext = new FormulasAndTermsPageVM(nodeManager);
        }

        private void SetCurrentEvaluatorCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is ConditionEvaluator eval)
            {
                var vm = (DataContext as FormulasAndTermsPageVM);
                if (vm?.SetCurrentEvaluatorCommand.CanExecute(eval) == true)
                    vm.SetCurrentEvaluatorCommand.Execute(eval);
            }
        }

        private void SetMarkingCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is string eval)
            {
                var vm = (DataContext as FormulasAndTermsPageVM);
                if (vm?.SetMarkingCommand.CanExecute(eval) == true)
                    vm.SetMarkingCommand.Execute(eval);
            }
        }

        private void SetSystemFormulaCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is ConditionEvaluator eval)
            {
                var vm = (DataContext as FormulasAndTermsPageVM);
                if (vm?.SetSystemFormulaCommand.CanExecute(eval) == true)
                    vm.SetSystemFormulaCommand.Execute(eval);
            }
        }
    }
}

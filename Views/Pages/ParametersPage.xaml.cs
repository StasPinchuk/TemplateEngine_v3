using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ParametersPage.xaml
    /// </summary>
    public partial class ParametersPage : Page
    {
        public ParametersPage(INodeManager nodeManager)
        {
            InitializeComponent(); 

            DataContext = new ParametersPageVM(nodeManager);
        }

        private void SetCurrentEvaluatorCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is ConditionEvaluator eval)
            {
                var vm = (DataContext as ParametersPageVM);
                if (vm?.SetCurrentEvaluatorCommand.CanExecute(eval) == true)
                    vm.SetCurrentEvaluatorCommand.Execute(eval);
            }
        }

        private void SetMarkingCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is string eval)
            {
                var vm = (DataContext as ParametersPageVM);
                if (vm?.SetMarkingCommand.CanExecute(eval) == true)
                    vm.SetMarkingCommand.Execute(eval);
            }
        }

        private void SetSystemFormulaCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is ConditionEvaluator eval)
            {
                var vm = (DataContext as ParametersPageVM);
                if (vm?.SetSystemFormulaCommand.CanExecute(eval) == true)
                    vm.SetSystemFormulaCommand.Execute(eval);
            }
        }
    }
}

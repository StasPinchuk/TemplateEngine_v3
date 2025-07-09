using System;
using System.Windows;
using System.Windows.Controls;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для FormulasAndTermsPage.xaml
    /// </summary>
    public partial class FormulasAndTermsPage : Page
    {
        readonly FormulasAndTermsPageVM vm;

        public FormulasAndTermsPage(NodeManager nodeManager)
        {
            InitializeComponent();

            vm = new FormulasAndTermsPageVM(nodeManager);
            DataContext = vm;

            Unloaded += FormulasAndTermsPage_Unloaded;
        }

        private void SetCurrentEvaluatorCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is ConditionEvaluator eval)
            {
                var vm = DataContext as FormulasAndTermsPageVM;
                if (vm?.SetCurrentEvaluatorCommand.CanExecute(eval) == true)
                    vm.SetCurrentEvaluatorCommand.Execute(eval);
            }
        }

        private void SetMarkingCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is string eval)
            {
                var vm = DataContext as FormulasAndTermsPageVM;
                if (vm?.SetMarkingCommand.CanExecute(eval) == true)
                    vm.SetMarkingCommand.Execute(eval);
            }
        }

        private void SetSystemFormulaCommand(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is ConditionEvaluator eval)
            {
                var vm = DataContext as FormulasAndTermsPageVM;
                if (vm?.SetSystemFormulaCommand.CanExecute(eval) == true)
                    vm.SetSystemFormulaCommand.Execute(eval);
            }
        }

        private void FormulasAndTermsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (vm is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

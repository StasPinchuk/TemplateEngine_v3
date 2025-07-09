using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.VM.Pages;

namespace TemplateEngine_v3.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditParametersPreviewPage.xaml
    /// </summary>
    public partial class EditParametersPreviewPage : Page
    {
        public EditParametersPreviewPage(ConditionEvaluator evaluator, EvaluatorManager evaluatorManager, DrawerHost drawerHost)
        {
            InitializeComponent();
            DataContext = new EditParametersPreviewVM(evaluator, evaluatorManager, drawerHost);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeEvaluator treeEvaluator)
                ((EditParametersPreviewVM)DataContext).SelectedEvaluator = treeEvaluator.ConditionEvaluator;
        }
    }
}

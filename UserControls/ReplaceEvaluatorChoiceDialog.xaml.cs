using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ReplaceEvaluatorChoiceDialog.xaml
    /// </summary>
    public partial class ReplaceEvaluatorChoiceDialog : UserControl
    {
        public static DependencyProperty AllEvaluatorsProperty =
            DependencyProperty.Register(
                    "AllEvaluators",
                    typeof(IEnumerable<ConditionEvaluator>),
                    typeof(ReplaceEvaluatorChoiceDialog),
                    new PropertyMetadata(null)
                );

        public static DependencyProperty ExistingEvaluatorsProperty =
            DependencyProperty.Register(
                    "ExistingEvaluators",
                    typeof(object),
                    typeof(ReplaceEvaluatorChoiceDialog),
                    new PropertyMetadata(null)
                );

        public static DependencyProperty ReplacementEvaluatorProperty =
            DependencyProperty.Register(
                    "ReplacementEvaluator",
                    typeof(ConditionEvaluator),
                    typeof(ReplaceEvaluatorChoiceDialog),
                    new PropertyMetadata(null)
                );

        public static DependencyProperty NodesProperty =
            DependencyProperty.Register(
                    "Nodes",
                    typeof(IEnumerable<Node>),
                    typeof(ReplaceEvaluatorChoiceDialog),
                    new PropertyMetadata(new ObservableCollection<Node>())
                );

        public static DependencyProperty ExistingNodeProperty =
            DependencyProperty.Register(
                    "ExistingNode",
                    typeof(Node),
                    typeof(ReplaceEvaluatorChoiceDialog),
                    new PropertyMetadata(new Node())
                );

        public IEnumerable<ConditionEvaluator> AllEvaluators
        {
            get => (IEnumerable<ConditionEvaluator>)GetValue(AllEvaluatorsProperty);
            set => SetValue(AllEvaluatorsProperty, value);
        }

        public object ExistingEvaluators
        {
            get => (object)GetValue(ExistingEvaluatorsProperty);
            set => SetValue(ExistingEvaluatorsProperty, value);
        }

        public ConditionEvaluator ReplacementEvaluator
        {
            get => (ConditionEvaluator)GetValue(ReplacementEvaluatorProperty);
            set => SetValue(ReplacementEvaluatorProperty, value);
        }

        public IEnumerable<Node> Nodes
        {
            get => (IEnumerable<Node>)GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public Node ExistingNode
        {
            get => (Node)GetValue(ExistingNodeProperty);
            set => SetValue(ExistingNodeProperty, value);
        }

        private readonly ObservableCollection<Node> _allNodes;
        private readonly EvaluatorManager _evaluatorManager;
        private bool _isEvaluator = false;

        public ReplaceEvaluatorChoiceDialog(ObservableCollection<Node> nodes, EvaluatorManager evaluatorManager)
        {
            InitializeComponent();

            _allNodes = nodes;
            Nodes = nodes;
            _evaluatorManager = evaluatorManager;

            AllEvaluators = _evaluatorManager.AllTemplateEvaluator;
        }

        private void ToggleButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggle && toggle.IsChecked == true)
            {
                _isEvaluator = true;

                AllEvaluatorsComboBox.Visibility = Visibility.Visible;
                AllEvaluatorsTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                _isEvaluator = false;

                AllEvaluatorsComboBox.Visibility = Visibility.Collapsed;
                AllEvaluatorsTextBox.Visibility = Visibility.Visible;
            }
        }


        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Node selectedNode)
            {
                _allNodes.Add(selectedNode.DeepCopyWithNewIds());
            }
        }

        private void TreeView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject clickedElement = e.OriginalSource as DependencyObject;

            while (clickedElement != null && !(clickedElement is TreeViewItem))
            {
                if (clickedElement is Visual || clickedElement is Visual3D)
                {
                    clickedElement = VisualTreeHelper.GetParent(clickedElement);
                }
                else
                {
                    clickedElement = LogicalTreeHelper.GetParent(clickedElement);
                }
            }

            if (clickedElement is TreeViewItem item && item.DataContext is Node node)
            {
                ExistingNode = node;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string existingName = string.Empty;

            if (_isEvaluator)
            {
                ExistingEvaluators = AllEvaluatorsComboBox.SelectedItem;
                var eval = ExistingEvaluators as ConditionEvaluator;
                existingName = eval.Name;
            }
            else
            {
                ExistingEvaluators = AllEvaluatorsTextBox.Text;
                existingName = AllEvaluatorsTextBox.Text;
            }
            bool isReplace = ReplaceEvaluator(ExistingNode);

            MessageBox.Show(isReplace ? $"Замена '{existingName}' на '{ReplacementEvaluator.Name}' прошла успешно!" : $"Замена '{existingName}' на '{ReplacementEvaluator.Name}' не выполнена!");
        }

        private bool ReplaceEvaluator(Node nodeFromExisting)
        {

            string replaceName, replaceValue, replaceId = string.Empty;

            if (ExistingEvaluators is ConditionEvaluator evaluator)
            {
                replaceName = evaluator.Name;
                replaceValue = evaluator.Value;
                replaceId = evaluator.Id;
            }
            else
            {
                replaceName = ExistingEvaluators.ToString();
                replaceValue = ExistingEvaluators.ToString();
            }


            nodeFromExisting.Name = nodeFromExisting.Name.Replace(replaceName, ReplacementEvaluator.Name);
            nodeFromExisting.Designation = nodeFromExisting.Designation.Replace(replaceName, ReplacementEvaluator.Name);
            nodeFromExisting.UsageCondition = nodeFromExisting.UsageCondition.Replace(replaceName, ReplacementEvaluator.Name);

            List<ConditionEvaluator> evaluators = [];

            evaluators.AddRange(nodeFromExisting.ExpressionRepository.Formulas);
            evaluators.AddRange(nodeFromExisting.ExpressionRepository.Terms);
            evaluators.AddRange(nodeFromExisting.Parameters);

            foreach(var eval in evaluators)
            {
                if (eval.Value.Contains(replaceName) && !eval.Id.Equals(ReplacementEvaluator.Id))
                {
                    eval.Value = eval.Value.Replace(replaceName, ReplacementEvaluator.Name);
                    if(_isEvaluator)
                        eval.Parts.Remove(replaceId);
                    eval.Parts.Add(ReplacementEvaluator.Id);
                }
            }

            if (nodeFromExisting.Nodes.Count > 0)
                foreach (var node in nodeFromExisting.Nodes)
                    ReplaceEvaluator(node);

            return true;
        }
    }
}

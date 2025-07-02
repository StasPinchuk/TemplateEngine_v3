using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для CopyNodeChoiceDialog.xaml
    /// </summary>
    public partial class CopyNodeChoiceDialog : UserControl
    {
        public static DependencyProperty TemplatesProperty =
            DependencyProperty.Register(
                    "Templates",
                    typeof(IEnumerable<ReferenceModelInfo>),
                    typeof(CopyNodeChoiceDialog),
                    new PropertyMetadata(null)
                );

        public static DependencyProperty CurrentTemplateProperty =
            DependencyProperty.Register(
                "CurrentTemplate",
                typeof(ReferenceModelInfo),
                typeof(CopyNodeChoiceDialog),
                new PropertyMetadata(null, OnCurrentTemplateChanged)
            );

        public static DependencyProperty RelationsProperty =
            DependencyProperty.Register(
                    "Relations",
                    typeof(ObservableCollection<TemplateRelations>),
                    typeof(CopyNodeChoiceDialog),
                    new PropertyMetadata(new ObservableCollection<TemplateRelations>())
                );

        public static DependencyProperty SelectedRelationProperty =
            DependencyProperty.Register(
                    "SelectedRelation",
                    typeof(TemplateRelations),
                    typeof(CopyNodeChoiceDialog),
                    new PropertyMetadata(new TemplateRelations(), OnSelectedRelationChanged)
                );

        public static DependencyProperty NodesProperty =
            DependencyProperty.Register(
                    "Nodes",
                    typeof(ObservableCollection<Node>),
                    typeof(CopyNodeChoiceDialog),
                    new PropertyMetadata(new ObservableCollection<Node>())
                );

        public IEnumerable<ReferenceModelInfo> Templates
        {
            get => (IEnumerable<ReferenceModelInfo>)GetValue(TemplatesProperty);
            set => SetValue(TemplatesProperty, value);
        }

        public ReferenceModelInfo CurrentTemplate
        {
            get => (ReferenceModelInfo)GetValue(CurrentTemplateProperty);
            set => SetValue(CurrentTemplateProperty, value);
        }

        public ObservableCollection<TemplateRelations> Relations
        {
            get => (ObservableCollection<TemplateRelations>)GetValue(RelationsProperty);
            set => SetValue(RelationsProperty, value);
        }

        public ObservableCollection<Node> Nodes
        {
            get => (ObservableCollection<Node>)GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public TemplateRelations SelectedRelation
        {
            get => (TemplateRelations)GetValue(SelectedRelationProperty);
            set => SetValue(SelectedRelationProperty, value);
        }

        private readonly ObservableCollection<Node> _allNodes;

        public CopyNodeChoiceDialog(IEnumerable<ReferenceModelInfo> templates, ObservableCollection<Node> nodes)
        {
            InitializeComponent();
            Templates = templates;
            _allNodes = nodes;
        }

        public CopyNodeChoiceDialog(Template currentTemplate, ObservableCollection<Node> nodes)
        {
            InitializeComponent();
            Relations = currentTemplate.TemplateRelations;
            _allNodes = nodes;
            TemplateComboBox.Visibility = Visibility.Collapsed;
        }

        private static void OnCurrentTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CopyNodeChoiceDialog)d;
            if (e.NewValue is ReferenceModelInfo reference)
            {
                var template = new JsonSerializer().Deserialize<Template>(reference.ObjectStruct);
                control.Relations = template.TemplateRelations;
                control.SelectedRelation = template.TemplateRelations.FirstOrDefault();
            }
        }

        private static void OnSelectedRelationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CopyNodeChoiceDialog)d;
            if (e.NewValue is TemplateRelations relations)
            {
                control.Nodes = relations.Nodes;
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

                _allNodes.Add(node.DeepCopyWithNewIds());
            }
        }
    }
}

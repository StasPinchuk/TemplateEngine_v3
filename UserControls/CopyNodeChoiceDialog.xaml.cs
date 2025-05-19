using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.UserControls
{
    /// <summary>
    /// Логика взаимодействия для CopyNodeChoiceDialog.xaml
    /// </summary>
    public partial class CopyNodeChoiceDialog : UserControl
    {
        private INodeManager _nodeManager;

        public static DependencyProperty TemplatesProperty =
            DependencyProperty.Register(
                    "Templates",
                    typeof(ObservableCollection<ReferenceModelInfo>),
                    typeof(CopyNodeChoiceDialog),
                    new PropertyMetadata(new ObservableCollection<ReferenceModelInfo>())
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

        public ObservableCollection<ReferenceModelInfo> Templates
        {
            get => (ObservableCollection<ReferenceModelInfo>)GetValue( TemplatesProperty );
            set => SetValue( TemplatesProperty, value );
        }

        public ReferenceModelInfo CurrentTemplate
        {
            get => (ReferenceModelInfo)GetValue(CurrentTemplateProperty);
            set => SetValue(CurrentTemplateProperty, value);
        }

        public ObservableCollection<TemplateRelations> Relations
        {
            get => (ObservableCollection<TemplateRelations>)GetValue(RelationsProperty);
            set => SetValue(RelationsProperty, value );
        }

        public ObservableCollection<Node> Nodes
        {
            get => (ObservableCollection<Node>)GetValue(NodesProperty);
            set => SetValue(NodesProperty, value );
        }

        public TemplateRelations SelectedRelation
        {
            get => (TemplateRelations)GetValue(SelectedRelationProperty);
            set => SetValue(SelectedRelationProperty, value );
        }

        public CopyNodeChoiceDialog(ObservableCollection<ReferenceModelInfo> templates, INodeManager nodeManager)
        {
            InitializeComponent();
            Templates = templates;
            _nodeManager = nodeManager;
        }

        public CopyNodeChoiceDialog(Template currentTemplate, INodeManager nodeManager)
        {
            InitializeComponent();
            Relations = currentTemplate.TemplateRelations;
            _nodeManager = nodeManager;
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
                _nodeManager.Nodes.Add( selectedNode.DeepCopyWithNewIds() );
            }
        }

    }
}

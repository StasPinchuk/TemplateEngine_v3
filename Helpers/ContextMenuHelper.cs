using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Helpers
{
    public class ContextMenuHelper
    {
        private ContextMenu _currentContext;
        private readonly Template _template;
        private readonly MaterialManager _materialManager;

        public ConditionEvaluator CurrentEvaluator { get; set; }

        private ICommand SetEvaluatorTextBoxCommand { get; set; }

        public ContextMenuHelper(Template template, MaterialManager materialManager)
        {
            _template = template;
            _materialManager = materialManager;

            SetEvaluatorTextBoxCommand = new RelayCommand(SetEvaluatorTextBox);
        }

        public void CreateContextMenu()
        {
            var contextMenu = new ContextMenu() { Cursor = Cursors.Hand };
            var contextItemsList = new List<MenuItem>();

            contextItemsList.Add(CreateAttributeItems());
            contextItemsList.Add(CreateMaterialsMenu());

            var templateRelations = _template.TemplateRelations;

            contextItemsList.AddRange(CreateTemplateRelationsItems(templateRelations));

            contextMenu.ItemsSource = contextItemsList;

            _currentContext = contextMenu;
        }

        private MenuItem CreateAttributeItems()
        {
            var attributeMenuItem = new MenuItem() { Header = "Параметры шаблона" };

            foreach (var attribute in _template.ProductMarkingAttributes)
            {
                var menuItem = new MenuItem()
                {
                    Header = attribute,
                    Command = SetEvaluatorTextBoxCommand,
                    CommandParameter = attribute
                };

                attributeMenuItem.Items.Add(menuItem);
            }

            return attributeMenuItem;
        }

        private List<MenuItem> CreateTemplateRelationsItems(ObservableCollection<TemplateRelations> templateRelations)
        {
            List<MenuItem> templateRelationsList = [];
            foreach (var templateRelation in templateRelations)
            {
                var menuItem = new MenuItem()
                {
                    Header = templateRelation.Designation
                };

                menuItem.ItemsSource = CreateNodeItems(templateRelation.Nodes);

                templateRelationsList.Add(menuItem);
            }

            return templateRelationsList;
        }

        private MenuItem CreateMaterialsMenu()
        {
            var materialMenuItem = new MenuItem() { Header = "Материалы" };
            var materials = new Dictionary<string, List<string>>();

            foreach (string material in _materialManager.GetMaterials())
            {
                string type = material.Split(new[] { " ", "-" }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (!materials.ContainsKey(type))
                    materials[type] = new List<string>();

                materials[type].Add(material);
            }

            foreach (var group in materials)
            {
                var groupItem = new MenuItem() { Header = group.Key };

                foreach (var material in group.Value)
                {
                    groupItem.Items.Add(new MenuItem()
                    {
                        Header = material,
                        Command = SetEvaluatorTextBoxCommand,
                        CommandParameter = material
                    });
                }

                materialMenuItem.Items.Add(groupItem);
            }

            return materialMenuItem;
        }

        private List<MenuItem> CreateNodeItems(ObservableCollection<Node> nodes)
        {
            List<MenuItem> nodesItems = [];

            foreach (var node in nodes)
            {
                var nodeItem = new MenuItem()
                {
                    Header = node.Name,
                    ToolTip = node.NodeComment,
                };

                foreach (var termOrFormula in CreateEvaluatorItems(node.ExpressionRepository))
                {
                    nodeItem.Items.Add(termOrFormula);
                }

                if (node.Nodes.Count > 0)
                {
                    foreach (var item in CreateNodeItems(node.Nodes))
                    {
                        nodeItem.Items.Add(item);
                    }
                }

                nodesItems.Add(nodeItem);
            }

            return nodesItems;
        }

        private List<MenuItem> CreateEvaluatorItems(ExpressionRepository expression)
        {
            var formulasItems = new MenuItem() { Header = "Формулы" };
            var termsItems = new MenuItem() { Header = "Условия" };

            formulasItems.ItemsSource = ConvertEvaluator(expression.Formulas);
            termsItems.ItemsSource = ConvertEvaluator(expression.Terms);

            return new List<MenuItem>() { formulasItems, termsItems };
        }

        private List<MenuItem> ConvertEvaluator(ObservableCollection<ConditionEvaluator> evaluators)
        {
            var convertList = new List<MenuItem>();
            foreach (var condition in evaluators)
            {
                var conditionItem = new MenuItem()
                {
                    Header = condition.Name,
                    ToolTip = condition.Value,
                    Command = SetEvaluatorTextBoxCommand,
                    CommandParameter = condition
                };

                convertList.Add(conditionItem);
            }

            return convertList;
        }

        public void UpdateContextMenu()
        {
            CreateContextMenu();
        }

        private MenuItem CloneMenuItem(MenuItem source)
        {
            var clone = new MenuItem
            {
                Header = source.Header,
                Command = source.Command,
                CommandParameter = source.CommandParameter,
                IsEnabled = source.IsEnabled,
                Icon = source.Icon,
                // и другие свойства по необходимости
            };

            foreach (var item in source.Items)
            {
                if (item is MenuItem subMenuItem)
                    clone.Items.Add(CloneMenuItem(subMenuItem));
                else
                    clone.Items.Add(item); // или обработать иначе, если не MenuItem
            }

            return clone;
        }

        public ContextMenu GetContextMenuCopy()
        {
            /*var contextMenu = new ContextMenu();

            foreach (var item in _currentContext.Items)
            {
                if (item is MenuItem menuItem)
                    contextMenu.Items.Add(CloneMenuItem(menuItem));
                else
                    contextMenu.Items.Add(item);
            }*/

            return _currentContext;
        }



        private void SetEvaluatorTextBox(object parameter)
        {
            if (_currentContext != null && _currentContext.PlacementTarget is TextBox textBox)
            {
                if (parameter is ConditionEvaluator evaluator)
                {
                    textBox.Text += $"[{evaluator.Name}]";
                    var condition = GetBoundPropertyObject(textBox, getParent: true) as ConditionEvaluator;
                    if (condition != null)
                    {
                        condition.Parts.Add(evaluator.Id);
                    }

                }

                if (parameter is string material)
                {
                    if (material.Contains(' '))
                        textBox.Text += $"'{material}'";
                    else
                        textBox.Text += $"[{material}]";
                }

            }
        }

        public object? GetBoundPropertyObject(TextBox textBox, bool getParent = false)
        {
            var binding = BindingOperations.GetBinding(textBox, TextBox.TextProperty);
            if (binding == null) return null;

            var dataContext = textBox.DataContext;
            if (dataContext == null) return null;

            var pathParts = binding.Path.Path.Split('.');
            if (getParent && pathParts.Length > 1)
                pathParts = pathParts.Take(pathParts.Length - 1).ToArray();

            object? currentObject = dataContext;
            foreach (var propName in pathParts)
            {
                if (currentObject == null) return null;

                var propInfo = currentObject.GetType().GetProperty(propName);
                if (propInfo == null) return null;

                currentObject = propInfo.GetValue(currentObject);
            }

            return currentObject;
        }

    }
}


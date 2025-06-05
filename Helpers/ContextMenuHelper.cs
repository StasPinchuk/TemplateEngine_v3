using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        public object DataContext { get; set; }

        private ICommand SetEvaluatorTextBoxCommand { get; set; }

        public ContextMenuHelper(Template template, MaterialManager materialManager)
        {
            _template = template;
            _materialManager = materialManager;

            SetEvaluatorTextBoxCommand = new RelayCommand(SetEvaluatorTextBox);
        }

        public async Task CreateContextMenuAsync()
        {
            var contextMenu = new ContextMenu { Cursor = Cursors.Hand };
            var contextItemsList = new List<MenuItem>();

            contextItemsList.Add(CreateAttributeItems());
            contextItemsList.Add(CreateMaterialsMenu());

            var templateRelations = await GetTemplateRelationsAsync();
            contextItemsList.AddRange(CreateTemplateRelationsItems(templateRelations));

            contextMenu.ItemsSource = contextItemsList;
            _currentContext = contextMenu;
        }

        private MenuItem CreateAttributeItems()
        {
            var attributeMenuItem = new MenuItem { Header = "Параметры шаблона" };
            bool isLoaded = false;

            attributeMenuItem.Items.Add(new MenuItem { Header = "Загрузка..." });

            attributeMenuItem.SubmenuOpened += (s, e) =>
            {
                if (isLoaded) return; // Уже загрузили, ничего не делать

                attributeMenuItem.Items.Clear();

                foreach (var attribute in _template.ProductMarkingAttributes)
                {
                    attributeMenuItem.Items.Add(new MenuItem
                    {
                        Header = attribute,
                        Command = SetEvaluatorTextBoxCommand,
                        CommandParameter = attribute
                    });
                }

                isLoaded = true;
            };

            return attributeMenuItem;
        }



        private List<MenuItem> CreateTemplateRelationsItems(IEnumerable<TemplateRelations> templateRelations)
        {
            var templateRelationsList = new List<MenuItem>();

            foreach (var templateRelation in templateRelations)
            {
                var menuItem = new MenuItem
                {
                    Header = templateRelation.Designation,
                    Tag = false // флаг загрузки
                };

                // Заглушка, чтобы стрелка появилась
                menuItem.Items.Add(new MenuItem { Header = "Загрузка...", IsEnabled = false });

                menuItem.SubmenuOpened += (s, e) =>
                {
                    var item = (MenuItem)s;
                    if ((bool)item.Tag!) return;

                    item.Items.Clear(); 

                    foreach (var node in templateRelation.Nodes)
                    {
                        var childItem = CreateLazyNodeMenuItem(node);
                        if (childItem != null)
                            menuItem.Items.Add(childItem);

                    }

                    item.Tag = true;
                };

                templateRelationsList.Add(menuItem);
            }

            return templateRelationsList;
        }



        public async Task<List<TemplateRelations>> GetTemplateRelationsAsync()
        {
            return await Task.FromResult(_template.TemplateRelations.ToList());
        }


        private MenuItem CreateMaterialsMenu()
        {
            var materialsMenuItem = new MenuItem { Header = "Материалы" };
            bool isLoaded = false;

            materialsMenuItem.Items.Add(new MenuItem { Header = "Загрузка..." });

            materialsMenuItem.SubmenuOpened += async (s, e) =>
            {
                if (isLoaded) return; // Чтобы не загружать повторно

                materialsMenuItem.Items.Clear();

                var materials = new Dictionary<string, List<string>>();
                var materialList = await Task.Run(() => _materialManager.GetMaterials());

                foreach (string material in materialList)
                {
                    string type = material.Split(new[] { " ", "-" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    if (!materials.ContainsKey(type))
                        materials[type] = new List<string>();

                    materials[type].Add(material);
                }

                foreach (var group in materials)
                {
                    var groupItem = new MenuItem { Header = group.Key };

                    foreach (var material in group.Value)
                    {
                        groupItem.Items.Add(new MenuItem
                        {
                            Header = material,
                            Command = SetEvaluatorTextBoxCommand,
                            CommandParameter = material
                        });
                    }

                    materialsMenuItem.Items.Add(groupItem);
                }

                isLoaded = true;
            };

            return materialsMenuItem;
        }

        private MenuItem? CreateLazyNodeMenuItem(Node node)
        {
            // Рекурсивно проверим, есть ли у подузлов хоть что-то содержательное
            bool HasValidSubNode(Node n)
            {
                return n.ExpressionRepository.Formulas.Any()
                    || n.ExpressionRepository.Terms.Any()
                    || n.Nodes.Any(HasValidSubNode);
            }

            // Проверяем, стоит ли вообще создавать пункт меню
            bool hasFormulasOrTerms = node.ExpressionRepository.Formulas.Any() || node.ExpressionRepository.Terms.Any();
            bool hasValidSubNodes = node.Nodes.Any(HasValidSubNode);

            if (!hasFormulasOrTerms && !hasValidSubNodes)
                return null;

            var menuItem = new MenuItem
            {
                Header = !string.IsNullOrEmpty(node.Designation) ? $"{node.Name}\n{node.Designation}" : $"{node.Name}",
                ToolTip = !string.IsNullOrEmpty(node.NodeComment) ? node.NodeComment : null
            };

            // Добавляем выражения
            foreach (var item in CreateEvaluatorItems(node.ExpressionRepository))
                menuItem.Items.Add(item);

            // Добавляем только валидные подузлы
            foreach (var subNode in node.Nodes)
            {
                var childItem = CreateLazyNodeMenuItem(subNode);
                if (childItem != null)
                    menuItem.Items.Add(childItem);
            }

            return menuItem;
        }


        private List<MenuItem> CreateEvaluatorItems(ExpressionRepository expression)
        {
            var items = new List<MenuItem>();

            if (expression.Formulas.Any())
            {
                var formulasMenu = new MenuItem { Header = "Формулы" };

                foreach (var formula in expression.Formulas)
                {
                    formulasMenu.Items.Add(new MenuItem
                    {
                        Header = formula.Name,
                        ToolTip = formula.Value,
                        Command = SetEvaluatorTextBoxCommand,
                        CommandParameter = formula
                    });
                }

                items.Add(formulasMenu);
            }

            if (expression.Terms.Any())
            {
                var termsMenu = new MenuItem { Header = "Условия" };

                foreach (var term in expression.Terms)
                {
                    termsMenu.Items.Add(new MenuItem
                    {
                        Header = term.Name,
                        ToolTip = term.Value,
                        Command = SetEvaluatorTextBoxCommand,
                        CommandParameter = term
                    });
                }

                items.Add(termsMenu);
            }

            return items;
        }

        public async Task UpdateContextMenuAsync()
        {
            await CreateContextMenuAsync();
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

        public ContextMenu GetContextMenu()
        {
            return _currentContext;
        }

        public void ClearContextMenu()
        {
            _currentContext = null;
        }

        private void SetEvaluatorTextBox(object parameter)
        {
            if (_currentContext != null && _currentContext.PlacementTarget is TextBox textBox)
            {
                if (parameter is ConditionEvaluator evaluator)
                {
                    var condition = GetBoundPropertyObject(textBox, getParent: true) as ConditionEvaluator;
                    if (condition != null)
                    {
                        textBox.Text += $"[{evaluator.Name}]";
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
            var bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpr == null) return null;

            var bindingPath = bindingExpr.ParentBinding.Path?.Path;
            if (string.IsNullOrWhiteSpace(bindingPath)) return null;

            var pathParts = bindingPath.Split('.');
            if (getParent && pathParts.Length > 1)
                pathParts = pathParts.Take(pathParts.Length - 1).ToArray();

            // Попробовать сначала с DataItem
            object? result = TryResolvePath(bindingExpr.DataItem, pathParts);
            if (result != null) return result;

            // Если не получилось — попробовать с DataContext
            return TryResolvePath(DataContext, pathParts);
        }

        private object? TryResolvePath(object? root, string[] pathParts)
        {
            object? current = root;

            foreach (var part in pathParts)
            {
                if (current == null) return null;

                var propInfo = current.GetType().GetProperty(part);
                if (propInfo == null) return null;

                current = propInfo.GetValue(current);
            }

            return current;
        }



    }
}


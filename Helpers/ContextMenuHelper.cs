using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TemplateEngine_v3.Command;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Helpers
{
    /// <summary>
    /// Помощник для создания и управления контекстным меню с динамическими элементами,
    /// такими как параметры шаблона, материалы и связи шаблонов.
    /// </summary>
    public class ContextMenuHelper
    {
        private ContextMenu _currentContext;
        private readonly Template _template;
        private readonly MaterialManager _materialManager;

        /// <summary>
        /// Текущий выбранный условный вычислитель.
        /// </summary>
        public ConditionEvaluator CurrentEvaluator { get; set; }

        /// <summary>
        /// Внешний контекст данных, используемый для разрешения биндингов.
        /// </summary>
        public object DataContext { get; set; }

        private ICommand SetEvaluatorTextBoxCommand { get; set; }

        /// <summary>
        /// Создает новый экземпляр помощника для контекстного меню.
        /// </summary>
        /// <param name="template">Шаблон, на основе которого создаются параметры меню.</param>
        /// <param name="materialManager">Менеджер материалов для загрузки списка материалов.</param>
        public ContextMenuHelper(Template template, MaterialManager materialManager)
        {
            _template = template;
            _materialManager = materialManager;

            SetEvaluatorTextBoxCommand = new RelayCommand(SetEvaluatorTextBox);
        }

        /// <summary>
        /// Асинхронно создает новое контекстное меню с элементами.
        /// </summary>
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

        /// <summary>
        /// Создает пункт меню с параметрами шаблона, подгружаемыми при открытии.
        /// </summary>
        /// <returns>Элемент меню с параметрами шаблона.</returns>
        private MenuItem CreateAttributeItems()
        {
            var attributeMenuItem = new MenuItem { Header = "Параметры шаблона" };
            bool isLoaded = false;

            // Добавляем заглушку "Загрузка..."
            attributeMenuItem.Items.Add(new MenuItem { Header = "Загрузка..." });

            // При открытии меню загружаем параметры, если они еще не загружены
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

        /// <summary>
        /// Создает пункты меню для связей шаблонов.
        /// </summary>
        /// <param name="templateRelations">Список связей шаблонов.</param>
        /// <returns>Список элементов меню с отношениями шаблонов.</returns>
        private List<MenuItem> CreateTemplateRelationsItems(IEnumerable<TemplateRelations> templateRelations)
        {
            var templateRelationsList = new List<MenuItem>();

            foreach (var templateRelation in templateRelations)
            {
                var menuItem = new MenuItem
                {
                    Header = templateRelation.Designation,
                    Tag = false // флаг загрузки подменю
                };

                // Заглушка, чтобы стрелка появилась
                menuItem.Items.Add(new MenuItem { Header = "Загрузка...", IsEnabled = false });

                // При открытии подменю загружаем дочерние узлы
                menuItem.SubmenuOpened += (s, e) =>
                {
                    var item = (MenuItem)s;
                    if ((bool)item.Tag!) return; // Уже загружено

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

        /// <summary>
        /// Асинхронно получает список связей шаблонов из текущего шаблона.
        /// </summary>
        /// <returns>Список связей шаблонов.</returns>
        public async Task<List<TemplateRelations>> GetTemplateRelationsAsync()
        {
            return await Task.FromResult(_template.TemplateRelations.ToList());
        }

        /// <summary>
        /// Создает пункт меню с материалами, сгруппированными по типу, подгружаемыми при открытии.
        /// </summary>
        /// <returns>Элемент меню с материалами.</returns>
        private MenuItem CreateMaterialsMenu()
        {
            var materialsMenuItem = new MenuItem { Header = "Материалы" };
            bool isLoaded = false;

            // Заглушка для отображения
            materialsMenuItem.Items.Add(new MenuItem { Header = "Загрузка..." });

            // При открытии подменю загружаем материалы
            materialsMenuItem.SubmenuOpened += async (s, e) =>
            {
                if (isLoaded) return; // Чтобы не загружать повторно

                materialsMenuItem.Items.Clear();

                var materials = new Dictionary<string, List<string>>();
                var materialList = await Task.Run(() => _materialManager.GetMaterials());

                foreach (string material in materialList)
                {
                    // Определяем тип материала по первой части строки
                    string type = material.Split(new[] { " ", "-" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    if (!materials.ContainsKey(type))
                        materials[type] = new List<string>();

                    materials[type].Add(material);
                }

                // Создаем группы и добавляем материалы
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

        /// <summary>
        /// Рекурсивно создает пункты меню для узлов с выражениями и подузлами.
        /// Возвращает null, если узел и подузлы не содержат формул и условий.
        /// </summary>
        /// <param name="node">Узел, для которого создается пункт меню.</param>
        /// <returns>Пункт меню или null.</returns>
        private MenuItem? CreateLazyNodeMenuItem(Node node)
        {
            // Вспомогательная функция для проверки наличия валидных подузлов
            bool HasValidSubNode(Node n)
            {
                return n.ExpressionRepository.Formulas.Any()
                    || n.ExpressionRepository.Terms.Any()
                    || n.Nodes.Any(HasValidSubNode);
            }

            // Проверяем, есть ли формулы или условия у узла или его подузлов
            bool hasFormulasOrTerms = node.ExpressionRepository.Formulas.Any() || node.ExpressionRepository.Terms.Any();
            bool hasValidSubNodes = node.Nodes.Any(HasValidSubNode);

            if (!hasFormulasOrTerms && !hasValidSubNodes)
                return null;

            var menuItem = new MenuItem
            {
                Header = !string.IsNullOrEmpty(node.Designation) ? $"{node.Name}\n{node.Designation}" : $"{node.Name}",
                ToolTip = !string.IsNullOrEmpty(node.NodeComment) ? node.NodeComment : null
            };

            // Добавляем пункты меню для формул и условий узла
            foreach (var item in CreateEvaluatorItems(node.ExpressionRepository))
                menuItem.Items.Add(item);

            // Рекурсивно добавляем валидные подузлы
            foreach (var subNode in node.Nodes)
            {
                var childItem = CreateLazyNodeMenuItem(subNode);
                if (childItem != null)
                    menuItem.Items.Add(childItem);
            }

            return menuItem;
        }

        /// <summary>
        /// Создает пункты меню для формул и условий из репозитория выражений.
        /// </summary>
        /// <param name="expression">Репозиторий выражений.</param>
        /// <returns>Список пунктов меню для формул и условий.</returns>
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

        /// <summary>
        /// Асинхронно обновляет текущее контекстное меню, пересоздавая его.
        /// </summary>
        public async Task UpdateContextMenuAsync()
        {
            await CreateContextMenuAsync();
        }

        /// <summary>
        /// Клонирует пункт меню вместе со всеми вложенными элементами.
        /// </summary>
        /// <param name="source">Исходный пункт меню для клонирования.</param>
        /// <returns>Новый клон пункта меню.</returns>
        private MenuItem CloneMenuItem(MenuItem source)
        {
            var clone = new MenuItem
            {
                Header = source.Header,
                Command = source.Command,
                CommandParameter = source.CommandParameter,
                IsEnabled = source.IsEnabled,
                Icon = source.Icon,
            };

            foreach (var item in source.Items)
            {
                if (item is MenuItem subMenuItem)
                    clone.Items.Add(CloneMenuItem(subMenuItem));
                else
                    clone.Items.Add(item);
            }

            return clone;
        }

        /// <summary>
        /// Получает текущее контекстное меню.
        /// </summary>
        /// <returns>Текущее контекстное меню.</returns>
        public ContextMenu GetContextMenu()
        {
            return _currentContext;
        }

        /// <summary>
        /// Очищает текущее контекстное меню, устанавливая его в null.
        /// </summary>
        public void ClearContextMenu()
        {
            _currentContext = null;
        }

        /// <summary>
        /// Вставляет выбранный элемент (формулу, условие или материал) в текстовое поле,
        /// связанное с контекстным меню.
        /// </summary>
        /// <param name="parameter">Объект, выбранный в меню (формула, условие или материал).</param>
        private void SetEvaluatorTextBox(object parameter)
        {
            if (_currentContext != null && _currentContext.PlacementTarget is TextBox textBox)
            {
                if (parameter is ConditionEvaluator evaluator)
                {
                    var condition = GetBoundPropertyObject(textBox, getParent: true);
                    if (condition is ConditionEvaluator cond)
                    {
                        textBox.Text += $"[{evaluator.Name}]";
                        cond.Parts.Add(evaluator.Id);
                    }
                    if(condition is string)
                        textBox.Text += $"[{evaluator.Name}]";
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

        /// <summary>
        /// Получает объект, к которому привязано свойство Text у TextBox, используя биндинг.
        /// Если getParent == true, возвращает объект, являющийся родителем для свойства Text.
        /// </summary>
        /// <param name="textBox">Текстовое поле с биндингом.</param>
        /// <param name="getParent">Если true, получает родительский объект по пути биндинга.</param>
        /// <returns>Объект, к которому привязано свойство, или null.</returns>
        public object? GetBoundPropertyObject(TextBox textBox, bool getParent = false)
        {
            var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);

            if (bindingExpression != null)
            {
                var path = bindingExpression.ParentBinding.Path.Path;

                DataContext = bindingExpression.DataItem;

                if (DataContext != null)
                {
                    var parts = path.Split('.');
                    if(parts.Length > 2)
                        if (getParent)
                            parts = parts.Take(parts.Length - 1).ToArray();

                    return TryResolvePath(DataContext, parts);
                }
            }

            return null;
        }

        /// <summary>
        /// Рекурсивно пытается получить значение свойства из объекта по цепочке имен свойств.
        /// </summary>
        /// <param name="root">Объект, с которого начинается поиск.</param>
        /// <param name="pathParts">Части пути свойства.</param>
        /// <returns>Значение свойства или null.</returns>
        private object? TryResolvePath(object? root, string[] pathParts)
        {
            if (root == null) return null;
            if (pathParts.Length == 0) return root;

            var property = root.GetType().GetProperty(pathParts[0]);
            if (property == null) return null;

            var value = property.GetValue(root);
            return TryResolvePath(value, pathParts.Skip(1).ToArray());
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using TemplateEngine_v3.Services;

namespace TemplateEngine_v3.Models
{
    public class Node : BaseNotifyPropertyChanged
    {
        [JsonIgnore] // Исключаем из JSON
        private bool _parameterLoaded = false;
        [JsonIgnore] // Исключаем из JSON
        private Action _updateMenuHandler;
        [JsonIgnore] // Исключаем из JSON
        public Action UpdateMenuHandler
        {
            get => _updateMenuHandler;
            set => SetValue(ref _updateMenuHandler, value, nameof(UpdateMenuHandler));
        }
        [JsonIgnore]
        private bool _isInitialized = false;

        public event Action CurrentNodeChanged;
        /// <summary>
        /// Уникальный идентификатор узла.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Название узла.
        /// </summary>
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (!string.IsNullOrEmpty(_name) && !_name.Equals(value))
                {
                }
                if(_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                    UpdateMenuHandler?.Invoke();
                }
            }
        }

        /// <summary>
        /// Обозначение узла.
        /// </summary>
        private string _designation = string.Empty;
        public string Designation
        {
            get => _designation;
            set
            {     
                _designation = value;
                OnPropertyChanged(nameof(Designation));
            }

        }

        /// <summary>
        /// Тип узла.
        /// </summary>
        private string _type = "Сборочная единица";
        public string Type
        {
            get => _type;
            set
            {
                if (_type == value)
                    return;

                if (value.Equals("Прочее изделие"))
                    Parameters.Clear();

                _type = value;
                OnPropertyChanged(nameof(Type));
                UpdateMenuHandler?.Invoke();
            }

        }

        /// <summary>
        /// Количество узлов.
        /// </summary>
        private ConditionEvaluator _amount = new();
        public ConditionEvaluator Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        /// <summary>
        /// Коллекция технологий, связанных с узлом.
        /// </summary>
        private Technologies _technologies = new();
        public Technologies Technologies
        {
            get => _technologies;
            set
            {
                _technologies = value;
                OnPropertyChanged(nameof(Technologies));
            }
        }

        /// <summary>
        /// Коллекция параметров, связанных с узлом.
        /// </summary>
        private ObservableCollection<ConditionEvaluator> _parameters = [];
        public ObservableCollection<ConditionEvaluator> Parameters
        {
            get => _parameters;
            set
            {
                _parameters = value;
            }
        }


        /// <summary>
        /// Коллекция узлов, вложенных в текущий узел.
        /// </summary>
        public ObservableCollection<Node> Nodes { get; set; } = [];

        /// <summary>
        /// Спецификации деталей узла.
        /// </summary>
        private ExpressionRepository _expressionRepository = new();
        public ExpressionRepository ExpressionRepository
        {
            get => _expressionRepository;
            set
            {
                _expressionRepository = value;
                OnPropertyChanged(nameof(ExpressionRepository));
            }
        }

        /// <summary>
        /// Условия использования узла.
        /// </summary>
        private string _usageCondition = string.Empty;
        public string UsageCondition
        {
            get => _usageCondition;
            set
            {
                if (!string.IsNullOrEmpty(_usageCondition) && !_usageCondition.Equals(value))
                {
                }
                if (_usageCondition != value)
                {
                    _usageCondition = value;
                    OnPropertyChanged(nameof(UsageCondition));
                    UpdateMenuHandler?.Invoke();
                }
            }
        }

        private string _nodeComment = string.Empty;
        public string NodeComment
        {
            get => _nodeComment;
            set
            {
                if (!string.IsNullOrEmpty(_nodeComment) && !_nodeComment.Equals(value))
                {
                }
                if (_nodeComment != value)
                {
                    _nodeComment = value;
                    OnPropertyChanged(nameof(NodeComment));
                    UpdateMenuHandler?.Invoke();
                }
            }
        }

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Node()
        {
        }

        public Node Copy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Node>(json);
        }

        public void SetValue(Node node)
        {
            Name = node.Name;
            Type = node.Type;
            Designation = node.Designation;
            Amount = node.Amount.Copy();
            Technologies = node.Technologies.Copy();
            Parameters = new(node.Parameters);
            Nodes = new(node.Nodes);
            ExpressionRepository = node.ExpressionRepository.Copy();
            UsageCondition = node.UsageCondition;
            NodeComment = node.NodeComment;
        }

        public Node DeepCopyWithNewIds()
        {
            // Сериализуем объект в JSON и десериализуем, чтобы получить глубокую копию
            string json = JsonConvert.SerializeObject(this);
            Node copiedNode = JsonConvert.DeserializeObject<Node>(json);

            // Генерация новых Id для текущего узла
            ReplaceIds(copiedNode);

            return copiedNode;
        }

        /// <summary>
        /// Рекурсивно заменяет Id для узла и всех вложенных элементов.
        /// </summary>
        /// <param name="node">Узел, в котором нужно заменить Id.</param>
        private void ReplaceIds(Node node)
        {
            // Генерация нового Id для текущего узла
            node.Id = Guid.NewGuid();

            // Замена Id в Amount
            if (node.Amount != null)
            {
                node.Amount.Id = Guid.NewGuid().ToString();
            }

            // Словари для отслеживания замен Id
            Dictionary<string, string> idReplacements = [];
            if(node.Parameters != null)
            {
                // Замена Id для параметров
                foreach (var parameter in node.Parameters)
                {
                    string oldId = parameter.Id;
                    string newId = Guid.NewGuid().ToString();
                    idReplacements[oldId] = newId;
                    parameter.Id = newId;

                    // Если параметры ссылаются на другие Id, заменяем их
                    ReplaceIdsInParts(parameter.Parts, idReplacements);
                }

            }

            // Замена Id для формул
            foreach (var formula in node.ExpressionRepository.Formulas)
            {
                string oldId = formula.Id;
                string newId = Guid.NewGuid().ToString();
                idReplacements[oldId] = newId;
                formula.Id = newId;

                // Заменяем старый Id на новый в частях формулы
                ReplaceIdsInParts(formula.Parts, idReplacements);
            }

            // Замена Id для условий
            foreach (var term in node.ExpressionRepository.Terms)
            {
                string oldId = term.Id;
                string newId =  Guid.NewGuid().ToString();
                idReplacements[oldId] = newId;
                term.Id = newId;

                // Заменяем старый Id на новый в частях условия
                ReplaceIdsInParts(term.Parts, idReplacements);
            }

            var technologies = node.Technologies;
            technologies.Id = Guid.Empty;

            foreach (var operation in technologies.Operations)
            {
                operation.Id = Guid.NewGuid().ToString();
                foreach (var division in operation.BranchDivisionDetails)
                {
                    division.Id = Guid.NewGuid().ToString();

                    // Замена Id в материалах
                    var material = division.Materials;
                    if (material != null)
                    {
                        material.Name.Id = Guid.NewGuid().ToString();
                        material.Consumption.Id = Guid.NewGuid().ToString();

                        // Заменяем Id в Parts материалов
                        ReplaceIdsInParts(material.Name.Parts, idReplacements);
                        ReplaceIdsInParts(material.Consumption.Parts, idReplacements);
                    }
                }
            }

            ReplaceIdsInParts(node.Amount.Parts, idReplacements);

            // Повторно проходим по всем формулам, условиям и параметрам для обновления ссылок
            foreach (var formula in node.ExpressionRepository.Formulas)
            {
                ReplaceIdsInParts(formula.Parts, idReplacements);
            }

            foreach (var term in node.ExpressionRepository.Terms)
            {
                ReplaceIdsInParts(term.Parts, idReplacements);
            }
            if(node.Parameters != null)
                foreach (var parameter in node.Parameters)
                {
                    ReplaceIdsInParts(parameter.Parts, idReplacements);
                }

            // Рекурсивная замена Id для вложенных узлов
            foreach (var childNode in node.Nodes)
            {
                ReplaceIds(childNode);
            }
        }

        /// <summary>
        /// Заменяет старые Id на новые в списке частей, используя словарь замен.
        /// </summary>
        /// <param name="parts">Список частей.</param>
        /// <param name="idReplacements">Словарь замен старых Id на новые.</param>
        private void ReplaceIdsInParts(IList<string> parts, Dictionary<string, string> idReplacements)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                if (idReplacements.TryGetValue(parts[i], out string newId))
                {
                    parts[i] = newId;
                }
            }
        }
        [JsonIgnore]
        private ObservableCollection<GroupNode> _groups;
        [JsonIgnore]
        public ObservableCollection<GroupNode> Groups
        {
            get
            {
                _groups = new ObservableCollection<GroupNode>();

                if (Technologies?.Operations?.Count > 0)
                {
                    var group = new GroupNode("ТП", new ObservableCollection<object>(Technologies.Operations.Cast<object>()));
                    _groups.Add(group);
                    Technologies.Operations.CollectionChanged += (s, e) => SyncGroup("ТП", Technologies.Operations);
                }

                if (Parameters?.Count > 0)
                {
                    var group = new GroupNode("Параметры", new ObservableCollection<object>(Parameters.Cast<object>()));
                    _groups.Add(group);
                    Parameters.CollectionChanged += (s, e) => SyncGroup("Параметры", Parameters);
                }

                if (Nodes?.Count > 0)
                {
                    var group = new GroupNode("Детали", new ObservableCollection<object>(Nodes.Cast<object>()));
                    _groups.Add(group);
                    Nodes.CollectionChanged += (s, e) => SyncGroup("Детали", Nodes);
                }

                return _groups;
            }
        }


        private void SyncGroup(string groupName, IEnumerable<object> source)
        {
            var group = _groups.FirstOrDefault(g => g.Name == groupName);

            if (source.Any())
            {
                if (group != null)
                {
                    group.Children.Clear();
                    foreach (var item in source)
                        group.Children.Add(item);
                }
                else
                {
                    _groups.Add(new GroupNode(groupName, new ObservableCollection<object>(source)));
                }
            }
            else
            {
                // Удалить группу, если данных больше нет
                if (group != null)
                    _groups.Remove(group);
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            _isInitialized = true;
        }

    }
}

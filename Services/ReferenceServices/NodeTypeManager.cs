using System;
using System.Collections.ObjectModel;
using System.Linq;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Статический менеджер для работы со списком типов узлов.
    /// </summary>
    public static class NodeTypeManager
    {
        /// <summary>
        /// Список доступных типов узлов в виде строк.
        /// </summary>
        public static ObservableCollection<string> NodeTypes { get; private set; } = new ObservableCollection<string>();

        /// <summary>
        /// Информация о справочнике типов узлов.
        /// </summary>
        public static ReferenceInfo NodeTypeInfo { get; set; }

        /// <summary>
        /// Заполняет список NodeTypes значениями из ReferenceInfo.
        /// </summary>
        public static void SetNodeTypesList()
        {
            if (NodeTypeInfo == null)
            {
                NodeTypes.Clear();
                return;
            }

            var reference = NodeTypeInfo.CreateReference();
            reference.Objects.Reload();

            var newItems = reference.Objects.Select(obj => obj.ToString()).ToList();

            // Обновляем коллекцию без пересоздания
            UpdateObservableCollection(NodeTypes, newItems);
        }

        private static void UpdateObservableCollection(ObservableCollection<string> collection, System.Collections.Generic.List<string> newItems)
        {
            // Удаляем лишние элементы
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (!newItems.Contains(collection[i]))
                    collection.RemoveAt(i);
            }

            // Добавляем новые элементы
            foreach (var item in newItems)
            {
                if (!collection.Contains(item))
                    collection.Add(item);
            }
        }

        public static bool AddNodeType(string typeName)
        {
            try
            {
                if (NodeTypeInfo == null)
                {
                    NodeTypes.Clear();
                    return false;
                }

                var reference = NodeTypeInfo.CreateReference();

                var nameGuid = reference.ParameterGroup.Parameters.FindByName("Наименование")?.Guid;

                if (nameGuid == null)
                    return false;

                var newType = reference.CreateReferenceObject();
                newType[nameGuid.Value].Value = typeName;
                newType.EndChanges();

                SetNodeTypesList();

                return true;
            }
            catch (Exception ex)
            {
                // TODO: логировать ex
                return false;
            }
        }

        public static bool EditNodeType(string oldTypeName, string newTypeName)
        {
            try
            {
                if (NodeTypeInfo == null)
                {
                    NodeTypes.Clear();
                    return false;
                }

                var reference = NodeTypeInfo.CreateReference();
                reference.Objects.Reload();

                var editNodeType = reference.Objects.FirstOrDefault(type => type.ToString().Equals(oldTypeName, StringComparison.OrdinalIgnoreCase));
                if (editNodeType == null)
                    return false;

                var nameGuid = reference.ParameterGroup.Parameters.FindByName("Наименование")?.Guid;
                if (nameGuid == null)
                    return false;

                editNodeType.BeginChanges();
                editNodeType[nameGuid.Value].Value = newTypeName;
                editNodeType.EndChanges();

                SetNodeTypesList();

                return true;
            }
            catch (Exception ex)
            {
                // TODO: логировать ex
                return false;
            }
        }

        public static bool RemoveNodeType(string typeName)
        {
            try
            {
                if (NodeTypeInfo == null)
                {
                    NodeTypes.Clear();
                    return false;
                }

                var reference = NodeTypeInfo.CreateReference();
                reference.Objects.Reload();

                var removeNodeType = reference.Objects.FirstOrDefault(type => type.ToString().Equals(typeName, StringComparison.OrdinalIgnoreCase));
                if (removeNodeType == null)
                    return false;

                removeNodeType.Delete();

                SetNodeTypesList();

                return true;
            }
            catch (Exception ex)
            {
                // TODO: логировать ex
                return false;
            }
        }
    }
}

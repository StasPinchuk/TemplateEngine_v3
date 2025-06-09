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
            if (NodeTypeInfo != null)
            {
                var reference = NodeTypeInfo.CreateReference();

                // Перезагружаем объекты, чтобы актуализировать данные
                reference.Objects.Reload();

                // Заполняем список названиями объектов
                NodeTypes = new(reference.Objects.Select(obj => obj.ToString()));
            }
            else
            {
                // Если NodeTypeInfo не установлен, очищаем список
                NodeTypes.Clear();
            }
        }

        public static bool AddNodeType(string typeName)
        {
            try
            {
                if (NodeTypeInfo != null)
                {
                    var reference = NodeTypeInfo.CreateReference();

                    var nameGuid = reference.ParameterGroup.Parameters.FindByName("Наименование").Guid;

                    var newType = reference.CreateReferenceObject();
                    newType[nameGuid].Value = typeName;
                    newType.EndChanges();
                    NodeTypes.Add(typeName);

                    SetNodeTypesList();

                    return true;
                }
                else
                {
                    NodeTypes.Clear();
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool EditNodeType(string typeName)
        {
            try
            {
                if (NodeTypeInfo != null)
                {
                    var reference = NodeTypeInfo.CreateReference();

                    reference.Objects.Reload();

                    var editNodeType = reference.Objects.FirstOrDefault(type => type.ToString().Equals(typeName));

                    if (editNodeType != null)
                    {
                        var nameGuid = reference.ParameterGroup.Parameters.FindByName("Наименование").Guid;
                        editNodeType.BeginChanges();
                        editNodeType[nameGuid].Value = typeName;
                        editNodeType.EndChanges();

                        SetNodeTypesList();

                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    NodeTypes.Clear();
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool RemoveNodeType(string typeName)
        {
            try
            {
                if (NodeTypeInfo != null)
                {
                    var reference = NodeTypeInfo.CreateReference();

                    reference.Objects.Reload();

                    var removeNodeType = reference.Objects.FirstOrDefault(type => type.ToString().Equals(typeName));

                    if (removeNodeType != null)
                    {
                        removeNodeType.Delete();

                        SetNodeTypesList();

                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    NodeTypes.Clear();
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

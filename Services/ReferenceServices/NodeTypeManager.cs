using System.Collections.Generic;
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
        public static List<string> NodeTypes { get; private set; } = new List<string>();

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
                NodeTypes = reference.Objects.Select(obj => obj.ToString()).ToList();
            }
            else
            {
                // Если NodeTypeInfo не установлен, очищаем список
                NodeTypes.Clear();
            }
        }
    }
}

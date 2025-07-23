using System;
using System.Collections.Generic;

namespace TemplateEngine_v3.Models.LogModels
{
    /// <summary>
    /// Группа записей журнала, относящихся к одному объекту.
    /// </summary>
    public class LogObjectGroup
    {
        /// <summary>
        /// Имя или идентификатор объекта (например, шаблон, ТП и т.д.).
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Тип объекта (например, Template, TechProcess, Branch и т.д.).
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Пользователь, выполнивший действия над объектом.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Список записей журнала, связанных с данным объектом.
        /// </summary>
        public List<LogEntry> Entries { get; set; } = new();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LogObjectGroup"/>.
        /// </summary>
        public LogObjectGroup() { }

        /// <summary>
        /// Инициализирует группу журнала с указанным именем объекта, его типом и пользователем.
        /// </summary>
        /// <param name="objectName">Имя или ID объекта.</param>
        /// <param name="objectType">Тип объекта.</param>
        /// <param name="user">Пользователь, совершивший действия.</param>
        public LogObjectGroup(string objectName, string objectType, string user)
        {
            ObjectName = objectName;
            ObjectType = objectType;
            User = user;
        }
    }
}

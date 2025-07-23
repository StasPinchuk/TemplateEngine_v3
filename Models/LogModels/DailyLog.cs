using System;
using System.Collections.Generic;

namespace TemplateEngine_v3.Models.LogModels
{
    /// <summary>
    /// Представляет лог событий за один день.
    /// </summary>
    public class DailyLog
    {
        private DateTime _date;

        /// <summary>
        /// Дата лога. Хранится без времени.
        /// </summary>
        public DateTime Date
        {
            get => _date.Date;
            set => _date = value;
        }

        /// <summary>
        /// Группы объектов, зафиксированных в логе за день.
        /// </summary>
        public List<LogObjectGroup> ObjectGroups { get; set; } = new();

        /// <summary>
        /// Инициализирует пустой лог.
        /// </summary>
        public DailyLog() { }

        /// <summary>
        /// Инициализирует лог с заданной датой.
        /// </summary>
        /// <param name="date">Дата лога.</param>
        public DailyLog(DateTime date)
        {
            Date = date;
        }
    }
}

using System;

namespace TemplateEngine_v3.Models.LogModels
{
    /// <summary>
    /// Перечисление возможных типов действий, записываемых в лог.
    /// </summary>
    public enum LogActionType
    {
        /// <summary>
        /// Редактирование данных.
        /// </summary>
        Edit,

        /// <summary>
        /// Создание нового объекта.
        /// </summary>
        Create,

        /// <summary>
        /// Обновление существующего объекта.
        /// </summary>
        Update,

        /// <summary>
        /// Удаление объекта.
        /// </summary>
        Delete,

        /// <summary>
        /// Ошибка, произошедшая в процессе работы.
        /// </summary>
        Error
    }

    /// <summary>
    /// Представляет одну запись в логе действий.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Дата и время события.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Тип действия, связанного с записью.
        /// </summary>
        public LogActionType ActionType { get; set; }

        /// <summary>
        /// Сообщение или описание действия.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Инициализирует пустую запись лога.
        /// </summary>
        public LogEntry() { }

        /// <summary>
        /// Инициализирует запись лога с заданным типом действия и сообщением.
        /// </summary>
        /// <param name="actionType">Тип действия.</param>
        /// <param name="message">Сообщение лога.</param>
        public LogEntry(LogActionType actionType, string message)
        {
            Timestamp = DateTime.Now;
            ActionType = actionType;
            Message = message;
        }
    }
}

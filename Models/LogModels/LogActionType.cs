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
}

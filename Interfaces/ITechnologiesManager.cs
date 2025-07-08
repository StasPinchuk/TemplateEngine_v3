using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для управления технологиями и связанными операциями.
    /// </summary>
    public interface ITechnologiesManager
    {
        /// <summary>
        /// Событие, возникающее при изменении текущей технологии.
        /// </summary>
        event Action<Technologies> CurrentTechnologiesChanged;

        /// <summary>
        /// Помощник для работы с контекстным меню.
        /// </summary>
        ContextMenuHelper MenuHelper { get; set; }

        /// <summary>
        /// Текущая выбранная технология.
        /// </summary>
        Technologies CurrentTechnologies { get; set; }

        /// <summary>
        /// Коллекция всех веток (Branches).
        /// </summary>
        ObservableCollection<ReferenceModelInfo> Branches { get; }

        /// <summary>
        /// Получает коллекцию всех технологий.
        /// </summary>
        /// <returns>Коллекция объектов <see cref="ReferenceModelInfo"/> представляющих технологии.</returns>
        ObservableCollection<ReferenceModelInfo> GetAllTechnologies();

        /// <summary>
        /// Удаляет указанную технологию.
        /// </summary>
        /// <param name="removeTechnologies">Объект технологии, которую необходимо удалить.</param>
        /// <returns>Задача, результат которой указывает успешность операции удаления.</returns>
        Task<bool> RemoveTechnologies(ReferenceModelInfo removeTechnologies);

        /// <summary>
        /// Добавляет новую технологию.
        /// </summary>
        /// <param name="createTechnologies">Объект технологии для добавления.</param>
        /// <returns>Задача, результат которой указывает успешность операции добавления.</returns>
        Task<bool> AddTechnologies(Technologies createTechnologies);

        /// <summary>
        /// Редактирует существующую технологию.
        /// </summary>
        /// <param name="createTechnologies">Объект технологии с обновленными данными.</param>
        /// <returns>Задача, результат которой указывает успешность операции редактирования.</returns>
        Task<bool> EditTechnologies(Technologies createTechnologies);

        /// <summary>
        /// Клонирует выбранную технологию.
        /// </summary>
        /// <param name="selectedTechnologies">Объект технологии для клонирования.</param>
        /// <returns>Задача, результат которой указывает успешность операции клонирования.</returns>
        Task<bool> CloneTechnologies(ReferenceModelInfo selectedTechnologies);

        /// <summary>
        /// Получает менеджер имен операций, связанных с технологиями.
        /// </summary>
        /// <returns>Экземпляр <see cref="OperationNamesManager"/> для управления именами операций.</returns>
        OperationNamesManager GetOperationNamesManager();

        /// <summary>
        /// Получает контекстное меню, связанное с технологиями.
        /// </summary>
        /// <returns>Экземпляр <see cref="ContextMenu"/> для отображения контекстного меню.</returns>
        ContextMenu GetContextMenu();

        TechnologiesManager DeepCopy();
    }
}

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.ReferenceServices;
using TFlex.DOCs.Model.Classes;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для управления шаблонами, включая создание, редактирование, удаление и выбор шаблонов.
    /// </summary>
    public interface ITemplateManager
    {
        TableService TableService { get; set; }
        MaterialManager MaterialManager { get; set; }
        /// <summary>
        /// Помощник для работы с контекстным меню.
        /// </summary>
        ContextMenuHelper MenuHelper { get; set; }

        /// <summary>
        /// Получает коллекцию шаблонов в статусе "Черновики".
        /// </summary>
        /// <returns>Коллекция черновых шаблонов.</returns>
        ObservableCollection<ReferenceModelInfo> GetDraftTemplates();

        /// <summary>
        /// Получает коллекцию шаблонов в статусе "Готовые".
        /// </summary>
        /// <returns>Коллекция готовых шаблонов.</returns>
        ObservableCollection<ReferenceModelInfo> GetReadyTemplate();

        /// <summary>
        /// Получает коллекцию шаблонов, находящихся в корзине.
        /// </summary>
        /// <returns>Коллекция шаблонов в корзине.</returns>
        ObservableCollection<ReferenceModelInfo> GetTrashCanTemplates();

        /// <summary>
        /// Получает шаблон по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор шаблона.</param>
        /// <returns>Объект информации о шаблоне.</returns>
        ReferenceModelInfo GetTemplateById(Guid id);

        /// <summary>
        /// Асинхронно удаляет указанный шаблон.
        /// </summary>
        /// <param name="referenceModel">Шаблон для удаления.</param>
        /// <returns>Задача, возвращающая результат успешности операции.</returns>
        Task<bool> RemoveTemplateAsync(ReferenceModelInfo referenceModel);

        /// <summary>
        /// Асинхронно копирует выбранный шаблон.
        /// </summary>
        /// <param name="selectedTemplate">Шаблон для копирования.</param>
        /// <returns>Задача, возвращающая результат успешности операции.</returns>
        Task<bool> CopyTemplateAsync(ReferenceModelInfo selectedTemplate);

        /// <summary>
        /// Асинхронно редактирует существующий шаблон.
        /// </summary>
        /// <param name="editTemplate">Шаблон с обновлёнными данными.</param>
        /// <returns>Задача, возвращающая результат успешности операции.</returns>
        Task<bool> EditTemplateAsync(Template editTemplate);

        /// <summary>
        /// Асинхронно добавляет новый шаблон с привязкой к объекту класса.
        /// </summary>
        /// <param name="createTemplate">Создаваемый шаблон.</param>
        /// <param name="classObject">Объект класса для привязки.</param>
        /// <returns>Задача, возвращающая результат успешности операции.</returns>
        Task<bool> AddTemplateAsync(Template createTemplate, ClassObject classObject);

        /// <summary>
        /// Устанавливает текущий шаблон по информации о шаблоне.
        /// </summary>
        /// <param name="referenceModel">Объект информации о шаблоне.</param>
        Task<bool> SetTemplateAsync(ReferenceModelInfo referenceModel);

        /// <summary>
        /// Устанавливает текущий шаблон напрямую.
        /// </summary>
        /// <param name="template">Объект шаблона.</param>
        Task<bool> SetTemplateAsync(Template template);

        /// <summary>
        /// Асинхронно сохраняет шаблон с указанным типом.
        /// </summary>
        /// <param name="type">Тип шаблона для сохранения.</param>
        /// <returns>Задача, возвращающая результат успешности операции.</returns>
        Task<bool> SaveTemplate(string type);
        Task<bool> RestoreTemplateAsync(ReferenceModelInfo reference);

        /// <summary>
        /// Получает выбранный в данный момент шаблон.
        /// </summary>
        /// <returns>Текущий выбранный шаблон.</returns>
        Template GetSelectedTemplate();

        /// <summary>
        /// Очищает текущий выбранный шаблон.
        /// </summary>
        void ClearTemplate();
    }
}

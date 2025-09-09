using System;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс, описывающий файл с шаблоном.
    /// </summary>
    public interface ITemplatedFile
    {
        /// <summary>
        /// Уникальный идентификатор файла.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Дата создания файла.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Дата последнего изменения файла.
        /// </summary>
        public DateTime LastModifiedDate { get; set; }
    }
}

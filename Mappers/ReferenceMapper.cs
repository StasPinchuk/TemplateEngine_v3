using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;

namespace TemplateEngine_v3.Mappers
{
    /// <summary>
    /// Класс для преобразования объектов TFlex Reference в модели приложения.
    /// </summary>
    public static class ReferenceMapper
    {
        private static Guid _titleParameterGuid;
        private static Guid _createDateParameterGuid;
        private static Guid _lastEditParameterGuid;
        private static Guid _structObjectParameterGuid;
        private static Guid _lockedParameterGuid;

        /// <summary>
        /// Экземпляр Reference для получения параметров.
        /// </summary>
        public static Reference Reference;
        public static string UserFio;

        /// <summary>
        /// Инициализирует и кэширует GUID параметров для Reference.
        /// </summary>
        /// <param name="referenceInfo">Информация о Reference, используемая для создания Reference.</param>
        public static void SetParameters(ReferenceInfo referenceInfo)
        {
            if (Reference == null)
                Reference = referenceInfo.CreateReference();
            if (_titleParameterGuid == Guid.Empty)
                _titleParameterGuid = Reference.ParameterGroup.Parameters.FindByName("Наименование").Guid;
            if (_createDateParameterGuid == Guid.Empty)
                _createDateParameterGuid = Reference.ParameterGroup.Parameters.FindByName("Дата создания").Guid;
            if (_lastEditParameterGuid == Guid.Empty)
                _lastEditParameterGuid = Reference.ParameterGroup.Parameters.FindByName("Дата последнего изменения").Guid;
            if (_structObjectParameterGuid == Guid.Empty)
                _structObjectParameterGuid = Reference.ParameterGroup.Parameters.FindByName("Структура файла").Guid;
            if (_lockedParameterGuid == Guid.Empty)
                _lockedParameterGuid = Reference.ParameterGroup.Parameters.FindByName("Заблокирован").Guid;
        }

        /// <summary>
        /// Преобразует объект <see cref="ReferenceObject"/> из TFlex в <see cref="ReferenceModelInfo"/>.
        /// </summary>
        /// <param name="reference">Объект Reference из TFlex.</param>
        /// <returns>Модель <see cref="ReferenceModelInfo"/> или <c>null</c>, если входной объект равен <c>null</c>.</returns>
        public static ReferenceModelInfo FromTFlexReferenceObject(ReferenceObject reference)
        {
            try
            {
                if (reference == null)
                    return null;

                return new ReferenceModelInfo
                {
                    Id = reference.Guid,
                    Name = reference[_titleParameterGuid].ToString(),
                    Type = reference.Class,
                    LastEditDate = Convert.ToDateTime(reference[_lastEditParameterGuid].ToString()),
                    CreateDate = Convert.ToDateTime(reference[_createDateParameterGuid].ToString()),
                    ObjectStruct = reference[_structObjectParameterGuid].ToString(),
                    IsLocked = !string.IsNullOrEmpty(reference[_lockedParameterGuid].ToString()) ? reference[_lockedParameterGuid].ToString().Contains(UserFio) : false
                };

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка конвертации: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Преобразует список объектов <see cref="ReferenceObject"/> из TFlex в коллекцию <see cref="ObservableCollection{ReferenceModelInfo}"/>.
        /// </summary>
        /// <param name="referenceObjects">Список объектов Reference из TFlex.</param>
        /// <returns>Коллекция моделей <see cref="ReferenceModelInfo"/> или <c>null</c>, если входной список равен <c>null</c>.</returns>
        public static ObservableCollection<ReferenceModelInfo> FromTFlexReferenceObjectList(List<ReferenceObject> referenceObjects)
        {
            if (referenceObjects == null) return null;

            return new ObservableCollection<ReferenceModelInfo>(
                referenceObjects
                    .Select(reference => FromTFlexReferenceObject(reference))
                    .Where(model => model != null)
            );
        }
    }
}

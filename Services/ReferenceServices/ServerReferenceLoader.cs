using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TemplateEngine_v3.Mappers;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Класс для загрузки материалов и справочных данных с сервера.
    /// </summary>
    public class ServerReferenceLoader
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ServerReferenceLoader"/>.
        /// </summary>
        public ServerReferenceLoader()
        {
        }

        /// <summary>
        /// Загружает список материалов из справочника.
        /// </summary>
        /// <param name="materialsInfo">Информация о справочнике материалов.</param>
        /// <returns>Список строк с названиями материалов.</returns>
        public List<string> LoadMaterials(ReferenceInfo materialsInfo)
        {
            var materialReference = materialsInfo.CreateReference();
            materialReference.Objects.Reload();
            List<string> result = materialReference.Objects.Select(material => material.ToString()).ToList();
            materialReference = null;
            return result;
        }

        /// <summary>
        /// Загружает объекты справочника и преобразует их в коллекцию моделей <see cref="ReferenceModelInfo"/>.
        /// </summary>
        /// <param name="referenceInfo">Информация о справочнике.</param>
        /// <returns>Коллекция моделей справочных объектов.</returns>
        public ObservableCollection<ReferenceModelInfo> LoadReference(ReferenceInfo referenceInfo)
        {   
            ReferenceMapper.SetParameters(referenceInfo);
            ReferenceMapper.Reference.Objects.Reload();
            List<ReferenceObject> referenceObjects = ReferenceMapper.Reference.Objects
                .Where(refs => !refs.ToString().Equals("LogList"))
                .ToList();
            return ReferenceMapper.FromTFlexReferenceObjectList(referenceObjects);
        }
    }
}

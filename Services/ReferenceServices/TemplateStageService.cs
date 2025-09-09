using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.Structure;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Сервис для работы с этапами шаблона, основанный на справочнике TFlex.
    /// </summary>
    public class TemplateStageService : BaseNotifyPropertyChanged
    {
        private readonly Reference _reference;
        private readonly ParameterInfo _nameParameter;
        private readonly ParameterInfo _structParameter;

        /// <summary>
        /// Коллекция этапов шаблона.
        /// </summary>
        public ObservableCollection<StageModel> StageList { get; set; } = [];

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TemplateStageService"/> на основе <see cref="ReferenceInfo"/>.
        /// </summary>
        /// <param name="referenceInfo">Информация о справочнике.</param>
        public TemplateStageService(ReferenceInfo referenceInfo)
        {
            _reference = referenceInfo.CreateReference();

            _nameParameter = _reference.ParameterGroup.Parameters.FindByName("Наименование");
            _structParameter = _reference.ParameterGroup.Parameters.FindByName("Структура файла");
        }

        /// <summary>
        /// Загружает этапы из справочника в коллекцию <see cref="StageList"/>.
        /// </summary>
        public void SetStageList()
        {
            StageList.Clear();
            _reference.Objects.Reload();

            foreach (var stage in _reference.Objects)
            {
                var stageModel = new JsonSerializer().Deserialize<StageModel>(stage[_structParameter.Guid].Value.ToString());
                StageList.Add(stageModel);
            }
        }

        /// <summary>
        /// Добавляет новый этап в справочник и коллекцию.
        /// </summary>
        /// <param name="stage">Добавляемый этап.</param>
        public async void AddStage(StageModel stage)
        {
            var newStage = _reference.CreateReferenceObject();

            stage.ID = newStage.Guid;
            newStage[_nameParameter].Value = stage.StageName;
            newStage[_structParameter].Value = new JsonSerializer().Serialize(stage);

            await newStage.EndChangesAsync();

            StageList.Add(stage);
        }

        /// <summary>
        /// Редактирует существующий этап в справочнике.
        /// </summary>
        /// <param name="stage">Редактируемый этап.</param>
        public async void EditStage(StageModel stage)
        {
            _reference.Objects.Reload();

            var editStage = await _reference.FindAsync(stage.ID);

            await editStage.BeginChangesAsync();

            editStage[_nameParameter].Value = stage.StageName;
            editStage[_structParameter].Value = new JsonSerializer().Serialize(stage);

            await editStage.EndChangesAsync();

            SetStageList();
        }

        /// <summary>
        /// Удаляет этап из справочника и коллекции.
        /// </summary>
        /// <param name="stage">Удаляемый этап.</param>
        public async void RemoveStage(StageModel stage)
        {
            _reference.Objects.Reload();

            var removeStage = await _reference.FindAsync(stage.ID);

            if (removeStage != null)
            {
                await removeStage.DeleteAsync();
                StageList.Remove(stage);
            }
        }

        /// <summary>
        /// Возвращает список описаний значений перечисления <see cref="StatusType"/>.
        /// </summary>
        /// <returns>Список строковых описаний.</returns>
        public List<string> GetStatusTypeList()
        {
            return Enum.GetValues(typeof(StatusType))
                    .Cast<StatusType>()
                    .Select(e => GetEnumDescription(e))
                    .ToList();
        }

        /// <summary>
        /// Возвращает описание значения перечисления, указанного через <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <param name="value">Значение перечисления.</param>
        /// <returns>Описание или имя значения перечисления.</returns>
        public string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }

        /// <summary>
        /// Возвращает значение перечисления по его описанию <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <typeparam name="TEnum">Тип перечисления.</typeparam>
        /// <param name="description">Описание значения.</param>
        /// <returns>Значение перечисления или null, если не найдено.</returns>
        public TEnum? GetEnumValueByDescription<TEnum>(string description) where TEnum : struct, Enum
        {
            foreach (var field in typeof(TEnum).GetFields())
            {
                var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null && attr.Description == description)
                {
                    return (TEnum)field.GetValue(null);
                }
            }

            return null;
        }
    }
}

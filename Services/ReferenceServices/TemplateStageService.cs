using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.Structure;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    public class TemplateStageService : BaseNotifyPropertyChanged
    {
        private readonly Reference _reference;
        private readonly ParameterInfo _nameParameter;
        private readonly ParameterInfo _structParameter;

        public ObservableCollection<StageModel> StageList { get; set; } = [];

        public TemplateStageService(ReferenceInfo referenceInfo)
        {
            _reference = referenceInfo.CreateReference();

            _nameParameter = _reference.ParameterGroup.Parameters.FindByName("Наименование");
            _structParameter = _reference.ParameterGroup.Parameters.FindByName("Структура файла");
        }

        public void SetStageList()
        {
            StageList.Clear();
            _reference.Objects.Reload();

            foreach(var stage in _reference.Objects)
            {
                var stageModel = new JsonSerializer().Deserialize<StageModel>(stage[_structParameter.Guid].Value.ToString());
                StageList.Add(stageModel);
            }
        }

        public async void AddStage(StageModel stage)
        {
            var newStage = _reference.CreateReferenceObject();

            stage.ID = newStage.Guid;
            newStage[_nameParameter].Value = stage.StageName;
            newStage[_structParameter].Value = new JsonSerializer().Serialize(stage);

            await newStage.EndChangesAsync();

            StageList.Add(stage);
        }

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

        public List<string> GetStatusTypeList()
        {
            return Enum.GetValues(typeof(StatusType))
                    .Cast<StatusType>()
                    .Select(e => GetEnumDescription(e))
                    .ToList();
        }

        public string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }

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

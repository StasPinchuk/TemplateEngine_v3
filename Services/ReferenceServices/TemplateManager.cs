using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.Classes;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.Structure;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Менеджер шаблонов изделий для генератора.
    /// </summary>
    public class TemplateManager : BaseNotifyPropertyChanged
    {
        private readonly ServerReferenceLoader _referenceLoader;
        private readonly ReferenceInfo _templateInfo;
        private readonly Reference _reference;
        private readonly ParameterInfo _nameParameter;
        private readonly ParameterInfo _objectStringParameter;
        private readonly ClassObject _readyTemplateType;
        private readonly ClassObject _draftTemplateType;
        private readonly ClassObject _trashCanType;
        public MaterialManager MaterialManager { get; set; }
        public TableService TableService { get; set; }

        /// <summary>
        /// Помощник для контекстного меню.
        /// </summary>
        public ContextMenuHelper MenuHelper { get; set; }

        /// <summary>
        /// Выбранный шаблон.
        /// </summary>
        public Template SelectedTemplate { get; private set; }

        private ObservableCollection<ReferenceModelInfo>? _cachedTemplates;

        /// <summary>
        /// Конструктор менеджера шаблонов.
        /// </summary>
        /// <param name="referenceLoader">Загрузчик справочных данных.</param>
        /// <param name="templateInfo">Информация о справочнике шаблонов.</param>
        /// <param name="materialManager">Менеджер материалов.</param>
        public TemplateManager(ServerReferenceLoader referenceLoader, ReferenceInfo templateInfo, MaterialManager materialManager, TableService tableService)
        {
            _referenceLoader = referenceLoader;
            _templateInfo = templateInfo;
            MaterialManager = materialManager;
            _reference = _templateInfo.CreateReference();

            _nameParameter = _reference.ParameterGroup.Parameters.FindByName("Наименование");
            _objectStringParameter = _reference.ParameterGroup.Parameters.FindByName("Структура файла");
            _readyTemplateType = _templateInfo.Classes.Find("Шаблоны");
            _draftTemplateType = _templateInfo.Classes.Find("Незавершённые изделия для генератора");
            _trashCanType = _templateInfo.Classes.Find("Корзина");
            TableService = tableService;
        }

        /// <summary>
        /// Обеспечивает загрузку шаблонов, если они еще не загружены.
        /// </summary>
        private void EnsureTemplatesLoaded()
        {
            _cachedTemplates = _referenceLoader.LoadReference(_templateInfo);
        }

        /// <summary>
        /// Получить коллекцию готовых шаблонов.
        /// </summary>
        /// <returns>Коллекция готовых шаблонов, отсортированных по имени в обратном порядке.</returns>
        public ObservableCollection<ReferenceModelInfo> GetReadyTemplate()
        {
            EnsureTemplatesLoaded();
            return new ObservableCollection<ReferenceModelInfo>(_cachedTemplates
                .Where(template => (template.Type.Equals(_readyTemplateType)
                                || template.Type.Equals(_draftTemplateType))
                                && template.Name != "LogsObject (Не удалять)")
                .OrderBy(template => template.Name)
                .Reverse());
        }

        /// <summary>
        /// Получить коллекцию шаблонов из корзины.
        /// </summary>
        /// <returns>Коллекция шаблонов из корзины, отсортированных по имени.</returns>
        public ObservableCollection<ReferenceModelInfo> GetTrashCanTemplates()
        {
            EnsureTemplatesLoaded();
            return new ObservableCollection<ReferenceModelInfo>(_cachedTemplates
                .Where(template => template.Type.Equals(_trashCanType))
                .OrderBy(template => template.Name));
        }

        /// <summary>
        /// Найти шаблон по уникальному идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор шаблона.</param>
        /// <returns>Найденный шаблон или null.</returns>
        public ReferenceModelInfo? GetTemplateById(Guid id)
        {
            EnsureTemplatesLoaded();
            return _cachedTemplates.FirstOrDefault(template => template.Id.Equals(id));
        }

        /// <summary>
        /// Создает копию выбранного шаблона асинхронно.
        /// </summary>
        /// <param name="selectedTemplate">Шаблон для копирования.</param>
        /// <returns>True, если копирование прошло успешно, иначе false.</returns>
        public Task<bool> CopyTemplateAsync(ReferenceModelInfo selectedTemplate)
        {
            if (_nameParameter == null || _objectStringParameter == null)
                return Task.FromResult(false);

            return DoAsync();

            async Task<bool> DoAsync()
            {
                try
                {
                    await _reference.Objects.ReloadAsync();
                    var newTemplate = _reference.CreateReferenceObject(selectedTemplate.Type);
                    var newName = $"{selectedTemplate.Name} - копия";

                    newTemplate[_nameParameter.Guid].Value = newName;

                    if (selectedTemplate.ObjectStruct != null)
                    {
                        newTemplate[_objectStringParameter.Guid].Value =
                            selectedTemplate.ObjectStruct
                                .Replace(selectedTemplate.Name, newName)
                                .Replace(selectedTemplate.Id.ToString(), newTemplate.Guid.ToString());
                    }

                    await newTemplate.EndChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[TemplateManager] Ошибка при копировании: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Асинхронно удаляет шаблон. Если шаблон является готовым или незавершённым, перемещает его в корзину.
        /// </summary>
        /// <param name="referenceModelInfo">Шаблон для удаления.</param>
        /// <returns>True при успешном удалении, иначе false.</returns>
        public async Task<bool> RemoveTemplateAsync(ReferenceModelInfo referenceModelInfo, TemplateStageService templateStageService)
        {
            try
            {
                await _reference.Objects.ReloadAsync();
                var templateToRemove = await _reference.FindAsync(referenceModelInfo.Id);

                if (templateToRemove == null)
                    return false;

                if (templateToRemove.Class == _readyTemplateType || templateToRemove.Class == _draftTemplateType)
                {
                    await SetTemplateAsync(referenceModelInfo, true);
                    SelectedTemplate.Stage = templateStageService.StageList.FirstOrDefault(stage => stage.StageType == StatusType.Archive).ID;
                    bool trashCopy = await AddTemplateAsync(SelectedTemplate, _trashCanType);

                    await templateToRemove.DeleteAsync();

                    referenceModelInfo = null;
                }
                else
                {
                    await templateToRemove.DeleteAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TemplateManager] Ошибка при удалении: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Асинхронно редактирует существующий шаблон.
        /// </summary>
        /// <param name="editTemplate">Шаблон с изменениями.</param>
        /// <returns>True при успешном редактировании, иначе false.</returns>
        public async Task<bool> EditTemplateAsync(Template editTemplate, string type)
        {
            try
            {
                await _reference.Objects.ReloadAsync();
                var editReference = await _reference.FindAsync(editTemplate.Id);

                ClassObject templateType = type.Equals("Final") ? _readyTemplateType : type.Equals("Archive") ? _trashCanType : _draftTemplateType;
                if (type.Equals("Archive") || editReference.Class != templateType)
                {
                    bool isSave = await AddTemplateAsync(editTemplate, templateType);
                    if (isSave)
                        await editReference.DeleteAsync();
                    return isSave;
                }

                if (editReference == null || _nameParameter == null || _objectStringParameter == null)
                    return false;

                await editReference.BeginChangesAsync();

                string jsonString = new JsonSerializer().Serialize(editTemplate);

                editReference[_nameParameter.Guid].Value = editTemplate.Name;
                editReference[_objectStringParameter.Guid].Value = jsonString;

                await editReference.EndChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                return false;
            }
        }

        /// <summary>
        /// Асинхронно добавляет новый шаблон.
        /// </summary>
        /// <param name="createTemplate">Создаваемый шаблон.</param>
        /// <param name="classObject">Класс объекта шаблона.</param>
        /// <returns>True при успешном добавлении, иначе false.</returns>
        public Task<bool> AddTemplateAsync(Template createTemplate, ClassObject classObject)
        {
            if (_nameParameter == null || _objectStringParameter == null)
                return Task.FromResult(false);

            return DoAsync();

            async Task<bool> DoAsync()
            {
                try
                {
                    await _reference.Objects.ReloadAsync();
                    var newTemplate = _reference.CreateReferenceObject(classObject);

                    createTemplate.Id = newTemplate.Guid;
                    newTemplate[_nameParameter.Guid].Value = createTemplate.Name;
                    newTemplate[_objectStringParameter.Guid].Value = new JsonSerializer().Serialize(createTemplate);

                    newTemplate.EndChanges();

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка");
                    return false;
                }
            }
        }

        /// <summary>
        /// Устанавливает текущий выбранный шаблон из объекта ReferenceModelInfo.
        /// </summary>
        /// <param name="referenceModel">Объект модели справочника шаблонов.</param>
        public async Task<bool> SetTemplateAsync(ReferenceModelInfo referenceModel, bool isRemove = false)
        {
            await _reference.Objects.ReloadAsync();
            var findTemplate = await _reference.FindAsync(referenceModel.Id);

            if (findTemplate == null)
                return false;
            Template template = new JsonSerializer().Deserialize<Template>(referenceModel.ObjectStruct);

            if (!isRemove)
                NavigationService.RenameSelectedTab(template.Name);
            template.ProductMarkingAttributes = SetMarkingAttributes(template.ExampleMarkings);

            MenuHelper = new ContextMenuHelper(template, MaterialManager);
            await MenuHelper.CreateContextMenuAsync();
            SelectedTemplate = template;
            return true;
        }

        /// <summary>
        /// Устанавливает текущий выбранный шаблон из объекта Template.
        /// </summary>
        /// <param name="template">Объект шаблона.</param>
        public async Task<bool> SetTemplateAsync(Template template)
        {
            if (template.Name.Equals("Новый шаблон"))
            {
                SelectedTemplate = template;
                MenuHelper = new ContextMenuHelper(SelectedTemplate, MaterialManager);
                await MenuHelper.CreateContextMenuAsync();
                return true;
            }
            else
            {
                await _reference.Objects.ReloadAsync();
                var findTemplate = await _reference.FindAsync(template.Id);

                if (findTemplate != null)
                {
                    template.ProductMarkingAttributes = SetMarkingAttributes(template.ExampleMarkings);
                }

                MenuHelper = new ContextMenuHelper(template, MaterialManager);
                await MenuHelper.CreateContextMenuAsync();
                SelectedTemplate = template;
                return true;
            }

        }

        /// <summary>
        /// Получает текущий выбранный шаблон.
        /// </summary>
        /// <returns>Выбранный шаблон.</returns>
        public Template GetSelectedTemplate()
        {
            return SelectedTemplate;
        }

        /// <summary>
        /// Очищает текущий выбранный шаблон.
        /// </summary>
        public async void ClearTemplate()
        {
            if (SelectedTemplate == null)
                return;
            _reference.Objects.Reload();
            var findTemplate = await _reference.FindAsync(SelectedTemplate.Id);
            if (findTemplate != null && findTemplate.CanUnlock)
            {
                findTemplate.Unlock();
            }
            SelectedTemplate = null;
        }

        /// <summary>
        /// Сохраняет выбранный шаблон, определяя тип (draft или ready).
        /// </summary>
        /// <param name="type">Тип шаблона ("draft" или "ready").</param>
        /// <returns>True при успешном сохранении, иначе false.</returns>
        public async Task<bool> SaveTemplate(string type, Template template = null)
        {
            try
            {

                if (template == null)
                    template = SelectedTemplate;
                await _reference.Objects.ReloadAsync();
                var findTemplate = await _reference.FindAsync(template.Id);
                if (findTemplate != null && findTemplate.Changing)
                    await findTemplate.EndChangesAsync();
                var currentList = GetReadyTemplate();

                if (currentList.Any(temp => temp.Id.Equals(template.Id)))
                {
                    bool isEdit = await EditTemplateAsync(template, type);
                    if (isEdit)
                        await LogManager.SaveLog();
                    return isEdit;
                }
                else
                {
                    ClassObject templateType = type.Equals("Final") ? _readyTemplateType : type.Equals("Archive") ? _trashCanType : _draftTemplateType;
                    bool isSave = await AddTemplateAsync(template, templateType);
                    if (isSave)
                        await LogManager.SaveLog();
                    return isSave;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                return false;
            }
        }

        /// <summary>
        /// Восстанавливает шаблон по указанной ссылке <see cref="ReferenceModelInfo"/>.
        /// </summary>
        /// <param name="reference">
        /// Модель ссылки на шаблон, который требуется восстановить.
        /// </param>
        /// <returns>
        /// <c>true</c>, если восстановление прошло успешно;  
        /// <c>false</c>, если произошла ошибка или операция была прервана.
        /// </returns>
        /// <remarks>
        /// Алгоритм работы:
        /// <list type="number">
        /// <item><description>Перезагружает коллекцию объектов из источника данных.</description></item>
        /// <item><description>Устанавливает текущий шаблон по переданной ссылке.</description></item>
        /// <item><description>Находит и удаляет шаблон с тем же <c>Id</c>, что и выбранный.</description></item>
        /// <item><description>Добавляет шаблон заново с использованием готового типа <c>_readyTemplateType</c>.</description></item>
        /// </list>
        /// При возникновении исключения выводит сообщение об ошибке в <see cref="MessageBox"/> и возвращает <c>false</c>.
        /// </remarks>
        /// <exception cref="Exception">
        /// Может выбрасываться при ошибках загрузки, поиска, удаления или добавления шаблона.
        public async Task<bool> RestoreTemplateAsync(ReferenceModelInfo reference)
        {
            try
            {
                await _reference.Objects.ReloadAsync();
                await SetTemplateAsync(reference);
                var findTemplate = await _reference.FindAsync(SelectedTemplate.Id);
                await findTemplate.DeleteAsync();
                bool isRestore = await AddTemplateAsync(SelectedTemplate, _readyTemplateType);
                return isRestore;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                return false;
            }
        }

        /// <summary>
        /// Создает список уникальных слов из коллекции примерных маркировок.
        /// </summary>
        /// <param name="exampleMarkings">Коллекция строк с примерными маркировками.</param>
        /// <returns>Список уникальных слов.</returns>
        private List<string> SetMarkingAttributes(ObservableCollection<string> exampleMarkings)
        {
            List<string> markings = new List<string>();

            foreach (var exampleMarking in exampleMarkings)
            {
                var splitExample = exampleMarking.Split(new string[] { " ", "-", "*", "_" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in splitExample)
                {
                    if (!markings.Contains(line))
                    {
                        markings.Add(line);
                    }
                }
            }

            return markings;
        }

        /// <summary>
        /// Создаёт глубокую копию текущего экземпляра <see cref="TemplateManager"/>.
        /// </summary>
        /// <remarks>
        /// Клонируются:
        /// <list type="bullet">
        /// <item><description>Ссылки на <c>_referenceLoader</c>, <c>_templateInfo</c>, <c>MaterialManager</c>, <c>TableService</c>.</description></item>
        /// <item><description><see cref="MenuHelper"/> — копируется по ссылке.</description></item>
        /// <item><description><see cref="SelectedTemplate"/> — сериализуется и десериализуется для создания отдельного экземпляра.</description></item>
        /// <item><description>Кэш <c>_cachedTemplates</c> — при наличии создаётся новая коллекция с копиями <see cref="ReferenceModelInfo"/>.</description></item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// Новый экземпляр <see cref="TemplateManager"/>, содержащий копии данных текущего объекта.
        public TemplateManager Clone()
        {
            var clone = new TemplateManager(_referenceLoader, _templateInfo, MaterialManager, TableService)
            {
                MenuHelper = this.MenuHelper,
                SelectedTemplate = this.SelectedTemplate != null
                    ? new JsonSerializer().Deserialize<Template>(
                        new JsonSerializer().Serialize(this.SelectedTemplate))
                    : null
            };

            if (_cachedTemplates != null)
            {
                clone._cachedTemplates = new ObservableCollection<ReferenceModelInfo>(
                    _cachedTemplates.Select(template =>
                        new ReferenceModelInfo
                        {
                            Id = template.Id,
                            Name = template.Name,
                            ObjectStruct = template.ObjectStruct,
                            Type = template.Type
                        }));
            }

            return clone;
        }

    }
}

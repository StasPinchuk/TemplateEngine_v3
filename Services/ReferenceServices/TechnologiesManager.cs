using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using TemplateEngine_v3.Helpers;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.Classes;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.Structure;

namespace TemplateEngine_v3.Services.ReferenceServices
{
    /// <summary>
    /// Менеджер для работы с технологиями (ТП).
    /// </summary>
    public class TechnologiesManager : ITechnologiesManager
    {
        /// <summary>
        /// Событие, вызываемое при изменении текущей технологии.
        /// </summary>
        public event Action<Technologies> CurrentTechnologiesChanged;

        private readonly OperationNamesManager _operationNamesManager;
        private readonly ServerReferenceLoader _referenceLoader;
        private readonly ReferenceInfo _technologiesInfo;
        private readonly Reference _reference;
        private readonly ParameterInfo _nameParameter;
        private readonly ParameterInfo _objectStringParameter;
        private readonly ClassObject _technologiesClass;

        private ContextMenuHelper _contextMenuHelper;

        /// <summary>
        /// Помощник для контекстного меню.
        /// </summary>
        public ContextMenuHelper MenuHelper
        {
            get => _contextMenuHelper;
            set => _contextMenuHelper = value;
        }

        private Technologies _currentTechnologies = new();

        /// <summary>
        /// Текущие выбранные технологии.
        /// При изменении вызывает событие <see cref="CurrentTechnologiesChanged"/>.
        /// </summary>
        public Technologies CurrentTechnologies
        {
            get => _currentTechnologies;
            set
            {
                if (_currentTechnologies != value)
                {
                    _currentTechnologies = value;
                    CurrentTechnologiesChanged?.Invoke(_currentTechnologies);
                }
            }
        }

        private readonly ObservableCollection<ReferenceModelInfo> _branches;

        /// <summary>
        /// Коллекция всех филиалов.
        /// </summary>
        public ObservableCollection<ReferenceModelInfo> Branches => _branches;

        /// <summary>
        /// Конструктор менеджера технологий.
        /// </summary>
        /// <param name="referenceLoader">Загрузчик справочных данных.</param>
        /// <param name="technologiesInfo">Информация о справочнике технологий.</param>
        /// <param name="branchManager">Менеджер филиалов.</param>
        /// <param name="operationNamesManager">Менеджер названий операций.</param>
        public TechnologiesManager(ServerReferenceLoader referenceLoader, ReferenceInfo technologiesInfo, IBranchManager branchManager, OperationNamesManager operationNamesManager)
        {
            _referenceLoader = referenceLoader;
            _technologiesInfo = technologiesInfo;
            _operationNamesManager = operationNamesManager;
            _reference = _technologiesInfo.CreateReference();

            _technologiesClass = _technologiesInfo.Classes.Find("ТП");
            _nameParameter = _reference.ParameterGroup.Parameters.FindByName("Наименование");
            _objectStringParameter = _reference.ParameterGroup.Parameters.FindByName("Структура файла");
            _branches = branchManager.GetAllBranches();
        }

        /// <summary>
        /// Получить копию контекстного меню.
        /// </summary>
        /// <returns>Контекстное меню или null, если помощник не установлен.</returns>
        public ContextMenu GetContextMenu()
        {
            return _contextMenuHelper?.GetContextMenu();
        }

        /// <summary>
        /// Получить менеджер названий операций.
        /// </summary>
        /// <returns>Менеджер названий операций.</returns>
        public OperationNamesManager GetOperationNamesManager()
        {
            return _operationNamesManager;
        }

        /// <summary>
        /// Асинхронно добавляет новую технологию в справочник.
        /// </summary>
        /// <param name="createTechnologies">Создаваемые технологии.</param>
        /// <returns>True, если успешно добавлено; иначе false.</returns>
        public async Task<bool> AddTechnologies(Technologies createTechnologies)
        {
            try
            {
                var newTechnologies = _reference.CreateReferenceObject();

                if (_nameParameter == null || _objectStringParameter == null)
                    return false;

                createTechnologies.Id = newTechnologies.Guid;
                newTechnologies[_nameParameter.Guid].Value = createTechnologies.Name;
                newTechnologies[_objectStringParameter.Guid].Value = new JsonSerializer().Serialize(createTechnologies);

                await newTechnologies.EndChangesAsync();

                newTechnologies = null;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Асинхронно редактирует существующую технологию.
        /// </summary>
        /// <param name="editTechnologies">Технологии для редактирования.</param>
        /// <returns>True, если успешно отредактировано; иначе false.</returns>
        public async Task<bool> EditTechnologies(Technologies editTechnologies)
        {
            try
            {
                var reference = await _technologiesInfo.CreateReferenceAsync();
                await reference.Objects.ReloadAsync();

                var selectedTechnologies = reference.Find(editTechnologies.Id);
                if (selectedTechnologies != null)
                {
                    await selectedTechnologies.BeginChangesAsync();

                    selectedTechnologies[_nameParameter.Guid].Value = editTechnologies.Name;
                    selectedTechnologies[_objectStringParameter.Guid].Value = new JsonSerializer().Serialize(editTechnologies);

                    await selectedTechnologies.EndChangesAsync();
                }

                selectedTechnologies = null;
                reference = null;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Асинхронно клонирует существующую технологию, создавая копию с новым именем.
        /// </summary>
        /// <param name="selectedTechnologies">Клонируемая технология.</param>
        /// <returns>True, если успешно клонирована; иначе false.</returns>
        public async Task<bool> CloneTechnologies(ReferenceModelInfo selectedTechnologies)
        {
            try
            {
                if (_nameParameter == null || _objectStringParameter == null)
                    return false;

                var newTechnologies = _reference.CreateReferenceObject(selectedTechnologies.Type);
                var newName = $"{selectedTechnologies.Name} - копия";

                newTechnologies[_nameParameter.Guid].Value = newName;

                if (selectedTechnologies.ObjectStruct != null)
                {
                    newTechnologies[_objectStringParameter.Guid].Value =
                        selectedTechnologies.ObjectStruct.Replace(selectedTechnologies.Name, newName)
                        .Replace(selectedTechnologies.Id.ToString(), newTechnologies.Guid.ToString());
                }

                await newTechnologies.EndChangesAsync();

                newTechnologies = null;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TemplateManager] Ошибка при копировании: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получить все технологии из справочника.
        /// </summary>
        /// <returns>Коллекция технологий.</returns>
        public ObservableCollection<ReferenceModelInfo> GetAllTechnologies()
        {
            return new(_referenceLoader.LoadReference(_technologiesInfo)
                .Where(branch => branch.Type.Equals(_technologiesClass))
                );
        }

        /// <summary>
        /// Асинхронно удаляет технологию из справочника.
        /// </summary>
        /// <param name="removeTechnologies">Технология для удаления.</param>
        /// <returns>True, если успешно удалена; иначе false.</returns>
        public async Task<bool> RemoveTechnologies(ReferenceModelInfo removeTechnologies)
        {
            try
            {
                var technologiesToRemove = await _reference.FindAsync(removeTechnologies.Id);

                if (technologiesToRemove == null)
                    return false;

                await technologiesToRemove.DeleteAsync();

                removeTechnologies = null;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TemplateManager] Ошибка при удалении: {ex.Message}");
                return false;
            }
        }
    }
}

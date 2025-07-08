using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
    /// Менеджер для работы с филиалами (Branches) через объекты Reference.
    /// Предоставляет методы для добавления, редактирования, клонирования, получения списка и удаления филиалов.
    /// </summary>
    public class BranchManager : IBranchManager
    {
        private readonly ServerReferenceLoader _referenceLoader;
        private readonly ReferenceInfo _branchInfo;
        private readonly Reference _reference;
        private readonly ParameterInfo _nameParameter;
        private readonly ParameterInfo _objectStringParameter;
        private readonly ClassObject _branchClass;

        /// <summary>
        /// Инициализация менеджера: устанавливает загрузчик Reference, 
        /// получает класс филиалов и ключевые параметры.
        /// Выбрасывает исключение, если нужные параметры или класс не найдены.
        /// </summary>
        /// <param name="referenceLoader">Загрузчик Reference объектов.</param>
        /// <param name="branchInfo">Информация о Reference, описывающем филиалы.</param>
        public BranchManager(ServerReferenceLoader referenceLoader, ReferenceInfo branchInfo)
        {
            _referenceLoader = referenceLoader;
            _branchInfo = branchInfo;
            _reference = branchInfo.CreateReference();

            _branchClass = _branchInfo.Classes.Find("Филиалы");
            _nameParameter = _reference.ParameterGroup.Parameters.FindByName("Наименование");
            _objectStringParameter = _reference.ParameterGroup.Parameters.FindByName("Структура файла");

            if (_nameParameter == null || _objectStringParameter == null || _branchClass == null)
                throw new InvalidOperationException("Не найдены необходимые параметры или класс филиалов в Reference");
        }

        /// <summary>
        /// Асинхронно добавляет новый филиал.
        /// Создаёт ReferenceObject, заполняет параметры и сохраняет изменения.
        /// </summary>
        /// <param name="createBranch">Модель филиала для добавления.</param>
        /// <returns>True — если добавление прошло успешно, иначе false.</returns>
        public Task<bool> AddBranch(Branch createBranch)
        {
            if (_nameParameter == null || _objectStringParameter == null)
                return Task.FromResult(false);

            return AddBranchAsync(createBranch);

            async Task<bool> AddBranchAsync(Branch createBranch)
            {
                try
                {
                    var newBranch = _reference.CreateReferenceObject(_branchClass);

                    // Присваиваем Id для модели филиала
                    createBranch.Id = newBranch.Guid;

                    // Заполняем параметры ReferenceObject
                    newBranch[_nameParameter.Guid].Value = createBranch.Name;
                    newBranch[_objectStringParameter.Guid].Value = new JsonSerializer().Serialize(createBranch);

                    await newBranch.EndChangesAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при добавлении филиала: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Асинхронно редактирует существующий филиал.
        /// Обновляет параметры ReferenceObject по Id.
        /// </summary>
        /// <param name="editBranchObj">Объект филиала с обновлёнными данными.</param>
        /// <returns>True — если редактирование прошло успешно, иначе false.</returns>
        public Task<bool> EditBranch(Branch editBranchObj)
        {
            if (_nameParameter == null || _objectStringParameter == null)
                return Task.FromResult(false);

            return EditBranchAsync(editBranchObj);

            async Task<bool> EditBranchAsync(Branch editBranchObj)
            {
                try
                {
                    _reference.Objects.Reload();

                    // Находим ReferenceObject по имени (желательно изменить на поиск по Id)
                    var editBranch = _reference.Objects.FirstOrDefault(branch => branch.ToString().Equals(editBranchObj.Name));
                    if (editBranch != null)
                    {
                        await editBranch.BeginChangesAsync();

                        editBranch[_nameParameter.Guid].Value = editBranchObj.Name;
                        editBranch[_objectStringParameter.Guid].Value = new JsonSerializer().Serialize(editBranchObj);

                        await editBranch.EndChangesAsync();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при редактировании филиала: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Клонирует существующий филиал.
        /// Создаёт копию с новым Guid и именем с суффиксом " - копия".
        /// </summary>
        /// <param name="selectedBranch">Филиал, который нужно клонировать.</param>
        /// <returns>True — если клонирование прошло успешно, иначе false.</returns>
        public async Task<bool> CloneBranch(ReferenceModelInfo selectedBranch)
        {
            try
            {
                if (_nameParameter == null || _objectStringParameter == null)
                    return false;

                var newBranch = _reference.CreateReferenceObject(selectedBranch.Type);

                var newName = $"{selectedBranch.Name} - копия";

                newBranch[_nameParameter.Guid].Value = newName;

                if (selectedBranch.ObjectStruct != null)
                {
                    newBranch[_objectStringParameter.Guid].Value =
                        selectedBranch.ObjectStruct.Replace(selectedBranch.Name, newName)
                                                  .Replace(selectedBranch.Id.ToString(), newBranch.Guid.ToString());
                }

                await newBranch.EndChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TemplateManager] Ошибка при копировании: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получает все филиалы из ReferenceLoader.
        /// Фильтрует объекты по классу "Филиалы" и возвращает коллекцию для биндинга.
        /// </summary>
        /// <returns>Коллекция филиалов.</returns>
        public ObservableCollection<ReferenceModelInfo> GetAllBranches()
        {
            try
            {
                return new ObservableCollection<ReferenceModelInfo>(
                    _referenceLoader.LoadReference(_branchInfo)
                    .Where(branch => branch.Type.Equals(_branchClass)
                                && !NavigationService.GetTabs().Any(tab => tab.Title.Equals(branch.Name)))
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке филиалов: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Асинхронно удаляет филиал из Reference.
        /// </summary>
        /// <param name="branch">Филиал для удаления.</param>
        /// <returns>True — если удаление прошло успешно, иначе false.</returns>
        public async Task<bool> RemoveBranch(ReferenceModelInfo branch)
        {
            try
            {
                var branchToRemove = await _reference.FindAsync(branch.Id);

                if (branchToRemove == null)
                    return false;

                await branchToRemove.DeleteAsync();

                branch = null;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TemplateManager] Ошибка при удалении: {ex.Message}");
                return false;
            }
        }

        public BranchManager DeepCopy()
        {
            var json = new JsonSerializer().Serialize(this);
            return new JsonSerializer().Deserialize<BranchManager>(json);
        }
    }
}

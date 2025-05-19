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
    /// Менеджер для работы с филиалами (Branches) через Reference объекты.
    /// Позволяет добавлять, клонировать, получать список и удалять филиалы.
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
        /// Конструктор инициализирует все необходимые зависимости и параметры.
        /// Выбрасывает исключение, если не удаётся найти обязательные параметры или класс.
        /// </summary>
        /// <param name="referenceLoader">Загрузчик Reference объектов.</param>
        /// <param name="branchInfo">Информация о Reference, описывающем филиалы.</param>
        public BranchManager(ServerReferenceLoader referenceLoader, ReferenceInfo branchInfo)
        {
            _referenceLoader = referenceLoader;
            _branchInfo = branchInfo;
            _reference = branchInfo.CreateReference();

            // Получаем класс филиалов и параметры "Наименование" и "Структура файла"
            _branchClass = _branchInfo.Classes.Find("Филиалы");
            _nameParameter = _reference.ParameterGroup.Parameters.FindByName("Наименование");
            _objectStringParameter = _reference.ParameterGroup.Parameters.FindByName("Структура файла");

            // Если что-то не найдено — это критическая ошибка, выбрасываем исключение
            if (_nameParameter == null || _objectStringParameter == null || _branchClass == null)
                throw new InvalidOperationException("Не найдены необходимые параметры или класс филиалов в Reference");
        }

        /// <summary>
        /// Асинхронно добавляет новый филиал.
        /// Создаёт ReferenceObject, заполняет параметры и сохраняет изменения.
        /// </summary>
        /// <param name="createBranch">Модель филиала для добавления.</param>
        /// <returns>True, если успешно добавлено, иначе false.</returns>
        public Task<bool> AddBranch(Branch createBranch)
        {
            // Проверка до создания стейт-машины
            if (_nameParameter == null || _objectStringParameter == null)
                return Task.FromResult(false);

            return AddBranchAsync(createBranch);

            async Task<bool> AddBranchAsync(Branch createBranch)
            {
                try
                {
                    var newBranch = _reference.CreateReferenceObject();

                    createBranch.Id = newBranch.Guid;

                    newBranch[_nameParameter.Guid].Value = createBranch.Name;
                    newBranch[_objectStringParameter.Guid].Value = new JsonSerializer().Serialize(createBranch);

                    await newBranch.EndChangesAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Клонирует существующий филиал.
        /// Создаёт копию с новым Guid и именем с суффиксом " - копия".
        /// </summary>
        /// <param name="selectedBranch">Филиал для клонирования.</param>
        /// <returns>True, если успешно, иначе false.</returns>
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
        /// Фильтрует по классу "Филиалы" и возвращает коллекцию для биндинга.
        /// </summary>
        /// <returns>Коллекция всех филиалов.</returns>
        public ObservableCollection<ReferenceModelInfo> GetAllBranches()
        {
            return new ObservableCollection<ReferenceModelInfo>(
                _referenceLoader.LoadReference(_branchInfo)
                .Where(branch => branch.Type.Equals(_branchClass))
            );
        }

        /// <summary>
        /// Асинхронно удаляет филиал из Reference.
        /// </summary>
        /// <param name="branch">Филиал для удаления.</param>
        /// <returns>True, если успешно удалено, иначе false.</returns>
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
    }
}

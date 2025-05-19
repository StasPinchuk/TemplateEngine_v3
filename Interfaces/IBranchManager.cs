using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TemplateEngine_v3.Models;

namespace TemplateEngine_v3.Interfaces
{
    /// <summary>
    /// Интерфейс для управления филиалами.
    /// </summary>
    public interface IBranchManager
    {
        /// <summary>
        /// Получает коллекцию всех филиалов.
        /// </summary>
        /// <returns>Коллекция с информацией о филиалах.</returns>
        ObservableCollection<ReferenceModelInfo> GetAllBranches();

        /// <summary>
        /// Удаляет указанный филиал.
        /// </summary>
        /// <param name="branch">Филиал для удаления.</param>
        /// <returns>Задача, возвращающая true при успешном удалении, иначе false.</returns>
        Task<bool> RemoveBranch(ReferenceModelInfo branch);

        /// <summary>
        /// Добавляет новый филиал.
        /// </summary>
        /// <param name="createBranch">Модель филиала для добавления.</param>
        /// <returns>Задача, возвращающая true при успешном добавлении, иначе false.</returns>
        Task<bool> AddBranch(Branch createBranch);
        Task<bool> EditBranch(Branch editBranchObj);

        /// <summary>
        /// Клонирует указанный филиал.
        /// </summary>
        /// <param name="branch">Филиал для клонирования.</param>
        /// <returns>Задача, возвращающая true при успешном клонировании, иначе false.</returns>
        Task<bool> CloneBranch(ReferenceModelInfo branch);
    }
}

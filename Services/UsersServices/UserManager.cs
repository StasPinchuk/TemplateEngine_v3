using System.Collections.Generic;
using System.Collections.ObjectModel;
using TemplateEngine_v3.Mappers;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References.Users;

namespace TemplateEngine_v3.Services.UsersServices
{
    /// <summary>
    /// Класс для управления пользователями, включая операции с полными и разрешенными пользователями.
    /// </summary>
    public class UserManager
    {
        private readonly ServerConnection _connection;
        private readonly AllUsersManager _allUsersManager;
        private readonly AllowedUsersManager _allowedUsersManager;
        private UserModel _currentUser;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UserManager"/>.
        /// </summary>
        /// <param name="currentConnection">Объект подключения к серверу.</param>
        /// <param name="allUsersManager">Менеджер всех пользователей.</param>
        /// <param name="allowedUsersManager">Менеджер разрешенных пользователей.</param>
        public UserManager(ServerConnection currentConnection, AllUsersManager allUsersManager, AllowedUsersManager allowedUsersManager)
        {
            _connection = currentConnection;
            _allUsersManager = allUsersManager;
            _allowedUsersManager = allowedUsersManager;
            Initialize(currentConnection);
        }

        /// <summary>
        /// Инициализирует текущего пользователя.
        /// </summary>
        /// <param name="currentConnection">Объект подключения к серверу.</param>
        private void Initialize(ServerConnection currentConnection)
        {
            User currentUser = currentConnection.ClientView.GetUser();
            _currentUser = UserMapper.FromTFlexUser(currentUser);
            ReferenceMapper.UserFio = _currentUser.FullName;

            currentUser = null;
        }

        /// <summary>
        /// Получает список всех пользователей.
        /// </summary>
        /// <returns>Список всех пользователей.</returns>
        public List<UserModel> GetAllUsers() => _allUsersManager.GetAllUsers();

        /// <summary>
        /// Получает отфильтрованный список всех пользователей, исключая текущего пользователя и разрешенных пользователей.
        /// </summary>
        /// <returns>Отфильтрованный список пользователей.</returns>
        public ObservableCollection<UserModel> GetFilteredAllUsersList() => _allUsersManager.GetFilteredUsersList(_currentUser, new()/*GetAlloweUsers()*/);

        /// <summary>
        /// Получает список разрешенных пользователей.
        /// </summary>
        /// <returns>Коллекция разрешенных пользователей.</returns>
        public ObservableCollection<UserModel> GetAlloweUsers() => _allowedUsersManager.GetAllowedUsers();

        /// <summary>
        /// Проверяет, является ли текущий пользователь в списке разрешенных.
        /// </summary>
        /// <returns><c>true</c>, если текущий пользователь в списке разрешенных; иначе <c>false</c>.</returns>
        public bool IsUserInAllowedList() => _allowedUsersManager.IsUserInList(_currentUser);

        /// <summary>
        /// Добавляет пользователя в список разрешенных.
        /// </summary>
        /// <param name="allowedUser">Пользователь, которого нужно добавить в список разрешенных.</param>
        /// <returns><c>true</c>, если пользователь был успешно добавлен; иначе <c>false</c>.</returns>
        public bool AddAllowedUser(UserModel allowedUser) => _allowedUsersManager.AddAllowedUser(allowedUser);

        /// <summary>
        /// Удаляет пользователя из списка разрешенных.
        /// </summary>
        /// <param name="allowedUser">Пользователь, которого нужно удалить из списка разрешенных.</param>
        /// <returns><c>true</c>, если пользователь был удален; иначе <c>false</c>.</returns>
        public bool RemoveAllosedUser(UserModel allowedUser) => _allowedUsersManager.RemoveAllowedUser(allowedUser);

        /// <summary>
        /// Удаляет пользователя из списка всех пользователей.
        /// </summary>
        /// <param name="selectedUser">Пользователь, которого нужно удалить.</param>
        /// <returns><c>true</c>, если пользователь был удален; иначе <c>false</c>.</returns>
        public bool RemoveUser(User selectedUser) => _allUsersManager.RemoveUser(selectedUser);
    }
}

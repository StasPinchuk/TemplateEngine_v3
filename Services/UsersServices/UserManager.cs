using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        // Подключение к серверу T-FLEX DOCs
        private readonly ServerConnection _connection;

        // Менеджер всех пользователей
        private readonly AllUsersManager _allUsersManager;

        // Менеджер разрешённых пользователей
        private readonly AllowedUsersManager _allowedUsersManager;

        // Текущий пользователь системы
        private UserModel _currentUser;

        /// <summary>
        /// Текущий пользователь (модель)
        /// </summary>
        public UserModel CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UserManager"/>.
        /// </summary>
        /// <param name="currentConnection">Объект подключения к серверу.</param>
        /// <param name="allUsersManager">Менеджер всех пользователей.</param>
        /// <param name="allowedUsersManager">Менеджер разрешённых пользователей.</param>
        public UserManager(ServerConnection currentConnection, AllUsersManager allUsersManager, AllowedUsersManager allowedUsersManager)
        {
            _connection = currentConnection;
            _allUsersManager = allUsersManager;
            _allowedUsersManager = allowedUsersManager;

            // Инициализация текущего пользователя
            Initialize(currentConnection);
        }

        /// <summary>
        /// Инициализирует текущего пользователя на основе активного подключения.
        /// </summary>
        /// <param name="currentConnection">Подключение к серверу.</param>
        private void Initialize(ServerConnection currentConnection)
        {
            // Получение текущего пользователя через клиентское представление
            User currentUser = currentConnection.ClientView.GetUser();

            // Поиск текущего пользователя в списке разрешённых пользователей
            CurrentUser = GetAlloweUsers().FirstOrDefault(user => user.FullName.Equals(currentUser.ToString()));

            // Обнуление ссылки на оригинальный объект User (опционально, освобождение памяти)
            currentUser = null;
        }

        /// <summary>
        /// Возвращает список всех пользователей.
        /// </summary>
        public List<UserModel> GetAllUsers() =>
            _allUsersManager.GetAllUsers();

        /// <summary>
        /// Возвращает отфильтрованный список всех пользователей, исключая текущего и разрешённых.
        /// </summary>
        public ObservableCollection<UserModel> GetFilteredAllUsersList() =>
            _allUsersManager.GetFilteredUsersList(_currentUser, new() /* можно заменить на GetAlloweUsers() */);

        /// <summary>
        /// Возвращает список разрешённых пользователей.
        /// </summary>
        public ObservableCollection<UserModel> GetAlloweUsers() =>
            _allowedUsersManager.GetAllowedUsers();

        /// <summary>
        /// Проверяет, входит ли текущий пользователь в список разрешённых.
        /// </summary>
        public bool IsUserInAllowedList() =>
            _allowedUsersManager.IsUserInList(_currentUser);

        /// <summary>
        /// Добавляет пользователя в список разрешённых.
        /// </summary>
        /// <param name="allowedUser">Добавляемый пользователь.</param>
        public bool AddAllowedUser(UserModel allowedUser) =>
            _allowedUsersManager.AddAllowedUser(allowedUser);

        /// <summary>
        /// Изменяет данные разрешённого пользователя.
        /// </summary>
        /// <param name="allowedUser">Пользователь с новыми данными.</param>
        public bool EditAllowedUser(UserModel allowedUser)
        {
            // Изменение пользователя в списке
            bool isEdit = _allowedUsersManager.EditAllowedUser(allowedUser);

            // Обновление текущего пользователя, если он редактировался
            if (isEdit && CurrentUser.FullName.Equals(allowedUser.FullName))
                CurrentUser = allowedUser;

            return isEdit;
        }

        /// <summary>
        /// Удаляет пользователя из списка разрешённых.
        /// </summary>
        public bool RemoveAllosedUser(UserModel allowedUser) =>
            _allowedUsersManager.RemoveAllowedUser(allowedUser);

        /// <summary>
        /// Удаляет пользователя из общего списка пользователей.
        /// </summary>
        public bool RemoveUser(User selectedUser) =>
            _allUsersManager.RemoveUser(selectedUser);
    }
}

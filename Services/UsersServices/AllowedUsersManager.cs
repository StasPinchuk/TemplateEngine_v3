using System.Collections.ObjectModel;
using System.Linq;
using TemplateEngine_v3.Mappers;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References.Users;

namespace TemplateEngine_v3.Services.UsersServices
{
    /// <summary>
    /// Класс для управления списком разрешенных пользователей.
    /// </summary>
    public class AllowedUsersManager
    {
        private readonly ServerConnection _connection;
        private ObservableCollection<UserModel> _allowedUsers;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AllowedUsersManager"/>.
        /// </summary>
        /// <param name="serverConnection">Объект подключения к серверу для получения данных о пользователях.</param>
        public AllowedUsersManager(ServerConnection serverConnection)
        {
            _connection = serverConnection;
            SetAllowedUsersList();
        }

        /// <summary>
        /// Получает разрешенного пользователя по полному имени.
        /// </summary>
        /// <param name="userFullName">Полное имя пользователя.</param>
        /// <returns>Пользователь с заданным полным именем или <c>null</c>, если не найден.</returns>
        public UserModel GetUserByFullName(string userFullName)
        {
            return _allowedUsers.FirstOrDefault(user => user.FullName.Equals(userFullName));
        }

        /// <summary>
        /// Получает коллекцию разрешенных пользователей.
        /// </summary>
        /// <returns>Коллекция разрешенных пользователей в виде <see cref="ObservableCollection{UserModel}"/>.</returns>
        public ObservableCollection<UserModel> GetAllowedUsers()
        {
            return _allowedUsers;
        }

        /// <summary>
        /// Устанавливает список разрешенных пользователей (в данный момент закомментировано).
        /// </summary>
        private void SetAllowedUsersList()
        {
            /*var AllUsers = new UserReference(_connection).GetAllUsers();
            _allowedUsers = UserMapper.FromTFlexUsersList(AllUsers);*/
        }

        /// <summary>
        /// Добавляет пользователя в список разрешенных, если он еще не в нем присутствует.
        /// </summary>
        /// <param name="user">Пользователь, которого нужно добавить.</param>
        /// <returns><c>true</c>, если пользователь был успешно добавлен; иначе <c>false</c>.</returns>
        public bool AddAllowedUser(UserModel user)
        {
            if (IsUserInList(user))
            {
                return false;
            }

            _allowedUsers.Add(user);
            return true;
        }

        /// <summary>
        /// Удаляет пользователя из списка разрешенных.
        /// </summary>
        /// <param name="user">Пользователь, которого нужно удалить.</param>
        /// <returns><c>true</c>, если пользователь был удален; иначе <c>false</c>.</returns>
        public bool RemoveAllowedUser(UserModel user)
        {
            var userToRemove = GetUserByFullName(user.FullName);

            if (userToRemove != null)
            {
                _allowedUsers.Remove(userToRemove);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет, присутствует ли пользователь в списке разрешенных.
        /// </summary>
        /// <param name="currentUser">Пользователь, которого нужно проверить.</param>
        /// <returns><c>true</c>, если пользователь находится в списке разрешенных; иначе <c>false</c>.</returns>
        public bool IsUserInList(UserModel currentUser)
        {
            return _allowedUsers.Any(user => user.Id == currentUser.Id);
        }
    }
}

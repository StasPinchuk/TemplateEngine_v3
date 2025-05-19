using System;
using System.Collections.ObjectModel;
using System.Linq;
using TemplateEngine_v3.Mappers;
using TemplateEngine_v3.Models;
using TemplateEngine_v3.Services.FileServices;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.References.Users;

namespace TemplateEngine_v3.Services.UsersServices
{
    /// <summary>
    /// Класс для управления списком разрешенных пользователей.
    /// </summary>
    public class AllowedUsersManager
    {
        private readonly ServerConnection _connection;
        private ObservableCollection<UserModel> _allowedUsers = [];
        private Reference _userWithPermissionReference;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AllowedUsersManager"/>.
        /// </summary>
        /// <param name="serverConnection">Объект подключения к серверу для получения данных о пользователях.</param>
        public AllowedUsersManager(ServerConnection serverConnection)
        {
            _connection = serverConnection;
            _userWithPermissionReference = _connection.ReferenceCatalog.Find(new Guid("1cadafb6-5234-4fe6-a8f5-b6415b05a436")).CreateReference();
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
            _userWithPermissionReference.Objects.Reload();
            var structObject = _userWithPermissionReference.ParameterGroup.Parameters.FindByName("Структура файла");
            foreach(var user in _userWithPermissionReference.Objects)
            {
                string objectStruct = user[structObject.Guid].Value.ToString();
                var curUser = new JsonSerializer().Deserialize<UserModel>(objectStruct);
                _allowedUsers.Add(
                    curUser
                );
            }
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

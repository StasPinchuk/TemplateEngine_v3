using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TemplateEngine_v3.Mappers;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Common;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References.Users;

namespace TemplateEngine_v3.Services.UsersServices
{
    /// <summary>
    /// Класс для управления списком пользователей, включая фильтрацию и удаление пользователей.
    /// </summary>
    public class AllUsersManager
    {
        private readonly ServerConnection _connection;
        private List<User> _allUsers;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AllUsersManager"/>.
        /// </summary>
        /// <param name="serverConnection">Объект подключения к серверу для получения данных о пользователях.</param>
        public AllUsersManager(ServerConnection serverConnection)
        {
            _connection = serverConnection;
            var currentUser = UserMapper.FromTFlexUser(_connection.ClientView.GetUser());
            SetUsersList();
        }

        /// <summary>
        /// Получает список всех пользователей.
        /// </summary>
        /// <returns>Список всех пользователей.</returns>
        public List<User> GetAllUsers()
        {
            return _allUsers;
        }

        /// <summary>
        /// Получает пользователя по полному имени.
        /// </summary>
        /// <param name="userFullName">Полное имя пользователя.</param>
        /// <returns>Пользователь с заданным полным именем или <c>null</c>, если не найден.</returns>
        public User GetUserByFullName(string userFullName)
        {
            return _allUsers.FirstOrDefault(user => user.FullName.Equals(userFullName));
        }

        /// <summary>
        /// Получает фильтрованный список пользователей, исключая текущего пользователя и указанных пользователей.
        /// </summary>
        /// <param name="currentUser">Текущий пользователь, который будет исключен из списка.</param>
        /// <param name="excludedUsers">Список пользователей, которые будут исключены из результата.</param>
        /// <returns>Отфильтрованный список пользователей в виде <see cref="ObservableCollection{User}"/>.</returns>
        public ObservableCollection<User> GetFilteredUsersList(UserModel currentUser, ObservableCollection<UserModel> excludedUsers)
        {
            return new ObservableCollection<User>(
                _allUsers.Where(user =>
                    !user.FullName.Equals(currentUser.FullName) &&
                    !excludedUsers.Any(excludedUser => excludedUser.FullName == user.FullName)
                )
            );
        }


        /// <summary>
        /// Устанавливает список всех пользователей, фильтруя по валидности полного имени.
        /// </summary>
        private void SetUsersList()
        {
            _allUsers = new UserReference(_connection).GetAllUsers().Where(user => IsFullNameValid(user.FullName)).ToList();
        }

        /// <summary>
        /// Удаляет пользователя из списка.
        /// </summary>
        /// <param name="user">Пользователь, которого нужно удалить.</param>
        /// <returns><c>true</c>, если пользователь был удален; иначе <c>false</c>.</returns>
        public bool RemoveUser(User user)
        {
            var userToRemove = GetUserByFullName(user.FullName);

            if (userToRemove != null)
            {
                _allUsers.Remove(userToRemove);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет, является ли полное имя пользователя валидным (состоящим из трех слов).
        /// </summary>
        /// <param name="fullName">Полное имя пользователя.</param>
        /// <returns><c>true</c>, если полное имя валидно; иначе <c>false</c>.</returns>
        private static bool IsFullNameValid(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return false;

            var words = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Length == 3;
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model.References.Users;

namespace TemplateEngine_v3.Mappers
{
    /// <summary>
    /// Класс для преобразования объектов пользователя TFlex в модели приложения.
    /// </summary>
    public static class UserMapper
    {
        /// <summary>
        /// Преобразует объект <see cref="User"/> из TFlex в модель <see cref="UserModel"/>.
        /// </summary>
        /// <param name="user">Объект пользователя из TFlex.</param>
        /// <returns>Модель <see cref="UserModel"/> или <c>null</c>, если входной объект равен <c>null</c>.</returns>
        public static UserModel FromTFlexUser(User user)
        {
            if (user == null)
                return null;

            return new UserModel
            {
                Id = user.Id.ToString(),
                LastName = user.LastName,
                FirstName = user.FirstName,
                Patronymic = user.Patronymic,
                Description = user.Description
            };
        }

        /// <summary>
        /// Преобразует список объектов <see cref="User"/> из TFlex в коллекцию <see cref="ObservableCollection{UserModel}"/>.
        /// </summary>
        /// <param name="users">Список пользователей из TFlex.</param>
        /// <returns>Коллекция моделей <see cref="UserModel"/> или <c>null</c>, если входной список равен <c>null</c>.</returns>
        public static ObservableCollection<UserModel> FromTFlexUsersList(List<User> users)
        {
            if (users == null) return null;

            return new ObservableCollection<UserModel>(users.Select(user => FromTFlexUser(user)));
        }
    }
}

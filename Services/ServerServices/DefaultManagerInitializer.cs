using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Services.FileServices;
using TemplateEngine_v3.Services.ReferenceServices;
using TemplateEngine_v3.Services.UsersServices;
using TFlex.DOCs.Model;

namespace TemplateEngine_v3.Services.ServerServices
{
    public class DefaultManagerInitializer : IManagerInitializer
    {
        public UserManager InitializeUserManager(ServerConnection connection)
        {
            var allUsers = new AllUsersManager(connection);
            var allowedUsers = new AllowedUsersManager(connection);
            return new UserManager(connection, allUsers, allowedUsers);
        }

        public ReferenceManager InitializeReferenceManager(ServerConnection connection)
        {
            var referenceLoader = new ServerReferenceLoader();
            var jsonreferenceLoader = new JsonReferenceLoader();
            return new ReferenceManager(connection, jsonreferenceLoader);
        }
    }
}

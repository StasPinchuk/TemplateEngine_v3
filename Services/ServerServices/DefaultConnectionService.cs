using System.Threading.Tasks;
using TemplateEngine_v3.Interfaces;
using TemplateEngine_v3.Models;
using TFlex.DOCs.Model;
using TFlex.PdmFramework.Resolve;

namespace TemplateEngine_v3.Services.ServerServices
{
    public class DefaultConnectionService : IConnectionService
    {
        public Task<ServerConnection> ConnectAsync(UserCredentials credentials)
        {
            AssemblyResolver.Instance.AddDirectory(credentials.ApiPath);
            return ServerConnection.OpenAsync(credentials.Login, credentials.Password, credentials.ServerIp);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateEngine_v3.Models.LogModels
{
    public enum LogActionType
    {
        Create,
        Update,
        Delete,
        Import,
        Export,
        Open,
        Other
    }


    public class UserLogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TemplateName { get; set; }
        public string UserFullName { get; set; }
        public string UserId { get; set; } // Дополнительно
        public LogActionType LogType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public List<UserLogOperation> Operations { get; set; } = new();
    }

}

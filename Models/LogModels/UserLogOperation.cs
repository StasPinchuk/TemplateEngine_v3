using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateEngine_v3.Models.LogModels
{
    public enum EntityType
    {
        Node,
        Operation,
        Parameter,
        Formula,
        Term,
        Technology,
        Other
    }

    public class UserLogOperation
    {
        public EntityType EntityType { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Message { get; set; }
    }

}

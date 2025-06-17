using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateEngine_v3.Models.LogModels
{
    public class LogObjectGroup
    {
        public string ObjectName { get; set; }       // ID или имя объекта (шаблон, ТП и т.д.)
        public string ObjectType { get; set; }     // Тип объекта (Template, TechProcess, Branch и т.д.)
        public string User { get; set; }
        public List<LogEntry> Entries { get; set; } = new();

        public LogObjectGroup() { }
        public LogObjectGroup(string objectName, string objectType, string user)
        {
            ObjectName = objectName; 
            ObjectType = objectType;
            User = user;
        }
    }

}

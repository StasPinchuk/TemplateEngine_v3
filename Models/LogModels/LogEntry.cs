using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateEngine_v3.Models.LogModels
{
    public enum LogActionType
    {
        Edit,
        Create, 
        Update, 
        Delete,
        Error
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }    // Точное время события
        public LogActionType ActionType { get; set; } // Тип действия: Edit, Create, Error, Delete
        public string Message { get; set; }        // Описание действия

        public LogEntry() { }

        public LogEntry(LogActionType actionType, string message)
        {
            Timestamp = DateTime.Now;
            ActionType = actionType;
            Message = message;
        }
    }

}

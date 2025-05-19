using System;

namespace TemplateEngine_v3.Models
{
    public class UserPermission
    {
        public string Key { get; set; }
        public Enum Permission { get; set; }
        public bool IsChecked { get; set; }
    }
}

using System.ComponentModel;

namespace TemplateEngine_v3.Models
{
    public enum StatusType
    {
        [Description("Черновик")]
        Draft,

        [Description("Проверка")]
        Verification,

        [Description("Завершён")]
        Final,

        [Description("Отклонён")]
        Rejected,

        [Description("Архив")]
        Archive
    }
}

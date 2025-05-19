using System;

namespace TemplateEngine_v3.Models
{
    public interface ITemplatedFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}

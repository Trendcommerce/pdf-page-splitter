using System;
using System.ComponentModel.DataAnnotations;

namespace pdf_page_splitter.Data
{
    public abstract partial class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

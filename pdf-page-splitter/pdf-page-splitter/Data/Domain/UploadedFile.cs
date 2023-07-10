using System.Collections.Generic;

namespace pdf_page_splitter.Data.Domain
{
    public class UploadedFile : BaseEntity
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string FailedReason { get; set; }

        public virtual ICollection<SplittedFile> SplittedFiles { get; set; }
    }
}

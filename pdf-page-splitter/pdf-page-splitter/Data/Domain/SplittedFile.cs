using System.ComponentModel.DataAnnotations.Schema;

namespace pdf_page_splitter.Data.Domain
{
    public class SplittedFile : BaseEntity
    {
        public string SlaName { get; set; }
        public string PageType { get; set; }
        public string DocumentId { get; set; }
        public string CountryCode { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FailedReason { get; set; }
        public string PageNumbersInBasePdf { get; set; }
        public string Status { get; set; }

        public int UploadedFileId { get; set; }

        [ForeignKey("UploadedFileId"), InverseProperty("SplittedFiles")]
        public virtual UploadedFile UploadedFile { get; set; }
    }
}

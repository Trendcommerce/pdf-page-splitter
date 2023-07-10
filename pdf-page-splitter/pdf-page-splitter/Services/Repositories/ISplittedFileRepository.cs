using pdf_page_splitter.Data.Domain;
using System.Threading.Tasks;

namespace pdf_page_splitter.Services.Repositories
{
    public interface ISplittedFileRepository : IGenericRepository<SplittedFile>
    {
        Task<SplittedFile> GetExecutedByDocumentIdAsync(string documentId, bool? isActive = null);
        Task<SplittedFile> GetByFilePathAsync(string documentPath, bool? isActive = null);
    }
}

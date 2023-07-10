using Microsoft.EntityFrameworkCore;
using pdf_page_splitter.Data;
using pdf_page_splitter.Data.Domain;
using pdf_page_splitter.Infrastructure;
using System.Threading.Tasks;

namespace pdf_page_splitter.Services.Repositories
{
    public class SplittedFileRepository : GenericRepository<SplittedFile>, ISplittedFileRepository
    {
        private readonly PdfPageSplitterObjectContext _context;

        public SplittedFileRepository(PdfPageSplitterObjectContext context) : base(context) => this._context = context;

        public async Task<SplittedFile> GetExecutedByDocumentIdAsync(string documentId, bool? isActive = null)
        {
            return await _context.SplittedFiles.Include(i => i.UploadedFile)
                                               .FirstOrDefaultAsync(x => x.DocumentId == documentId && x.UploadedFile.Status == AppConstant.UploadedFileStatusExecuted && (isActive == null || x.IsActive == isActive));
        }

        public async Task<SplittedFile> GetByFilePathAsync(string filePath, bool? isActive = null)
        {
            return await _context.SplittedFiles.FirstOrDefaultAsync(x => x.FilePath == filePath && (isActive == null || x.IsActive == isActive));
        }
    }
}

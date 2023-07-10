using Microsoft.EntityFrameworkCore;
using pdf_page_splitter.Data;
using pdf_page_splitter.Data.Domain;
using pdf_page_splitter.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pdf_page_splitter.Services.Repositories
{
    public class UploadedFileRepository : GenericRepository<UploadedFile>, IUploadedFileRepository
    {
        private readonly PdfPageSplitterObjectContext _context;

        public UploadedFileRepository(PdfPageSplitterObjectContext context) : base(context) => this._context = context;

        public async Task<IEnumerable<UploadedFile>> GetAllExecutedAsync(bool? isActive = null)
        {
            return await _context.UploadedFiles.Where(x => x.Status == AppConstant.UploadedFileStatusExecuted && (isActive == null || x.IsActive == isActive)).ToListAsync();
        }
    }
}

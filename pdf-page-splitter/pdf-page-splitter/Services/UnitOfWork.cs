using pdf_page_splitter.Data;
using pdf_page_splitter.Services.Repositories;
using System;
using System.Threading.Tasks;

namespace pdf_page_splitter.Services
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly PdfPageSplitterObjectContext _context;

        public UnitOfWork(PdfPageSplitterObjectContext context)
        {
            _context = context;
            SplittedFile = new SplittedFileRepository(context);
            UploadedFile = new UploadedFileRepository(context);
        }

        public ISplittedFileRepository SplittedFile { get; private set; }
        public IUploadedFileRepository UploadedFile { get; private set; }

        public async Task CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}

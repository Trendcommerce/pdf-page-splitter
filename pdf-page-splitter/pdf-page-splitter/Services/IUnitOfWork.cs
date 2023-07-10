using pdf_page_splitter.Services.Repositories;
using System.Threading.Tasks;

namespace pdf_page_splitter.Services
{
    public interface IUnitOfWork
    {
        ISplittedFileRepository SplittedFile { get; }
        IUploadedFileRepository UploadedFile { get; }
        Task CompleteAsync();
        void Dispose();
    }
}

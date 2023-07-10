using pdf_page_splitter.Data.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pdf_page_splitter.Services.Repositories
{
    public interface IUploadedFileRepository : IGenericRepository<UploadedFile>
    {
        Task<IEnumerable<UploadedFile>> GetAllExecutedAsync(bool? isActive = null);
    }
}

using pdf_page_splitter.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace pdf_page_splitter.Services
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync(bool? isActive = null);

        Task<T> GetByIdAsync(int id, bool? isActive = null);

        Task<bool> CreateAsync(T entity);

        Task<bool> UpdateAsync(T entity);

        Task<bool> DeleteAsync(T entity);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}

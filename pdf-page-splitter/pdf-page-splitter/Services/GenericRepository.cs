using Microsoft.EntityFrameworkCore;
using pdf_page_splitter.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace pdf_page_splitter.Services
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        internal DbSet<T> _dbSet;
        private readonly PdfPageSplitterObjectContext _context;

        public GenericRepository(PdfPageSplitterObjectContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(bool? isActive = null)
        {
            return await _dbSet.Where(x => isActive == null || x.IsActive == isActive).ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(int id, bool? isActive = null)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id && (isActive == null || x.IsActive == isActive));
        }

        public async Task<bool> CreateAsync(T entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            entity.DeletedAt = DateTime.Now;
            entity.IsActive = false;
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();
    }
}

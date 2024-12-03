using Microsoft.EntityFrameworkCore;
using StudyAPI.Data;
using StudyAPI.Models;
using StudyAPI.Repository.IRepository;
using System.Linq.Expressions;

namespace StudyAPI.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly VillaDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(VillaDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }
        public async Task CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await SaveAsync();

        }

        public Task DeleteAsync(T entity)
        {
            dbSet.Remove(entity);
            return SaveAsync();
        }

        public Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
                query = query.Where(filter);
            if (!tracked)
                query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, int pageSize = 0, int pageNumber = 1)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
                query = query.Where(filter);

            if (pageSize > 0)
            {
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
                query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            }

            return await query.ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

       
    }
}
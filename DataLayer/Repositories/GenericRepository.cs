using Microsoft.EntityFrameworkCore;
using BusinessLayer.Interfaces.Repositories;
using System.Linq.Expressions;

namespace DataLayer.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly CraftflowDbContext context;
        protected readonly DbSet<T> dbSet;

        public GenericRepository(CraftflowDbContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
            => await dbSet.ToListAsync();

        public async Task<T> GetByIdAsync(int id)
            => await dbSet.FindAsync(id);

        public async Task AddAsync(T entity)
            => await dbSet.AddAsync(entity);

        public void Update(T entity)
            => dbSet.Update(entity);

        public void Delete(T entity)
            => dbSet.Remove(entity);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await dbSet.Where(predicate).ToListAsync();
    }
}
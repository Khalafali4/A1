using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Da3m.Data.Repositories
{

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly Da3mDbContext _context;
        protected readonly DbSet<T> _dbSet;

        // Constructor 
        public GenericRepository(Da3mDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // GetAll
        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        //GetById
        public async Task<T?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        //Find 
        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        // Any 
        public async Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        // Add
        public async Task AddAsync(T entity)
            => await _dbSet.AddAsync(entity);

        // Update
        public void Update(T entity)
            => _dbSet.Update(entity);

        // Delete
        public void Delete(T entity)
            => _dbSet.Remove(entity);
    }
}
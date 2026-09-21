using Microsoft.EntityFrameworkCore;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.Infrastructure.Specifications;

namespace StocksApp.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        public GenericRepository(ApplicationDbContext context) => _context = context;

        public async Task<T?> GetByIdAsync(Guid id) =>
            await _context.Set<T>().FindAsync(id);

        public async Task<T?> GetAsync(ISpecification<T> spec) =>
            await ApplySpecification(spec).FirstOrDefaultAsync();

        public async Task<List<T>> ListAsync(ISpecification<T> spec) =>
            await ApplySpecification(spec).ToListAsync();

        public async Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec) =>
            await ApplySpecification(spec).Select(spec.Selector).ToListAsync();

        public async Task<int> CountAsync(ISpecification<T> spec) =>
            await ApplySpecification(spec).CountAsync();

        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> spec) =>
            SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
    }
}
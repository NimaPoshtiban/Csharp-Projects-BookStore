using System.Linq.Expressions;

namespace BookStore.DataAccess.Repository.IRepository;

public interface IRepository<T> where T : class
{
    Task<T> GetFirstOrDefault(Expression<Func<T,bool>> filter,string? includeProperties=null);
    Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null);
    Task AddAsync(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
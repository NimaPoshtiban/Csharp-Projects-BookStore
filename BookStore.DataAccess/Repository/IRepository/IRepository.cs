using System.Linq.Expressions;

namespace BookStore.DataAccess.Repository.IRepository;

public interface IRepository<T> where T : class
{
    Task<T> GetFirstOrDefault(Expression<Func<T,bool>> filter);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
using System.Linq.Expressions;
using Bifrost.Core.Models;

namespace Bifrost.Core.Repositories;

public interface IRepository<T> where T : Entity
{
    Task<T?> GetById(long id);
    Task<IEnumerable<T>> GetAll();
    Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate);
    Task Add(T entity);
    Task AddRange(IEnumerable<T> entities);
    Task Update(T entity);
    Task Remove(T entity);
    Task RemoveRange(IEnumerable<T> entities);
}

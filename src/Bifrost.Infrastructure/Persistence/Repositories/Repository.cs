using System.Linq.Expressions;
using Bifrost.Core.Models;
using Bifrost.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Bifrost.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly BifrostDbContext Context;

    public Repository(BifrostDbContext context)
    {
        Context = context;
    }

    public async Task<T?> GetById(long id)
    {
        return await Context.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        return await Context.Set<T>().ToListAsync();
    }

    public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate)
    {
        return await Context.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task Add(T entity)
    {
        await Context.Set<T>().AddAsync(entity);
    }

    public async Task AddRange(IEnumerable<T> entities)
    {
        await Context.Set<T>().AddRangeAsync(entities);
    }

    public void Remove(T entity)
    {
        Context.Set<T>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        Context.Set<T>().RemoveRange(entities);
    }
}

using Bifrost.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bifrost.Infrastructure.Tests.Repositories;

public class RepositoryTestBase
{
    protected BifrostDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<BifrostDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new BifrostDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}

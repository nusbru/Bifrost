using Bifrost.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bifrost.Infrastructure.Tests.Fixtures;

public class BifrostDbContextFixture : IDisposable
{
    public BifrostDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BifrostDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new BifrostDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
    }
}

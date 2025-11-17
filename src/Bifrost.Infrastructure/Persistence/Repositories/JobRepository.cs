using Bifrost.Core.Models;
using Bifrost.Core.Repositories;

namespace Bifrost.Infrastructure.Persistence.Repositories;

public class JobRepository : Repository<Job>, IJobRepository
{
    public JobRepository(BifrostDbContext context) : base(context)
    {
    }
}

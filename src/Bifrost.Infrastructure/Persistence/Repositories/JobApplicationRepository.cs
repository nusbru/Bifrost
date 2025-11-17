using Bifrost.Core.Models;
using Bifrost.Core.Repositories;

namespace Bifrost.Infrastructure.Persistence.Repositories;

public class JobApplicationRepository : Repository<JobApplication>, IJobApplicationRepository
{
    public JobApplicationRepository(BifrostDbContext context) : base(context)
    {
    }
}

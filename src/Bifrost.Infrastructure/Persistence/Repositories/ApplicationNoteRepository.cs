using Bifrost.Core.Models;
using Bifrost.Core.Repositories;

namespace Bifrost.Infrastructure.Persistence.Repositories;

public class ApplicationNoteRepository : Repository<ApplicationNote>, IApplicationNoteRepository
{
    public ApplicationNoteRepository(BifrostDbContext context) : base(context)
    {
    }
}

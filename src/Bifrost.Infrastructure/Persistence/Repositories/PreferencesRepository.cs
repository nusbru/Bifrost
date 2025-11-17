using Bifrost.Core.Models;
using Bifrost.Core.Repositories;

namespace Bifrost.Infrastructure.Persistence.Repositories;

public class PreferencesRepository : Repository<Preferences>, IPreferencesRepository
{
    public PreferencesRepository(BifrostDbContext context) : base(context)
    {
    }
}

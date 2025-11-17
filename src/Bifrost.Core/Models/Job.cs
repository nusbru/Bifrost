using Bifrost.Core.Enums;

namespace Bifrost.Core.Models;

public class Job : Entity
{
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PublicationLink { get; set; }  = string.Empty;
    public JobType JobType { get; set; } = JobType.None;
    public string Description { get; set; } = string.Empty;
    public bool OfferSponsorship { get; set; } = true;
    public bool OfferRelocation {get; set;} = true;

    public virtual JobApplication? JobApplication { get; set; }
}

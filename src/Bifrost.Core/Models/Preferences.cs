using Bifrost.Core.Enums;

namespace Bifrost.Core.Models;

public class Preferences : Entity
{
    public JobType JobType { get; set; } = JobType.None;
    public SalaryRange SalaryRange { get; set; }
    public bool NeedSponsorship { get; set; } = true;
    public bool NeedRelocation {get; set;} = true;
}
namespace Bifrost.Core.Models;

public class JobApplication : Entity
{
    public long JobId { get; set; }
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.NotApplied;
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    
    public virtual Job Job { get; set; }
    public virtual IEnumerable<ApplicationNote> Notes { get; set; } = [];
}
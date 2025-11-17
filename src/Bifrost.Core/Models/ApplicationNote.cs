namespace Bifrost.Core.Models;

public class ApplicationNote : Entity
{
    public long JobApplicationId { get; set; }
    public string Note { get; set; } = String.Empty;
    
    public virtual JobApplication JobApplication { get; set; }
}
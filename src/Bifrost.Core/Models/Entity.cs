namespace Bifrost.Core.Models;

public abstract class Entity
{
    public long  Id { get; set; }
    public Guid SupabaseUserId { get; set; } 
}
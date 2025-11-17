using Bifrost.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Bifrost.Infrastructure.Persistence;

public class BifrostDbContext : DbContext
{
    public BifrostDbContext(DbContextOptions<BifrostDbContext> options) : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<ApplicationNote> ApplicationNotes { get; set; }
    public DbSet<Preferences> Preferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BifrostDbContext).Assembly);
    }
}

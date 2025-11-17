using Bifrost.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bifrost.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : EntityConfiguration<JobApplication>
{
    public override void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        base.Configure(builder);
        builder.Property(ja => ja.Status).IsRequired();
        builder.Property(ja => ja.Created).IsRequired();
        builder.Property(ja => ja.Updated).IsRequired();
        builder.HasOne(ja => ja.Job)
            .WithOne(j => j.JobApplication)
            .HasForeignKey<JobApplication>(ja => ja.JobId);
        builder.HasMany(ja => ja.Notes)
            .WithOne(an => an.JobApplication)
            .HasForeignKey(an => an.JobApplicationId);
    }
}

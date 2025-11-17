using Bifrost.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bifrost.Infrastructure.Persistence.Configurations;

public class JobConfiguration : EntityConfiguration<Job>
{
    public override void Configure(EntityTypeBuilder<Job> builder)
    {
        base.Configure(builder);
        builder.Property(j => j.Title).IsRequired().HasMaxLength(200);
        builder.Property(j => j.Company).IsRequired().HasMaxLength(200);
        builder.Property(j => j.Location).HasMaxLength(200);
        builder.Property(j => j.PublicationLink).HasMaxLength(500);
        builder.Property(j => j.Description).IsRequired();
        builder.Property(j => j.JobType).IsRequired();
        builder.Property(j => j.OfferSponsorship).IsRequired();
        builder.Property(j => j.OfferRelocation).IsRequired();
    }
}
